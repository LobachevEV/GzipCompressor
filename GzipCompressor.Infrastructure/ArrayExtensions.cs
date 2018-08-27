using System;
using System.Collections.Generic;

namespace GzipCompressor.Infrastructure
{
    public static class ArrayExtensions
    {
        public static IEnumerable<int> FindStartingIndexes(this byte[] array, byte[] subArray)
        {
            var result = new List<int>();
            var position = 0;
            while (true)
            {
                var index = Array.FindIndex(array, position, b => b == subArray[0]);
                if (index == -1 || array.Length - index < subArray.Length) return result;
                if (Check())
                {
                    result.Add(index);
                    position = index + subArray.Length;
                    continue;
                }

                position = index + 2;

                bool Check()
                {
                    for (var i = 1; i < subArray.Length; i++)
                        if (array[index + i] != subArray[i])
                            return false;

                    return true;
                }
            }
        }
    }
}