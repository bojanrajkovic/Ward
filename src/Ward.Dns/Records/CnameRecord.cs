using System;

using static Ward.Dns.Utils;

namespace Ward.Dns.Records
{
    public class CnameRecord : Record
    {
        public string Hostname { get; }

        internal CnameRecord(
            string name,
            Class @class,
            uint timeToLive,
            ushort length,
            ReadOnlyMemory<byte> data,
            byte[] message
        ) : base(name, Dns.Type.CNAME, @class, timeToLive, length, data) {
            var _ = 0;
            Hostname = ParseComplexName(message, data.ToArray(), ref _);
        }

        public CnameRecord(
            string name,
            Class @class,
            uint timeToLive,
            string cname
        ) : base(name, Dns.Type.CNAME, @class, timeToLive, 0, Array.Empty<byte>()) {
            Hostname = cname;
        }

        [System.Diagnostics.DebuggerStepThrough]
        public override string ToString() =>
            $"{Name}\t{TimeToLive}\t{Class}\t{Type}\t{Hostname}";
    }
}
