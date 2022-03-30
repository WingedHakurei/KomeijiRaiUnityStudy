using System;
using Serilog;

namespace Common
{
    public class Logger
    {
        /// <summary>
        /// 创建一个全局日志对象
        /// </summary>
        public static void Create(string logFileName)
        {
            warpLogger ??= new Logger(logFileName);
        }

        public static Serilog.Core.Logger Instance => coreLogger;

        private Logger(string logFileName)
        {
            coreLogger = new LoggerConfiguration().
                WriteTo.Console().
                WriteTo.File($"{logFileName}-.txt", rollingInterval: RollingInterval.Day).
                CreateLogger();
        }

        private static Serilog.Core.Logger coreLogger;

        private static Logger warpLogger;
    }
}
