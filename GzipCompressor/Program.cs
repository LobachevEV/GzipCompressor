using System;
using System.Configuration;
using System.IO;
using System.Timers;
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
            LogSettings.LogLevel = GetLogLevel();
            if (File.Exists(targetFilePath)) File.Delete(targetFilePath);


            Console.WriteLine($"Start {mode.ToLowerInvariant()}ing file {sourceFilePath}");
            var progressBar = new AnimatedBar();
            using (var timer = new Timer {Interval = 500})
            {
                timer.Elapsed += (sender, eventArgs) => progressBar.Step();
                timer.Start();

                using (var pool = new WorkerPool(Math.Max(Environment.ProcessorCount * 2, 4)))
                {
                    var logger = LogFactory.GetInstance().GetLogger<ConsoleLogger>();
                    var compressor = new GzipCompressorFactory(logger, pool).Get(mode);
                    var time = StopwatchHelper.Time(() => compressor.Execute(sourceFilePath, targetFilePath), logger);
                    Console.WriteLine($"{mode.ToLowerInvariant()}ing finished in {time}");
                }

                timer.Stop();
            }

            Console.WriteLine("Press Enter to continue");
            Console.ReadLine();
        }

        private static LogLevel GetLogLevel()
        {
            var strLogLevel = ConfigurationManager.AppSettings["LogLevel"];
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
}