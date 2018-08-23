using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Timers;
using GzipCompressor.AdvanceCopier;
using GzipCompressor.BL;
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
            LogSettings.LogLevel = GetLogLevel();
            var logger = LogFactory.GetInstance().GetLogger<ConsoleLogger>();
            if (File.Exists(targetFilePath)) File.Delete(targetFilePath);


            Console.WriteLine($"Start {mode.ToLowerInvariant()}ing file {sourceFilePath}");
            using (var timer = new Timer {Interval = 500})
            {
                timer.Elapsed += (sender, eventArgs) => Console.Write(".");
                timer.Start();
                var compressor = new GzipCompressorFactory(logger, new WorkerScheduler(16, logger)).Get(mode);
                var time = StopwatchHelper.Time(() => compressor.Copy(sourceFilePath, targetFilePath), logger);
                timer.Stop();
                Console.WriteLine();
                Console.WriteLine($"{mode.ToLowerInvariant()}ing finished in {time}");
            }

            Console.WriteLine("Press Enter to continue");
            Console.ReadLine();
        }

        private static LogLevel GetLogLevel()
        {
            var strLogLevel = ConfigurationManager.AppSettings["BatchFile"];
            return !string.IsNullOrEmpty(strLogLevel)
                ? (LogLevel) Enum.Parse(typeof(LogLevel), strLogLevel)
                : LogLevel.Info;
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

    public class GzipCompressorFactory
    {
        private readonly Logger logger;
        private readonly WorkerScheduler workerScheduler;

        public GzipCompressorFactory(Logger logger, WorkerScheduler workerScheduler)
        {
            this.logger = logger;
            this.workerScheduler = workerScheduler;
        }

        public FileAdvanceCopier Get(string mode)
        {
            var processor = GetProcessor(mode);
            var reader = GetReader(mode);
            return new FileAdvanceCopier(reader, processor, new OrderingWriter(logger), logger);
        }

        private IProcessor GetProcessor(string mode)
        {
            switch (mode.ToLowerInvariant())
            {
                case "compress":
                    return new Compressor(workerScheduler, logger);
                case "decompress":
                    return new Decompressor(workerScheduler, logger);
                default:
                    throw new ArgumentException(
                        "The mode is incorrect. Please choose one of the following options: compress, decompress.");
            }
        }

        private IStreamReader GetReader(string mode)
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
    }
}