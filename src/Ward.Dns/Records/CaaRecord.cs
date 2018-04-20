using System;
using System.Text;

using static Ward.Dns.Utils;

namespace Ward.Dns.Records
{
    public readonly struct CaaRecord : IRecord
    {
        public string Name { get; }
        public Type Type { get; }
        public Class Class { get; }
        public uint TimeToLive { get; }
        public ushort Length { get; }
        public ReadOnlyMemory<byte> Data { get; }
        public bool Critical { get; }
        public string Tag { get; }
        public string Value { get; }

        public unsafe CaaRecord (
            string name,
            Dns.Type type,
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

            // The top bit is set if the critical flag is true, all other
            // bit positions are reserved per RFC 6844.
            Critical = (data.Span[0] & 0b1000_0000) == 0b1000_0000;
            var tagLength = data.Span[1];

            var dataPointer = data.Pin().Pointer;
            Tag = Encoding.ASCII.GetString((byte*)dataPointer + 2, tagLength);
            Value = Encoding.ASCII.GetString((byte*)dataPointer + 2 + tagLength, (length - 2 - tagLength));
        }

        public override string ToString() =>
            $"{Name}\t{TimeToLive}\t{Class}\t{Type}\t{(Critical ? 1 : 0)} {Tag} {Value}";
    }
}
