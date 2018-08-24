using System;
using System.IO;
using GzipCompressor.Infrastructure;

namespace GzipCompressor.AdvanceCopier
{
    public class DefaultStreamReader : IStreamReader
    {
        private readonly int bufferSize = 16 * 1024 * 1024;

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
                var readBytes = source.Read(buffer, 0, buffer.Length);
                if (readBytes == 0) break;

                if (readBytes < bufferSize) Array.Resize(ref buffer, readBytes);
                target.Add(buffer);
            }
        }
    }
}