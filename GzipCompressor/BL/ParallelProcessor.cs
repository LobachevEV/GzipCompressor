using System.Collections.Generic;
using System.Threading;
using GzipCompressor.AdvanceCopier;
using GzipCompressor.Infrastructure;
using GzipCompressor.Infrastructure.Logging;

namespace GzipCompressor.BL
{
    public abstract class ParallelProcessor : IProcessor
    {
        protected readonly Logger Logger;
        private readonly WorkerScheduler scheduler;

        protected ParallelProcessor(WorkerScheduler scheduler, Logger logger)
        {
            this.scheduler = scheduler;
            Logger = logger;
        }

        public void Process(BoundedBlockingQueue<byte[]> source, BoundedBlockingQueue<IndexedBuffer> target)
        {
            Logger.Debug("Processing started");
            var bufferFactory = new IndexedBufferFactory();
            var waitHandlesHelper = new EventWaitHandlesHelper();
            foreach (var buffer in source.Consume())
            {
                var waitHandle = source.AddingCompleted ? waitHandlesHelper.GetNew() : null;
                var indexedBuffer = bufferFactory.GetNext();
                scheduler.StartNew(() =>
                {
                    indexedBuffer.Data = ProcessInternal(buffer);
                    target.Add(indexedBuffer);
                    Logger.Debug($"Processed {indexedBuffer.Index}");
                }, waitHandle);
            }

            Logger.Debug("Consuming finished");
            waitHandlesHelper.WaitAll();
            target.CompleteAdding();
        }

        protected abstract byte[] ProcessInternal(byte[] buffer);

        private class IndexedBufferFactory
        {
            private int count;

            public IndexedBuffer GetNext()
            {
                var indexedBuffer = new IndexedBuffer(count++);
                return indexedBuffer;
            }

            public void Reset()
            {
                count = 0;
            }
        }

        private class EventWaitHandlesHelper
        {
            private readonly List<WaitHandle> waitHandles = new List<WaitHandle>();

            public EventWaitHandle GetNew()
            {
                var waitHandle = new ManualResetEvent(false);
                waitHandles.Add(waitHandle);
                return waitHandle;
            }

            public void WaitAll()
            {
                WaitHandle.WaitAll(waitHandles.ToArray());
            }
        }
    }
}