using System;
using System.Collections.Generic;
using System.IO;
using GzipComressor.Infrastructure;

namespace GzipCompressor.AdvanceCopier
{
    public class OrderingWriter : IQueueToStreamWriter<IndexedBuffer>
    {
        public void Write(BoundedBlockingQueue<IndexedBuffer> source, Stream target)
        {
            var awaitDict = new Dictionary<int, IndexedBuffer>();
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

                if (bufferIndex > currentIndex) awaitDict[bufferIndex] = buffer;
            }

            while (awaitDict.ContainsKey(currentIndex))
            {
                WriteInternal(awaitDict[currentIndex], target);
                currentIndex++;
            }
        }
        
        private void WriteInternal(IndexedBuffer buffer, Stream target)
        {
            if (buffer.Data.Length == 0)
                return;

            target.Write(buffer.Data, 0, buffer.Data.Length);
            target.Flush();
            GC.Collect();
        }
    }
}