namespace GzipCompressor.AdvanceCopier
{
    public class IndexedBuffer
    {
        public IndexedBuffer(int index)
        {
            Index = index;
        }

        public int Index { get; }
        public byte[] Data { get; set; }
    }
}