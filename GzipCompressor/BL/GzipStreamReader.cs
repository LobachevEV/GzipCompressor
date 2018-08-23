using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GzipCompressor.AdvanceCopier;
using GzipComressor.Infrastructure;
using GzipComressor.Infrastructure.Logging;

namespace GzipCompressor.BL
{
    public class GzipStreamReader : IStreamReader
    {
        private readonly int bufferSize = 512 * 1024;
        private readonly byte[] gzipHeader = {31, 139, 8, 0, 0, 0, 0, 0, 4, 0};
        private readonly List<byte> temp = new List<byte>();

        public GzipStreamReader()
        {
        }

        public GzipStreamReader(int bufferSize)
        {
            this.bufferSize = bufferSize;
        }

        public void Read(Stream source, BoundedBlockingQueue<byte[]> target)
        {
            var logger = LogFactory.GetInstance().GetLogger<ConsoleLogger>();
            while (true)
            {
                
                var buffer = ReadInternal(source, out var readBytes);
                if (readBytes == 0) break;
                if (buffer.Length == 0) continue;
                target.Add(buffer);
                logger.Debug($"Read bytes {buffer.Length}");
            }

            if (temp.Count != 0)
            {
                target.Add(temp.ToArray());
                logger.Debug($"Read bytes (temp) {temp.Count}");
            }
        }

        private byte[] ReadInternal(Stream source, out int readBytes)
        {
            var buffer = new byte[bufferSize];
            readBytes = source.Read(buffer, 0, buffer.Length);
            if (readBytes == 0) return new byte[] { };
            if (readBytes == source.Length) return buffer;
            if (readBytes < bufferSize) Array.Resize(ref buffer, readBytes);


            var indexes = buffer.FindStartingIndexes(gzipHeader).ToList();
            if (!indexes.Any())
            {
                CopyToList(buffer, temp, buffer.Length, 0);
                return new byte[] { };
            }

            var lastIndex = indexes.Last();
            var result = InitResult(lastIndex);
            CopyToList(buffer, result, lastIndex);
            CopyToList(buffer, temp, buffer.Length - lastIndex, lastIndex);
            return result.ToArray();
        }

        private List<byte> InitResult(int lastIndex)
        {
            List<byte> result;
            if (temp.Count != 0)
            {
                result = new List<byte>(temp);
                temp.Clear();
            }
            else
            {
                result = new List<byte>(lastIndex);
            }

            return result;
        }

        private void CopyToList(byte[] buffer, List<byte> dest, int length, int offset = 0)
        {
            var array = new byte[length];
            Array.Copy(buffer, offset, array, 0, length);
            dest.AddRange(array);
        }
    }
}