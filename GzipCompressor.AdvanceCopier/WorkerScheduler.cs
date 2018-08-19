using System;
using System.Threading;
using GzipComressor.Infrastructure.Logging;

namespace GzipCompressor.AdvanceCopier
{
    internal class WorkerScheduler
    {
        private readonly int maxCount;
        private int threadsCount;
        private readonly Logger logger;

        public WorkerScheduler(int maxCount, Logger logger)
        {
            this.maxCount = maxCount;
            this.logger = logger;
        }

        public void StartNew(Action action, Action callBack = null)
        {
            while (threadsCount >= maxCount) Thread.Sleep(100);

            new Worker(action, () =>
            {
                Interlocked.Decrement(ref threadsCount);
                callBack?.Invoke();
            }).Start();
            Interlocked.Increment(ref threadsCount);
            logger.Debug($"Started {threadsCount} thred");
        }

        public void WaitAll()
        {
            while (threadsCount > 0) Thread.Sleep(100);
        }
    }
}