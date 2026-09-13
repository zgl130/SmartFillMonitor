using SmartFillMonitor.Models;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SmartFillMonitor.Services
{
    public class ConfigServices
    {
        private const string SettingsFileName = "devicesettings.json";

        private static readonly SemaphoreSlim IoLock = new SemaphoreSlim(1, 1);

        public static string GetSettingsPath() => Path.Combine(AppContext.BaseDirectory, SettingsFileName);
        //也可以定义为属性，这里是只读属性简化
        //public static string SettingsPath => Path.Combine(AppContext.BaseDirectory, SettingsFileName);
        //////这并不是lambda表达式！！！
        ////最原始的写法：普通方法 + return
        //public static string GetSettingsPath()
        //{
        //    return Path.Combine(AppContext.BaseDirectory, SettingsFileName);
        //}
        ////方法体只有一句 return → 压缩成表达式体方法（C# 6）
        ////   规则：{ return X; }  ⟹  => X;
        //public static string GetSettingsPath() => Path.Combine(AppContext.BaseDirectory, SettingsFileName);

        ////换成属性的传统写法：get 访问器里 return
        //public static string SettingsPath { get { return Path.Combine(AppContext.BaseDirectory, SettingsFileName); } }

        //// get 里只有一句 return → 压缩成表达式体属性（C# 6），get 都省了
        //public static string SettingsPath => Path.Combine(AppContext.BaseDirectory, SettingsFileName);
        //以上均为语法糖

        public static async Task<DeviceSettings> LoadDeviceSettingAsync()
        {
            var path = GetSettingsPath();
            DeviceSettings settings = null;
            await IoLock.WaitAsync();//进入锁临界区，等待获取IO，确保只有一个线程进行文件操作

            try
            {
                if (File.Exists(path))
                {
                    try
                    {
                        var jsontext = await File.ReadAllTextAsync(path);
                        var opt = new JsonSerializerOptions()
                        {
                            PropertyNameCaseInsensitive = true,
                        };//设置属性大小写区分，提供稳定性

                        settings = JsonSerializer.Deserialize<DeviceSettings>(jsontext, opt);
                        if (settings != null)
                        {
                            return settings;
                        }
                    }
                    catch (JsonException jsonEx)
                    {
                        BackCorruptFile(path);//备份已经损坏的文件
                    }
                    catch (Exception Ex)
                    {
                        throw Ex;
                    }
                }
                else
                {
                    //日志提示文件不存在
                }
            }
            finally
            {
                IoLock.Release();//释放IO锁，允许其他线程进入
            }
            if (settings == null)
            {
                //返回默认配置
                settings = new DeviceSettings();
                //保存方法
                await SaveDeviceSettingAsync(settings);
            }
            return settings;

        }

        public static async Task<bool> SaveDeviceSettingAsync(DeviceSettings settings)
        {
            if (settings == null) return false;
            var path = GetSettingsPath();
            var tempPath = path + ".tmp";//临时文件路径
            await IoLock.WaitAsync();//等待获取IO锁，如果IO锁在有活跃线程的话
            try
            {
                var jsonText = JsonSerializer.Serialize(settings, new JsonSerializerOptions() { WriteIndented = true, });//格式缩进
                await File.WriteAllTextAsync(tempPath, jsonText);
                File.Move(tempPath, path,true);//覆盖写入
                //提示日志
                return true;
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                IoLock.Release();//释放IO锁
                if (File.Exists(tempPath))
                {
                    try
                    {
                        File.Delete(tempPath);
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                }
            }
        }

        public static void BackCorruptFile(string originalPath)
        {
            try
            {
                //备份路径
                var backupPath = originalPath + ".corrupt." + DateTime.Now.ToString("yyyyMMddHHmmSS");
                File.Copy(originalPath, backupPath, true);//拷贝备份
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
