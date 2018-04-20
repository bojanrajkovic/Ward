using System;

using static Ward.Dns.Utils;

namespace Ward.Dns.Records
{
    public class NsRecord : Record
    {
        public string Hostname { get; }

        public NsRecord (
            string name,
            Dns.Type type,
            Class @class,
            uint timeToLive,
            ushort length,
            ReadOnlyMemory<byte> data,
            byte[] message
        ) : base(name, type, @class, timeToLive, length, data) {
            var _ = 0;
            Hostname = ParseComplexName(message, data.ToArray(), ref _);
        }

        public override string ToString() => $"{Name}\t{TimeToLive}\t{Class}\t{Type}\t{Hostname}";
    }
}
