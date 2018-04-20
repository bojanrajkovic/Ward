using System;
using System.Text;

using static Ward.Dns.Utils;

namespace Ward.Dns.Records
{
    public readonly struct TxtRecord : IRecord
    {
        public string Name { get; }
        public Type Type { get; }
        public Class Class { get; }
        public uint TimeToLive { get; }
        public ushort Length { get; }
        public ReadOnlyMemory<byte> Data { get; }
        public string TextData { get; }

        public unsafe TxtRecord (
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
            using (var pin = data.Pin())
                TextData = Encoding.ASCII.GetString((byte*)pin.Pointer + 1, *((byte*)pin.Pointer));
        }

        public override string ToString() =>
            $"{Name}\t{TimeToLive}\t{Class}\t{Type}\t{TextData}";
    }
}
