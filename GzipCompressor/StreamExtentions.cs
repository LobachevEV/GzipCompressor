using System.IO;
using System.Threading;

namespace GzipCompressor
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
                    var readBytesCount = bufferSize;
                    while (batchSize == 0 ? readBytesCount != 0 : readBytesCount < batchSize)
                    {
                        readBytesCount += source.Read(buffer, 0, buffer.Length);
                        queue.Add(buffer);
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