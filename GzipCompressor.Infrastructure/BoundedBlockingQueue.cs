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

        public BoundedBlockingQueue(int boundedCapacity)
        {
            nonFullQueueSemaphore = new Semaphore(boundedCapacity, boundedCapacity);
        }

        public bool AddingCompleted { get; private set; }

        public void Dispose()
        {
            nonEmptyQueueSemaphore.Close();
            nonFullQueueSemaphore.Close();
            LogFactory.GetInstance().GetLogger<ConsoleLogger>().Debug("Queue disposed");
        }

        public void CompleteAdding()
        {
            nonEmptyQueueSemaphore.Release();
            AddingCompleted = true;
            LogFactory.GetInstance().GetLogger<ConsoleLogger>().Debug("Queue complete");
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
            if (!TryTake(out var item)) throw new InvalidOperationException();

            return item;
        }

        public IEnumerable<T> Consume()
        {
            while (TryTake(out var element)) yield return element;
        }

        private bool TryTake(out T result)
        {
            result = default(T);

            if (!AddingCompleted)
                try
                {
                    nonEmptyQueueSemaphore.WaitOne();
                }
                catch (ObjectDisposedException)
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