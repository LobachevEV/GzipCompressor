using System;
using System.Collections.Generic;

namespace GzipCompressor.Infrastructure
{
    public class WorkerPool : IDisposable
    {
        private readonly Queue<int> availableWorkers;
        private readonly int maxCount;
        private readonly Dictionary<int, Worker> workers;

        public WorkerPool(int maxCount)
        {
            this.maxCount = maxCount;
            workers = new Dictionary<int, Worker>(maxCount);
            availableWorkers = new Queue<int>(maxCount);
        }

        public void Dispose()
        {
            lock (workers)
            {
                foreach (var worker in workers.Values) worker.Dispose();
            }
        }

        public bool TryGetWorker(out Worker worker)
        {
            worker = GetWorker();
            return worker != null;
        }

        public Worker GetWorker()
        {
            if (TryGetAvailableWorker(out var worker)) return worker;

            if (workers.Count == maxCount) return null;

            lock (workers)
            {
                if (workers.Count == maxCount) return null;

                var workerId = workers.Count + 1;
                worker = new Worker {ManagedId = workerId};
                worker.OnComplete += () => availableWorkers.Enqueue(workerId);
                workers.Add(workerId, worker);
                return worker;
            }
        }

        private bool TryGetAvailableWorker(out Worker worker)
        {
            worker = null;
            if (availableWorkers.Count == 0) return false;

            lock (availableWorkers)
            {
                if (availableWorkers.Count == 0) return false;

                var workerId = availableWorkers.Dequeue();
                worker = workers[workerId];
                return true;
            }
        }
    }
}