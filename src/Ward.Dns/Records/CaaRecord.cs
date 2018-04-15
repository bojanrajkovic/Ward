using System;
using System.Text;

using static Ward.Dns.Utils;

namespace Ward.Dns.Records
{
    public class CaaRecord : Record
    {
        public CaaRecord (
            string name,
            Dns.Type type,
            Class @class,
            uint timeToLive,
            ushort length,
            byte[] data
        ) : base(name, type, @class, timeToLive, length, data) {
            // The top bit is set if the critical flag is true, all other
            // bit positions are reserved per RFC 6844.
            Critical = (data[0] & 0b1000_0000) == 0b1000_0000;
            var tagLength = data[1];
            Tag = Encoding.ASCII.GetString(data, 2, tagLength);
            Value = Encoding.ASCII.GetString(data, 2 + tagLength, length - (2 + tagLength));
        }

        public bool Critical { get; }
        public string Tag { get; }
        public string Value { get; }

        public override string ToString() =>
            $"{Name}\t{TimeToLive}\t{Class}\t{Type}\t{(Critical ? 1 : 0)} {Tag} {Value}";
    }
}
