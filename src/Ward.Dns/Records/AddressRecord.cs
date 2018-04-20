using System;
using System.Net;

namespace Ward.Dns.Records
{
    public class AddressRecord : Record
    {
        public IPAddress Address { get; }

        public AddressRecord(
            string name,
            Type type,
            Class @class,
            uint timeToLive,
            ushort length,
            ReadOnlyMemory<byte> data
        ) : base(name, type, @class, timeToLive, length, data) {
            Address = new IPAddress(data.ToArray());
        }

        [System.Diagnostics.DebuggerStepThrough]
        public override string ToString() => $"{Name}\t{TimeToLive}\t{Class}\t{Type}\t{Address}";
    }
}
