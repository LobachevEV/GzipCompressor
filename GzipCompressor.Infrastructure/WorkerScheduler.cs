using System;
using System.Threading;
using GzipCompressor.Infrastructure.Logging;

namespace GzipCompressor.Infrastructure
{
    public class WorkerScheduler
    {
        private readonly Logger logger;
        private readonly WorkerPool pool;

        public WorkerScheduler(WorkerPool pool, Logger logger)
        {
            this.logger = logger;
            this.pool = pool;
        }

        public void StartNew(Action action, EventWaitHandle waitHandle = null)
        {
            Worker worker;

            while (!pool.TryGetWorker(out worker)) Thread.Sleep(50);

            worker.Start(() =>
            {
                action.Invoke();
                waitHandle?.Set();
            });
        }
    }
}