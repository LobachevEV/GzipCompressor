using System;
using System.Diagnostics;
using GzipCompressor.Infrastructure.Logging;

namespace GzipCompressor.Infrastructure
{
    public static class StopwatchHelper
    {
        public static TimeSpan Time(Action action, Logger logger)
        {
            var stopWatch = Stopwatch.StartNew();
            action.Invoke();
            stopWatch.Stop();
            return stopWatch.Elapsed;
        }
    }
}