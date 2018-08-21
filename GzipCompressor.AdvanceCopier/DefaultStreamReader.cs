using System;
using System.IO;
using GzipComressor.Infrastructure;

namespace GzipCompressor.AdvanceCopier
{
    public class DefaultStreamReader : IStreamReader
    {
        private readonly int bufferSize = 42 * 1024;

        public DefaultStreamReader()
        {
        }

        public DefaultStreamReader(int bufferSize)
        {
            this.bufferSize = bufferSize;
        }

        public void Read(Stream source, BoundedBlockingQueue<byte[]> target)
        {
            while (true)
            {
                var buffer = new byte[bufferSize];
                var readedBytes = source.Read(buffer, 0, buffer.Length);
                if (readedBytes == 0) break;

                if (readedBytes < bufferSize) Array.Resize(ref buffer, readedBytes);
                target.Add(buffer);
            }
        }
    }
}