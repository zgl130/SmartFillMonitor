using Serilog;
using System;

namespace SmartFillMonitor.Services.Logs
{
    public static class LogServices
    {
        public static void Info(string message) => Log.Information(message);

        public static void Warn(string message) => Log.Warning(message);

        public static void Debug(string message) => Log.Debug(message);

        public static void Verbose(string message) => Log.Verbose(message);

        public static void Fatal(string message) => Log.Fatal(message);

        public static void Fatal(string message, Exception ex) => Log.Fatal(ex, message);
        public static void Error(string message) => Log.Error(message);
        public static void Error(string message, Exception ex) => Log.Error(message);
    }
}
