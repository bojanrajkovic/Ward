using System;
using System.Text;

using static Ward.Dns.Utils;

namespace Ward.Dns.Records
{
    public class TxtRecord : Record
    {
        public string TextData { get; }

        internal unsafe TxtRecord(
            string name,
            Class @class,
            uint timeToLive,
            ushort length,
            ReadOnlyMemory<byte> data
        ) : base(name, Dns.Type.TXT, @class, timeToLive, length, data) {
            using (var pin = data.Pin())
                TextData = Encoding.ASCII.GetString((byte*)pin.Pointer + 1, *((byte*)pin.Pointer));
        }

        public TxtRecord(
            string name,
            Class @class,
            uint timeToLive,
            string textData
        ) : base(
            name,
            Dns.Type.TXT,
            @class,
            timeToLive,
            (ushort)(1+Encoding.ASCII.GetByteCount(textData)),
            Utils.Concat(
                new [] { (byte)Encoding.ASCII.GetByteCount(textData) },
                Encoding.ASCII.GetBytes(textData)
            )
        ) {
            TextData = textData;
        }

        [System.Diagnostics.DebuggerStepThrough]
        public override string ToString() =>
            $"{Name}\t{TimeToLive}\t{Class}\t{Type}\t{TextData}";
    }
}
