using System.IO;
using System.Threading;
using GzipCompressor.Infrastructure;
using GzipCompressor.Infrastructure.Logging;

namespace GzipCompressor.AdvanceCopier
{
    public class StreamAdvanceCopier
    {
        private readonly Logger logger;
        private readonly IProcessor processor;
        private readonly IStreamReader reader;
        private readonly WorkerScheduler scheduler;
        private readonly IQueueToStreamWriter<IndexedBuffer> writer;

        public StreamAdvanceCopier(IStreamReader reader, IProcessor processor,
            IQueueToStreamWriter<IndexedBuffer> writer, Logger logger, WorkerScheduler scheduler)
        {
            this.processor = processor;
            this.logger = logger;
            this.scheduler = scheduler;
            this.writer = writer;
            this.reader = reader;
        }

        public void Copy(Stream source, Stream target)
        {
            using (var readQueue = new BoundedBlockingQueue<byte[]>(100))
            {
                scheduler.StartNew(() => reader.Read(source, readQueue), readQueue.CompleteAdding);
                using (var processedQueue = new BoundedBlockingQueue<IndexedBuffer>(100))
                {
                    var writeWaitHandle = new AutoResetEvent(false);
                    scheduler.StartNew(() => writer.Write(processedQueue, target), waitHandle: writeWaitHandle);
                    scheduler.StartNew(() => processor.Process(readQueue, processedQueue), processedQueue.CompleteAdding);
                    WaitHandle.WaitAll(new WaitHandle[] {writeWaitHandle});
                }
            }
        }
    }
}