namespace Aelfweard.Dns
{
    public class Record
    {
        public Name Name { get; }
        public Type Type { get; }
        public Class Class { get; }
        public int TimeToLive { get; }
        public short Length { get; }
        public byte[] Data { get; }
    }
}
