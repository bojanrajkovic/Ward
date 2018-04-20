using System;
using System.Text;

using static Ward.Dns.Utils;

namespace Ward.Dns.Records
{
    public class TxtRecord : Record
    {
        public string TextData { get; }

        public unsafe TxtRecord (
            string name,
            Dns.Type type,
            Class @class,
            uint timeToLive,
            ushort length,
            ReadOnlyMemory<byte> data
        ) : base(name, type, @class, timeToLive, length, data) {
            using (var pin = data.Pin())
                TextData = Encoding.ASCII.GetString((byte*)pin.Pointer + 1, *((byte*)pin.Pointer));
        }

        [System.Diagnostics.DebuggerStepThrough]
        public override string ToString() =>
            $"{Name}\t{TimeToLive}\t{Class}\t{Type}\t{TextData}";
    }
}
