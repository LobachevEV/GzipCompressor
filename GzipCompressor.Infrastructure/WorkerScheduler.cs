using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using GzipCompressor.Infrastructure.Logging;

namespace GzipCompressor.Infrastructure
{
    public class WorkerScheduler : IDisposable
    {
        private readonly Logger logger;
        private readonly WorkerPool pool;
        private readonly object sync = new object();

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
            Worker worker = null;
            lock (sync)
            {
                while (worker == null)
                {
                    worker = pool.GetWorker();
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

    internal class WorkerPool : IDisposable
    {
        private readonly int maxCount;
        private readonly object sync = new object();
        private readonly List<Worker> workers;


        public WorkerPool(int maxCount)
        {
            this.maxCount = maxCount;
            workers = new List<Worker>(maxCount);
        }

        public void Dispose()
        {
            lock (sync)
            {
                workers.ForEach(worker => worker.Dispose());
            }
        }

        public Worker GetWorker()
        {
            lock (sync)
            {
                var worker = workers.FirstOrDefault(w => w.IsWaiting);
                if (workers.Count == maxCount || worker != null)
                {
                    return worker;
                }

                worker = new Worker();
                workers.Add(worker);
                return worker;
            }
        }
    }
}