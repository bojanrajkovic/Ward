using System;
using System.Net;

namespace Ward.Dns.Records
{
    public readonly struct AddressRecord : IRecord
    {
        public IPAddress Address { get; }
        public string Name { get; }
        public Type Type { get; }
        public Class Class { get; }
        public uint TimeToLive { get; }
        public ushort Length { get; }
        public ReadOnlyMemory<byte> Data { get; }

        public AddressRecord(
            string name,
            Type type,
            Class @class,
            uint timeToLive,
            ushort length,
            ReadOnlyMemory<byte> data
        ) {
            Name = name;
            Type = type;
            Class = @class;
            TimeToLive = timeToLive;
            Length = length;
            Data = data;
            Address = new IPAddress(data.ToArray());
        }

        public override string ToString() => $"{Name}\t{TimeToLive}\t{Class}\t{Type}\t{Address}";
    }
}
