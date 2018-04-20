using System;

using static Ward.Dns.Utils;

namespace Ward.Dns.Records
{
    public readonly struct NsRecord : IRecord
    {
        public string Name { get; }
        public Type Type { get; }
        public Class Class { get; }
        public uint TimeToLive { get; }
        public ushort Length { get; }
        public ReadOnlyMemory<byte> Data { get; }
        public string Hostname { get; }

        public NsRecord (
            string name,
            Dns.Type type,
            Class @class,
            uint timeToLive,
            ushort length,
            ReadOnlyMemory<byte> data,
            byte[] message
        ) {
            Name = name;
            Type = type;
            Class = @class;
            TimeToLive = timeToLive;
            Length = length;
            Data = data;

            var _ = 0;
            Hostname = ParseComplexName(message, data.ToArray(), ref _);
        }

        public override string ToString() => $"{Name}\t{TimeToLive}\t{Class}\t{Type}\t{Hostname}";
    }
}
