namespace GzipCompressor.AdvanceCopier
{
    public interface IAdvanceCopierStrategy
    {
        byte[] Process(byte[] buffer);
    }
}