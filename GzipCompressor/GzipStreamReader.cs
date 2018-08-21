using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GzipCompressor.AdvanceCopier;
using GzipComressor.Infrastructure;

namespace GzipCompressor
{
    public class GzipStreamReader : IStreamReader
    {
        private readonly int bufferSize = 6 * 1024 * 1024;
        private readonly byte[] gzipHeader = {31, 139, 8, 0, 0, 0, 0, 0, 4, 0};
        private byte[] temp = new byte[0];

        public GzipStreamReader()
        {
        }

        public GzipStreamReader(int bufferSize)
        {
            this.bufferSize = bufferSize;
        }

        public void Read(Stream source, BoundedBlockingQueue<byte[]> target)
        {
            temp = new byte[0];
            while (true)
            {
                var buffers = ReadInternal(source, out var readBytes);
                if (readBytes == 0) break;

                foreach (var buffer in buffers) target.Add(buffer);
            }
            target.Add(temp);
        }

        private byte[][] ReadInternal(Stream source, out int readBytes)
        {
            var buffer = new byte[bufferSize];
            readBytes = source.Read(buffer, 0, buffer.Length);
            if (readBytes < bufferSize) Array.Resize(ref buffer, readBytes);

            var indexes = StartingIndexes(buffer, gzipHeader).ToList();
            if (!indexes.Any())
            {
                var newSize = temp.Length + buffer.Length;
                Array.Resize(ref temp, newSize);
                buffer.CopyTo(temp, 0);
                return new byte[][] { };
            }

            
            return indexes.Select((currentIndex, i) =>
            {
                if (indexes.Count == i + 1)
                {
                    temp = new byte[buffer.Length - currentIndex];
                    Array.Copy(buffer, currentIndex, temp, 0, temp.Length);
                    return null;
                }

                var nextIndex = indexes[i + 1];
                var toCopyCount = nextIndex - currentIndex;
                var result = new byte[temp.Length + toCopyCount];
                if (temp.Length != 0)
                {
                    Array.Copy(temp, 0, result, 0, temp.Length);
                    Array.Clear(temp, 0, 0);
                }

                Array.Copy(buffer, currentIndex, result, temp.Length, toCopyCount);
                
                return result;
            }).Where(bytes => bytes != null).ToArray();
        }

        private static IEnumerable<int> StartingIndexes(byte[] array, byte[] subArray)
        {
            var result = new List<int>();
            var position = 0;
            while (true)
            {
                var index = StartingIndex(array, subArray, position);
                switch (index)
                {
                    case -1:
                        return result;
                    case -2:
                        position++;
                        break;
                    default:
                        result.Add(index);
                        position = index + subArray.Length;
                        break;
                }
            }
        }
        
        private static int StartingIndex(byte[] array, byte[] subArray, int position)
        {
            var index = Array.FindIndex(array, position, b => b == subArray[0]);
            if (index == -1)
            {
                return -1;
            }

            if (array.Length - index < subArray.Length)
            {
                return -1;
            }

            bool Check()
            {
                for (var i = 1; i < subArray.Length; i++)
                {
                    if (array[index + i] != subArray[i])
                    {
                        return false;
                    }
                }

                return true;
            }

            return Check() ? index : -2;
        }
    }
}