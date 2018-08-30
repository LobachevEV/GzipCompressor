using System;
using System.Collections.Generic;
using System.IO;
using GzipCompressor.Infrastructure;
using GzipCompressor.Infrastructure.Logging;

namespace GzipCompressor.AdvanceCopier
{
    public class OrderingWriter : IQueueToStreamWriter<IndexedBuffer>
    {
        private readonly Logger logger;

        public OrderingWriter(Logger logger)
        {
            this.logger = logger;
        }

        public void Write(BoundedBlockingQueue<IndexedBuffer> source, Stream target)
        {
            logger.Debug("Writing started");
            var currentIndex = 0;
            foreach (var buffer in source.Consume())
            {
                var bufferIndex = buffer.Index;
                if (currentIndex == bufferIndex)
                {
                    WriteInternal(buffer, target);
                    currentIndex++;
                    continue;
                }

                if (bufferIndex > currentIndex) source.Add(buffer);
            }
        }

        private void WriteInternal(IndexedBuffer buffer, Stream target)
        {
            var dataLength = buffer.Data.Length;
            if (dataLength == 0)
                return;

            target.Write(buffer.Data, 0, dataLength);
            target.Flush();
            GC.Collect();
            logger.Debug($"Written {buffer.Index} buffer {dataLength} bytes");
        }
    }
}