using System;

using static Ward.Dns.Utils;

namespace Ward.Dns.Records
{
    public class NsRecord : Record
    {
        public string Hostname { get; }

        internal NsRecord(
            string name,
            Class @class,
            uint timeToLive,
            ushort length,
            ReadOnlyMemory<byte> data,
            byte[] message
        ) : base(name, Dns.Type.NS, @class, timeToLive, length, data) {
            var _ = 0;
            Hostname = ParseComplexName(message, data.ToArray(), ref _);
        }

        public NsRecord(
            string name,
            Class @class,
            uint timeToLive,
            string hostname
        ) : base(name, Dns.Type.NS, @class, timeToLive, 0, Array.Empty<byte>()) {
            Hostname = hostname;
        }

        [System.Diagnostics.DebuggerStepThrough]
        public override string ToString() => $"{Name}\t{TimeToLive}\t{Class}\t{Type}\t{Hostname}";
    }
}
