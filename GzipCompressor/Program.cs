using System;
using System.Diagnostics;
using System.IO;

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

            var compressor = new GzipComperssor();
            switch (mode.ToLowerInvariant())
            {
                case "compress":
                    var stopWatch = Stopwatch.StartNew();
                    compressor.Compress(sourceFilePath, targetFilePath);
                    stopWatch.Stop();
                    Console.WriteLine(stopWatch.ElapsedMilliseconds);
                    break;
                case "decompress":
                    compressor.Decompress(sourceFilePath, targetFilePath);
                    break;
                default:
                    throw new ArgumentException(
                        "Mode is incorrect. Please choose one of: compress, decompress.");
            }
        }

        private static void UnhandledExceptionTrapper(object sender, UnhandledExceptionEventArgs e)
        {
            Console.WriteLine(e.ExceptionObject.ToString());
            Console.WriteLine("Press Enter to continue");
            Console.ReadLine();
            Environment.Exit(1);
        }
    }
}