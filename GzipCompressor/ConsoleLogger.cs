﻿using System;
using GzipComressor.Infrastructure.Logging;

namespace GzipCompressor
{
    public class ConsoleLogger : Logger
    {
        protected override void Log(string level, string message)
        {
            Console.WriteLine($"{level}: {message}");
        }
    }
}