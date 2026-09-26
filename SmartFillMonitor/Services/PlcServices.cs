using LiveCharts;
using Modbus.Device;
using SmartFillMonitor.Models;
using SmartFillMonitor.Services.Logs;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SmartFillMonitor.Services
{
    /// <summary>
    /// PLC通信服务，用于ModbusRTU串口通信读取数据
    /// 功能包括
    /// -管理串口，连断开等操作
    /// -周期性轮询PLC数据并且通过事件公布
    /// -提供一些命名接口给上层调用
    /// </summary>
    public static class PlcServices
    {
        private static SerialPort? _serialPort;
        private static IModbusSerialMaster modbusMaster;
        private static CancellationTokenSource _cts;
        private static readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);
        private const byte SlaveId = 1;
        public static bool IsConnected => _serialPort != null && _serialPort.IsOpen;

        public static event EventHandler<DeviceState>? DataReceviced;
        public static event EventHandler<bool> ConnectionChanged;

        public static async Task Initialize(DeviceSettings settings)
        {
            //先断开再连接
            await DisConnectAsync();
            _serialPort = new SerialPort()
            {
                PortName = settings.PortName,
                BaudRate = settings.BaudRate,
                DataBits = settings.DataBits,
                Parity = ParseParity(settings.Parity),
                StopBits = ParseStopBits(settings.StopBits)
            };
            await ConnectAsync();
        }


        private static Parity ParseParity(string parityStr) => Enum.TryParse<Parity>(parityStr, out var parity) ? parity : Parity.None;
        private static StopBits ParseStopBits(string stopBitsStr) => Enum.TryParse<StopBits>(stopBitsStr, out var stopBits) ? stopBits : StopBits.One;


        public static async Task ConnectAsync()
        {
            if (_serialPort == null) return;
            if (IsConnected) return;
            try
            {
                _serialPort.Open();
                modbusMaster = ModbusSerialMaster.CreateRtu(_serialPort);
                modbusMaster.Transport.ReadTimeout = 1000;
                modbusMaster.Transport.WriteTimeout = 10000;
                ConnectionChanged?.Invoke(modbusMaster, false);

                LogServices.Info($"PLC连接成功{_serialPort.PortName}");
                _cts = new CancellationTokenSource();
                _ = Task.Run(() => PollDataLoop(_cts.Token), _cts.Token);

            }
            catch (Exception ex)
            {
                ConnectionChanged?.Invoke(null, false);
                LogServices.Error($"断开串口失败{_serialPort?.PortName}", ex);
            }
            await Task.CompletedTask;
        }

        public static async Task DisConnectAsync()
        {
            _cts?.Cancel();
            await _lock.WaitAsync();
            try
            {
                if (_serialPort != null)
                {
                    if (_serialPort.IsOpen) _serialPort.Close();

                    _serialPort.Dispose();
                    _serialPort = null;
                }

                if (modbusMaster != null)
                {
                    modbusMaster.Dispose();
                    modbusMaster = null;
                }
            }
            finally
            {
                _lock.Release();
                ConnectionChanged?.Invoke(null, false);
            }
        }

        private static async Task PollDataLoop(CancellationToken token)
        {
            int errCount = 0;
            while (token.IsCancellationRequested)
            {
                try
                {
                    if (!IsConnected)
                    {
                        await Task.Delay(1000, token);
                        continue;
                    }
                    var state = await ReadStateAsync();
                    errCount = 0;
                    DataReceviced?.Invoke(null, state);
                    await Task.Delay(200, token);//200ms轮询
                }
                catch (OperationCanceledException ex)
                { break; }
                catch (Exception ex)
                {
                    errCount++;
                    if (errCount >= 3)
                    {
                        LogServices.Warn($"PLC通信异常{ex.Message}");
                        ConnectionChanged?.Invoke(null, false);
                        errCount = 0;
                    }
                    await Task.Delay(1000, token);
                }
            }
        }

        public static async Task<DeviceState> ReadStateAsync()
        {
            await _lock.WaitAsync();
            try
            {
                if (modbusMaster == null) throw new OperationCanceledException("未连接");
                ushort[] registers = await modbusMaster.ReadHoldingRegistersAsync(SlaveId, 0, 10);
                const ushort barcodeStart = 10;
                const ushort barcodeLength = 10;
                string barcode = string.Empty;
                try
                {
                    ushort[] barcodeRes = await modbusMaster.ReadHoldingRegistersAsync(SlaveId, barcodeStart, barcodeLength);
                    barcode = ConvertRegistersToString(barcodeRes);
                }
                catch (Exception ex)
                {
                    LogServices.Warn($"读取失败{ex.Message}");
                }
                return new DeviceState
                {
                    ActualCount = registers[ModbusConfigHelper.ActualCount],
                    TargetCount = registers[ModbusConfigHelper.TargetCount],
                    CurrentTemp = registers[ModbusConfigHelper.CurrentTemp],
                    SettingTemp = registers[ModbusConfigHelper.SettingTemp],
                    RunningTime = registers[ModbusConfigHelper.RunningTime],
                    CurrentCycleTime = registers[ModbusConfigHelper.CurrentCycleTime],
                    StandarCycleTime = registers[ModbusConfigHelper.StandarCycleTime],
                    LiquidLevel = registers[ModbusConfigHelper.LiquidLevel],
                    ValueOpen = registers[ModbusConfigHelper.ValueOpen]==1,
                    BarCode = barcode,
                };
            }
            catch (Exception ex)
            {
                LogServices.Warn($"读取失败{ex.Message}");
                return null;
            }
            finally { _lock.Release(); }
        }

        private static string ConvertRegistersToString(ushort[] registers)
        {
            //空检测
            if (registers == null || registers.Length == 0) return string.Empty;
            List<byte> bytes = new List<byte>();
            foreach (var reg in registers)
            {
                if (reg == 0) break;
                byte high = (byte)(reg >> 8);
                byte low = (byte)reg;

                if (high != 0) bytes.Add(high);
                if (low != 0) bytes.Add(low);
            }
            return Encoding.ASCII.GetString(bytes.ToArray()).Trim();
        }

        public static async Task WriteCommandAsync(string command, bool value)
        {
            ushort address = command == "Start" ? (ushort)1 : (ushort)2;
            await _lock.WaitAsync();
            try
            {
                if (modbusMaster == null) return;
                await modbusMaster.WriteSingleCoilAsync(SlaveId, address, value);
                LogServices.Info($"写入指令成功:{command}={value}");
            }
            catch (Exception ex)
            {
                LogServices.Error($"写入指令失败:{command}={value}", ex);
            }
            finally { _lock.Release(); }
        }

        /// <summary>
        /// 获取计算机可以串口列表
        /// </summary>
        public static string[] GetAvailablePorts => SerialPort.GetPortNames();

    }
}
