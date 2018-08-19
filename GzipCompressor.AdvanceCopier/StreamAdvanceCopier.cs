using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using GzipComressor.Infrastructure.Logging;

namespace GzipCompressor.AdvanceCopier
{
    public class StreamAdvanceCopier
    {
        private readonly WorkerScheduler scheduler;
        private readonly IAdvanceCopierStrategy strategy;
        private readonly Logger logger;

        public StreamAdvanceCopier(IAdvanceCopierStrategy strategy, Logger logger)
        {
            this.strategy = strategy;
            this.logger = logger;
            scheduler = new WorkerScheduler(4, logger);
        }

        public void Copy(Stream source, Stream target)
        {
            using (var readQueue = new BoundedBlockingQueue<byte[]>(100))
            {
                scheduler.StartNew(() => ReadToQueue(source, readQueue), readQueue.CompleteAdding);
                using (var processedQueue = new BoundedBlockingQueue<byte[]>(100))
                {
                    scheduler.StartNew(() =>
                    {
                        var workerPool = new WorkerScheduler(16, logger);
                        foreach (var buffer in readQueue.Consume())
                        {
                            workerPool.StartNew(() => processedQueue.Add(strategy.Process(buffer)));
                        }

                        workerPool.WaitAll();
                    }, processedQueue.CompleteAdding);
                    scheduler.StartNew(() => { WriteToFileAsync(target, processedQueue); });
                    scheduler.WaitAll();
                }
            }
        }

        private void ReadToQueue(Stream source, BoundedBlockingQueue<byte[]> queue)
        {
            var readedBytes = 1;
            const int bufferSize = 1024 * 1024;
            while (readedBytes > 0)
            {
                var buffer = new byte[bufferSize];
                readedBytes = source.Read(buffer, 0, buffer.Length);
                queue.Add(buffer);
            }
        }

        private void WriteToFileAsync(Stream target, BoundedBlockingQueue<byte[]> compressedQueue)
        {
            foreach (var buffer in compressedQueue.Consume())
            {
                target.Write(buffer, 0, buffer.Length);
                target.Flush();
                GC.Collect();
            }
        }
    }
}