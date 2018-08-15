using System.IO;
using System.Threading;

namespace GzipCompressor.Infrastructure
{
    public static class StreamExtentions
    {
        public static void CopyAsync(this Stream source, Stream target, long batchSize = 0)
        {
            using (var queue = new BoundedBlockingQueue<byte[]>(100))
            {
                var readThread = new Thread(() =>
                {
                    const int bufferSize = 16 * 1024;
                    var buffer = new byte[bufferSize];
                    var currentBatchSize = 0;
                    while (batchSize == 0 || currentBatchSize < batchSize)
                    {
                        var readBytes = source.Read(buffer, 0, buffer.Length);
                        if (readBytes == 0) break;
                        queue.Add(buffer);
                        currentBatchSize += readBytes;
                    }

                    queue.CompleteAdding();
                });
                readThread.Start();

                var writeThread = new Thread(() =>
                {
                    foreach (var bytese in queue.Consume()) target.Write(bytese, 0, bytese.Length);
                });
                writeThread.Start();
                writeThread.Join();
            }
        }
    }
}