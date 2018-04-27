using System;
using System.Net;

namespace Ward.Dns.Records
{
    public class AddressRecord : Record
    {
        public IPAddress Address { get; }

        internal AddressRecord(
            string name,
            Type type,
            Class @class,
            uint timeToLive,
            ushort length,
            ReadOnlyMemory<byte> data
        ) : base(name, type, @class, timeToLive, length, data) {
            if (type != Type.A && type != Type.AAAA)
                throw new ArgumentOutOfRangeException(nameof(type));

            Address = new IPAddress(data.ToArray());
        }

        public AddressRecord(
            string name,
            Type type,
            Class @class,
            uint timeToLive,
            IPAddress address
        ) : this(
            name,
            type,
            @class,
            timeToLive,
            (ushort)address.GetAddressBytes().Length,
            address.GetAddressBytes()
        ) {
        }

        [System.Diagnostics.DebuggerStepThrough]
        public override string ToString() => $"{Name}\t{TimeToLive}\t{Class}\t{Type}\t{Address}";
    }
}
