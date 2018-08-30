using System;

namespace GzipCompressor.Infrastructure.Logging
{
    public class ConsoleLogger : Logger
    {
        public ConsoleLogger(LogLevel level) : base(level)
        {
        }

        protected override void Log(string level, string message)
        {
            Console.WriteLine($"{level}: {message}");
        }
    }
}