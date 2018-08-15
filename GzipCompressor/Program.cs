using System;
using GzipCompressor.Infrastructure;
using GzipCompressor.Infrastructure.Logging;

namespace GzipCompressor
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += UnhandledExceptionTrapper;
            InputValidator.Validate(args);
            var mode = args[0];
            var sourceFilePath = args[1];
            var targetFilePath = args[2];
            LogSettings.LogLevel = GetLogLevel(args);
            var logManager = LogFactory.GetInstance();
            var logger = logManager.GetLogger<ConsoleLogger>();
            var compressor = new GzipCompressor(logger);

            switch (mode.ToLowerInvariant())
            {
                case "compress":
                    StopwatchHelper.Time(() => compressor.Compress(sourceFilePath, targetFilePath), logger);
                    break;
                case "decompress":
                    StopwatchHelper.Time(() => compressor.Decompress(sourceFilePath, targetFilePath), logger);
                    break;
                default:
                    throw new ArgumentException(
                        "Mode is incorrect. Please choose one of: compress, decompress.");
            }
        }

        private static LogLevel GetLogLevel(string[] args)
        {
            for (var i = 0; i < args.Length; i++)
                if (args[i].Equals("-s", StringComparison.InvariantCulture) && i + 1 < args.Length)
                    return (LogLevel) Enum.Parse(typeof(LogLevel), args[i + 1]);
            return LogLevel.Info;
        }

        private static void UnhandledExceptionTrapper(object sender, UnhandledExceptionEventArgs e)
        {
            var logger = LogFactory.GetInstance().GetLogger<ConsoleLogger>();
            logger.Error(e.ExceptionObject.ToString());
            logger.Info("Press Enter to continue");
            Console.ReadLine();
            Environment.Exit(1);
        }
    }
}