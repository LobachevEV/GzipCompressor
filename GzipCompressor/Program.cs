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

            var processor = GetProcessor(mode);
            var reader = GetReader(mode);
            var fileAdvanceCopier = new FileAdvanceCopier(reader, processor, new OrderingWriter(), logger);
            StopwatchHelper.Time(() => fileAdvanceCopier.Copy(sourceFilePath, targetFilePath), logger);
        }

        private static IProcessor GetProcessor(string mode)
        {
            var logger = LogFactory.GetInstance().GetLogger<ConsoleLogger>();
            var scheduler = new WorkerScheduler(16, logger);
            switch (mode.ToLowerInvariant())
            {
                case "compress":
                    return new GzipCompressor(scheduler, logger);
                case "decompress":
                    return new GzipDecompressor(scheduler, logger);
                default:
                    throw new ArgumentException(
                        "The mode is incorrect. Please choose one of the following options: compress, decompress.");
            }
        }

        private static IStreamReader GetReader(string mode)
        {
            switch (mode.ToLowerInvariant())
            {
                case "compress":
                    return new DefaultStreamReader();
                case "decompress":
                    return new GzipStreamReader();
                default:
                    throw new ArgumentException(
                        "The mode is incorrect. Please choose one of the following options: compress, decompress.");
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
            Console.WriteLine("Press Enter to continue");
            Console.ReadLine();
            Environment.Exit(1);
        }
    }
}