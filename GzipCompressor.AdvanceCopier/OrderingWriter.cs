using System;
using System.Collections.Generic;
using System.IO;
using GzipComressor.Infrastructure;
using GzipComressor.Infrastructure.Logging;

namespace GzipCompressor.AdvanceCopier
{
    public class OrderingWriter : IQueueToStreamWriter<IndexedBuffer>
    {
        public void Write(BoundedBlockingQueue<IndexedBuffer> source, Stream target)
        {
            var awaitDict = new Dictionary<int, IndexedBuffer>();
            var currentIndex = 0;
            var scheduler = new WorkerScheduler(16, null);
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
            LogFactory.GetInstance().GetLogger<ConsoleLogger>().Debug($"Writed buffer {buffer.Index}");
            target.Flush();
            GC.Collect();
        }
    }
}