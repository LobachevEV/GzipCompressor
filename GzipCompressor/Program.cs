using System;
using System.Diagnostics;
using System.IO;

namespace GzipCompressor
{
    internal class Program
    {
        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += UnhandledExceptionTrapper;
            var mode = args[0];
            var sourceFile = args[1];
            var targetFile = args[2];

            if (!File.Exists(sourceFile))
            {
                Console.WriteLine("Source file does not exist");
                return;
            }

            var compressor = new GzipComperssor();
            if (mode.Equals("compress", StringComparison.OrdinalIgnoreCase))
            {
                var stopWatch = Stopwatch.StartNew();
                compressor.Compress(sourceFile, targetFile);
                stopWatch.Stop();
                Console.WriteLine(stopWatch.ElapsedMilliseconds);
            }
            else if (mode.Equals("decompress", StringComparison.OrdinalIgnoreCase))
            {
                compressor.Decompress(sourceFile, targetFile);
            }
        }
        
        static void UnhandledExceptionTrapper(object sender, UnhandledExceptionEventArgs e) {
            Console.WriteLine(e.ExceptionObject.ToString());
            Console.WriteLine("Press Enter to continue");
            Console.ReadLine();
            Environment.Exit(1);
        }
    }
}