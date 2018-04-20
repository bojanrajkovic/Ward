using System;

using static Ward.Dns.Utils;

namespace Ward.Dns.Records
{
    public readonly struct MailExchangerRecord : IRecord
    {
        public string Name { get; }
        public Type Type { get; }
        public Class Class { get; }
        public uint TimeToLive { get; }
        public ushort Length { get; }
        public ReadOnlyMemory<byte> Data { get; }
        public ushort Preference { get; }
        public string Hostname { get; }

        public unsafe MailExchangerRecord (
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

            var dataArray = data.ToArray();
            Preference = SwapUInt16(BitConverter.ToUInt16(dataArray, 0));
            var _ = 2;
            Hostname = ParseComplexName(message, dataArray, ref _);
        }

        public override string ToString() =>
            $"{Name}\t{TimeToLive}\t{Class}\t{Type}\t{Preference}\t{Hostname}";
    }
}
