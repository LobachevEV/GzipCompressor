using System;
using System.Collections.Generic;

namespace GzipComressor.Infrastructure.Logging
{
    public static class LogSettings
    {
        public static LogLevel LogLevel { get; set; } = LogLevel.Info;
    }

    public class LogFactory
    {
        private static readonly object CreationLock = new object();
        private static LogFactory instance;
        private readonly object loggerCreationLock = new object();

        private readonly Dictionary<Type, Logger> loggers;

        private readonly LogLevel logLevel;

        private LogFactory()
        {
            logLevel = LogSettings.LogLevel;
            loggers = new Dictionary<Type, Logger>();
        }

        public static LogFactory GetInstance()
        {
            if (instance != null) return instance;
            lock (CreationLock)
            {
                instance = new LogFactory();
            }

            return instance;
        }

        public Logger GetLogger<T>() where T : Logger
        {
            var type = typeof(T);
            if (loggers.ContainsKey(type)) return loggers[type];

            lock (loggerCreationLock)
            {
                return loggers.ContainsKey(type) ? loggers[type] : CreateLogger<T>(type);
            }
        }

        private T CreateLogger<T>(Type type) where T : Logger
        {
            var logger = (T) Activator.CreateInstance(type, logLevel);
            loggers.Add(typeof(T), logger);
            return logger;
        }
    }
}