using System;
using System.Threading;
using GzipCompressor.Infrastructure.Logging;

namespace GzipCompressor.Infrastructure
{
    public class WorkerScheduler : IDisposable
    {
        private readonly Logger logger;
        private readonly WorkerPool pool;

        public WorkerScheduler(int maxCount, Logger logger)
        {
            this.logger = logger;
            pool = new WorkerPool(maxCount);
        }

        public void Dispose()
        {
            pool.Dispose();
        }

        public void StartNew(Action action, Action callBack = null, EventWaitHandle waitHandle = null)
        {
            Worker worker;

            while (!pool.TryGetWorker(out worker))
            {
                Thread.Sleep(50);
            }

            worker.Start(() =>
            {
                action.Invoke();
                callBack?.Invoke();
                waitHandle?.Set();
            });
        }
    }
}