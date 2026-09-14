using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFillMonitor.Services
{
    public class PlcServices
    {
        public static string[] GetAvailablePorts=>SerialPort.GetPortNames();

    }
}
