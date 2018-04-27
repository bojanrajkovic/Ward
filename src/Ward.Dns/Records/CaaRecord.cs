using System;
using System.Text;

using static Ward.Dns.Utils;

namespace Ward.Dns.Records
{
    public class CaaRecord : Record
    {
        public bool Critical { get; }
        public string Tag { get; }
        public string Value { get; }

        internal unsafe CaaRecord(
            string name,
            Class @class,
            uint timeToLive,
            ushort length,
            ReadOnlyMemory<byte> data
        ) : base(name, Dns.Type.CAA, @class, timeToLive, length, data) {
            // The top bit is set if the critical flag is true, all other
            // bit positions are reserved per RFC 6844.
            Critical = (data.Span[0] & 0b1000_0000) == 0b1000_0000;
            var tagLength = data.Span[1];

            var dataPointer = data.Pin().Pointer;
            Tag = Encoding.ASCII.GetString((byte*)dataPointer + 2, tagLength);
            Value = Encoding.ASCII.GetString((byte*)dataPointer + 2 + tagLength, (length - 2 - tagLength));
        }

        public CaaRecord(
            string name,
            Class @class,
            uint timeToLive,
            bool critical,
            string tag,
            string value
        ) : base(name, Dns.Type.CAA, @class, timeToLive, 0, Array.Empty<byte>()) {
            Critical = critical;
            Tag = tag;
            Value = value;
        }

        [System.Diagnostics.DebuggerStepThrough]
        public override string ToString() =>
            $"{Name}\t{TimeToLive}\t{Class}\t{Type}\t{(Critical ? 1 : 0)} {Tag} {Value}";
    }
}
