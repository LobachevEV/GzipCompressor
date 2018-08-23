namespace GzipComressor.Infrastructure.Logging
{
    public abstract class Logger
    {
        private readonly LogLevel minimumLogLevel;

        protected Logger()
        {
            minimumLogLevel = LogLevel.Info;
        }

        protected Logger(LogLevel level)
        {
            minimumLogLevel = level;
        }

        public void Info(string message)
        {
            if (minimumLogLevel <= LogLevel.Info) Log(LogLevel.Info.ToString(), message);
        }

        public void Debug(string message)
        {
            if (minimumLogLevel <= LogLevel.Debug) Log(LogLevel.Debug.ToString(), message);
        }

        public void Error(string message)
        {
            if (minimumLogLevel <= LogLevel.Error) Log(LogLevel.Error.ToString(), message);
        }

        protected abstract void Log(string level, string message);
    }
}