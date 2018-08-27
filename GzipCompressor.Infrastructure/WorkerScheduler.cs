using System;
using System.Threading;
using GzipCompressor.Infrastructure.Logging;

namespace GzipCompressor.Infrastructure
{
    public class WorkerScheduler
    {
        private readonly Logger logger;
        private readonly int maxCount;
        private int threadsCount;
        private object sync = new object();

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