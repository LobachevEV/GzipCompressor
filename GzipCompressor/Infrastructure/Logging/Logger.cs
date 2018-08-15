namespace GzipCompressor.Infrastructure.Logging
{
    public abstract class Logger
    {
        protected Logger()
        {
            MinimumLogLevel = LogLevel.Info;
        }

        public LogLevel MinimumLogLevel { get; set; }

        public void Info(string message)
        {
            if (MinimumLogLevel <= LogLevel.Info) Log(LogLevel.Info.ToString(), message);
        }

        public void Debug(string message)
        {
            if (MinimumLogLevel <= LogLevel.Debug) Log(LogLevel.Info.ToString(), message);
        }

        public void Error(string message)
        {
            if (MinimumLogLevel <= LogLevel.Error) Log(LogLevel.Info.ToString(), message);
        }

        protected abstract void Log(string level, string message);
    }
}