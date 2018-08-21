using System;
using System.Threading;
using GzipComressor.Infrastructure.Logging;

namespace GzipComressor.Infrastructure
{
    public class WorkerScheduler
    {
        private readonly Logger logger;
        private readonly int maxCount;
        private int threadsCount;

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
        }

        public void WaitAll()
        {
            while (threadsCount > 0) Thread.Sleep(100);
        }
    }
}