using System;
using System.Collections.Generic;
using System.IO;
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
                using (var processedQueue = new BoundedBlockingQueue<IndexedBuffer>(100))
                {
                    scheduler.StartNew(() =>
                    {
                        var workerPool = new WorkerScheduler(16, logger);
                        var i = 0;
                        foreach (var buffer in readQueue.Consume())
                        {
                            var indexedBuffer = new IndexedBuffer(i);
                            i++;
                            workerPool.StartNew(() =>
                            {
                                indexedBuffer.Data = strategy.Process(buffer);
                                processedQueue.Add(indexedBuffer);
                            });
                        }

                        workerPool.WaitAll();
                    }, processedQueue.CompleteAdding);
                    scheduler.StartNew(() => WriteToFile(processedQueue, target));
                    scheduler.WaitAll();
                }
            }
        }

        private void ReadToQueue(Stream source, BoundedBlockingQueue<byte[]> queue)
        {
            var readedBytes = 1;
            const int bufferSize = 6 * 1024 * 1024;
            while (readedBytes > 0)
            {
                var buffer = new byte[bufferSize];
                readedBytes = source.Read(buffer, 0, buffer.Length);
                queue.Add(buffer);
            }
        }

        private void WriteToFile(BoundedBlockingQueue<IndexedBuffer> source, Stream target)
        {
            var awaitDict = new Dictionary<int, IndexedBuffer>();
            var currentIndex = 0;
            foreach (var buffer in source.Consume())
            {
                var bufferIndex = buffer.Index;
                if (currentIndex == bufferIndex)
                {
                    Write( buffer, target);
                    currentIndex++;
                    continue;
                }

                if (bufferIndex > currentIndex)
                {
                    awaitDict[bufferIndex] = buffer;
                }
            }

            while (awaitDict.ContainsKey(currentIndex))
            {
                Write(awaitDict[currentIndex], target);
                currentIndex++;
            }
        }

        private void Write(IndexedBuffer buffer, Stream target)
        {
            logger.Debug($"Write {buffer.Index} buffer");
            if (buffer.Data.Length == 0)
                return;

            target.Write(buffer.Data, 0, buffer.Data.Length);
            target.Flush();
            GC.Collect();
        }

        private class IndexedBuffer
        {
            public IndexedBuffer(int index, byte[] data)
            {
                Index = index;
                Data = data;
            }

            public IndexedBuffer(int index)
            {
                Index = index;
            }

            public int Index { get; set; }
            public byte[] Data { get; set; }
        }
    }
}