using System;
using System.Collections.Generic;
using System.Threading;
using GzipCompressor.Infrastructure.Logging;

namespace GzipCompressor.Infrastructure
{
    public class BoundedBlockingQueue<T> : IDisposable
    {
        private readonly Semaphore nonEmptyQueueSemaphore =
            new Semaphore(0, int.MaxValue);

        private readonly Semaphore nonFullQueueSemaphore;
        private readonly Queue<T> queue = new Queue<T>();

        public bool AddingCompleted { get; private set; }

        public BoundedBlockingQueue(int boundedCapacity)
        {
            nonFullQueueSemaphore = new Semaphore(boundedCapacity, boundedCapacity);
        }

        public void Dispose()
        {
            nonEmptyQueueSemaphore.Close();
            nonFullQueueSemaphore.Close();
            LogFactory.GetInstance().GetLogger<ConsoleLogger>().Debug($"Queue disposed");
        }

        public void CompleteAdding()
        {
            nonEmptyQueueSemaphore.Release();
            AddingCompleted = true;
            LogFactory.GetInstance().GetLogger<ConsoleLogger>().Debug($"Queue complete");
        }

        public void Add(T value)
        {
            nonFullQueueSemaphore.WaitOne();
            lock (queue)
            {
                queue.Enqueue(value);
            }

            nonEmptyQueueSemaphore.Release();
        }

        public T Take()
        {
            T item;
            if (!TryTake(out item)) throw new InvalidOperationException();

            return item;
        }

        public IEnumerable<T> Consume()
        {
            T element;
            while (TryTake(out element)) yield return element;
        }

        private bool TryTake(out T result)
        {
            result = default(T);

            if (!AddingCompleted)
                try
                {
                    nonEmptyQueueSemaphore.WaitOne();
                }
                catch (ObjectDisposedException e)
                {
                    return false;
                }

            lock (queue)
            {
                if (queue.Count == 0) return false;
                result = queue.Dequeue();
            }

            nonFullQueueSemaphore.Release();
            return true;
        }
    }
}