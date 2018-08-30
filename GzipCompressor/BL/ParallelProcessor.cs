using System.Collections.Generic;
using System.Linq;
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
                var waitHandle = new ManualResetEvent(false);
                i++;
                scheduler.StartNew(() =>
                {
                    indexedBuffer.Data = ProcessInternal(buffer);
                    target.Add(indexedBuffer);
                    waitHandle.Set();
                }, waitHandle: waitHandle);
                waitHandles.Add(waitHandle);
            }

            Logger.Debug($"Consuming finished, wait {waitHandles.Count}");

            WaitHandle.WaitAll(waitHandles.ToArray());
            target.CompleteAdding();
        }

        protected abstract byte[] ProcessInternal(byte[] buffer);
    }
}