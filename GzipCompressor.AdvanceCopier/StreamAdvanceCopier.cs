using System.Collections.Generic;
using System.IO;
using GzipComressor.Infrastructure;
using GzipComressor.Infrastructure.Logging;

namespace GzipCompressor.AdvanceCopier
{
    public class StreamAdvanceCopier
    {
        private readonly Logger logger;
        private readonly WorkerScheduler scheduler;
        private readonly IProcessor processor;
        private readonly IStreamReader reader;
        private readonly IQueueToStreamWriter<IndexedBuffer> writer;

        public StreamAdvanceCopier(IStreamReader reader, IProcessor processor,
            IQueueToStreamWriter<IndexedBuffer> writer, Logger logger)
        {
            this.processor = processor;
            this.logger = logger;
            this.writer = writer;
            this.reader = reader;
            scheduler = new WorkerScheduler(4, logger);
        }

        public void Copy(Stream source, Stream target)
        {
            using (var readQueue = new BoundedBlockingQueue<byte[]>(100))
            {
                scheduler.StartNew(() => reader.Read(source, readQueue), readQueue.CompleteAdding);
                using (var processedQueue = new BoundedBlockingQueue<IndexedBuffer>(100))
                {
                    scheduler.StartNew(() => processor.Process(readQueue, processedQueue),
                        processedQueue.CompleteAdding);
                    scheduler.StartNew(() => writer.Write(processedQueue, target));
                    scheduler.WaitAll();
                }
            }
        }
    }
}