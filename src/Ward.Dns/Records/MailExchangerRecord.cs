using System;

using static Ward.Dns.Utils;

namespace Ward.Dns.Records
{
    public class MailExchangerRecord : Record
    {
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
        ) : base(name, type, @class, timeToLive, length, data) {
            var dataArray = data.ToArray();
            Preference = SwapUInt16(BitConverter.ToUInt16(dataArray, 0));
            var _ = 2;
            Hostname = ParseComplexName(message, dataArray, ref _);
        }

        public override string ToString() =>
            $"{Name}\t{TimeToLive}\t{Class}\t{Type}\t{Preference}\t{Hostname}";
    }
}
