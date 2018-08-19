using System;
using System.Collections.Generic;
using System.Threading;

namespace GzipCompressor.AdvanceCopier
{
    public class BoundedBlockingQueue<T> : IDisposable
    {
        private readonly Semaphore nonEmptyQueueSemaphore =
            new Semaphore(0, int.MaxValue);

        private readonly Semaphore nonFullQueueSemaphore;
        private readonly Queue<T> queue = new Queue<T>();

        private bool completeAdding;

        public BoundedBlockingQueue(int boundedCapacity)
        {
            nonFullQueueSemaphore = new Semaphore(boundedCapacity, boundedCapacity);
        }

        public void Dispose()
        {
            nonEmptyQueueSemaphore.Close();
            nonFullQueueSemaphore.Close();
        }

        public void CompleteAdding()
        {
            completeAdding = true;
            nonEmptyQueueSemaphore.Release();
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

            if (!completeAdding)
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