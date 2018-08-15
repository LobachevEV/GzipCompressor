using System;
using System.Diagnostics;
using GzipCompressor.Infrastructure.Logging;

namespace GzipCompressor.Infrastructure
{
    public static class StopwatchHelper
    {
        public static void Time(Action action, Logger logger)
        {
            var stopWatch = Stopwatch.StartNew();
            action.Invoke();
            stopWatch.Stop();
            logger.Debug($"{stopWatch.ElapsedMilliseconds} ms");
        }
    }
}