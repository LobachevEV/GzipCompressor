using System;
using System.IO;
using GzipCompressor.AdvanceCopier;
using GzipComressor.Infrastructure;
using GzipComressor.Infrastructure.Logging;

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
            if (File.Exists(targetFilePath)) File.Delete(targetFilePath);

            var strategy = GetStrategy(mode);
            var fileAdvanceCopier = new FileAdvanceCopier(strategy, logger);
            StopwatchHelper.Time(() => fileAdvanceCopier.Copy(sourceFilePath, targetFilePath), logger);
        }

        private static IAdvanceCopierStrategy GetStrategy(string mode)
        {
            switch (mode.ToLowerInvariant())
            {
                case "compress":
                    return new GzipCompressStrategy();
                case "decompress":
                    return new GzipDecompressionStrategy();
                default:
                    throw new ArgumentException(
                        "Mode is incorrect. Please choose one of: compress, decompress.");
            }
        }

        private static LogLevel GetLogLevel(string[] args)
        {
            for (var i = 0; i < args.Length; i++)
                if (args[i].Equals("-l", StringComparison.InvariantCulture) && i + 1 < args.Length)
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