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
            var i = 0;
            foreach (var buffer in source.Consume())
            {
                var indexedBuffer = new IndexedBuffer(i);
                i++;
                scheduler.StartNew(() =>
                {
                    indexedBuffer.Data = ProcessInternal(buffer);
                    target.Add(indexedBuffer);
                    Logger.Debug($"Processed buffer {indexedBuffer.Index}");
                });
            }

            scheduler.WaitAll();
        }

        protected abstract byte[] ProcessInternal(byte[] buffer);
    }
}