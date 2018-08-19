using System;
using System.Diagnostics;
using GzipComressor.Infrastructure.Logging;

namespace GzipComressor.Infrastructure
{
    public static class StopwatchHelper
    {
        public static void Time(Action action, Logger logger)
        {
            var stopWatch = Stopwatch.StartNew();
            action.Invoke();
            stopWatch.Stop();
            logger.Debug($"{stopWatch.Elapsed} ms");
        }
    }
}