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
            var i = 0;
            var waitHandles = new List<WaitHandle>();
            foreach (var buffer in source.Consume())
            {
                var indexedBuffer = new IndexedBuffer(i);
                ManualResetEvent waitHandle = null;
                if (source.AddingCompleted)
                {
                    waitHandle = new ManualResetEvent(false);
                    waitHandles.Add(waitHandle);
                }
                i++;
                scheduler.StartNew(() =>
                {
                    indexedBuffer.Data = ProcessInternal(buffer);
                    target.Add(indexedBuffer);
                    Logger.Debug($"Processed {indexedBuffer.Index}");

                }, waitHandle: waitHandle);
            }

            Logger.Debug($"Consuming finished, wait {waitHandles.Count}");

            WaitHandle.WaitAll(waitHandles.ToArray());
            target.CompleteAdding();
        }

        protected abstract byte[] ProcessInternal(byte[] buffer);
    }
}