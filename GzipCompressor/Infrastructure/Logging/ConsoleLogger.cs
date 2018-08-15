using System;

namespace GzipCompressor.Infrastructure.Logging
{
    public class ConsoleLogger : Logger
    {
        protected override void Log(string level, string message)
        {
            Console.WriteLine($"{level}: {message}");
        }
    }
}