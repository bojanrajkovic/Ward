using System;
using System.Text;

using static Ward.Dns.Utils;

namespace Ward.Dns.Records
{
    public class TxtRecord : Record
    {
        readonly byte[] message;

        public TxtRecord (
            string name,
            Dns.Type type,
            Class @class,
            uint timeToLive,
            ushort length,
            byte[] data,
            byte[] message
        ) : base(name, type, @class, timeToLive, length, data) {
            this.message = message;
        }

        public string TextData => Encoding.ASCII.GetString(Data, 1, Data[0]);

        public override string ToString() =>
            $"{Name}\t{TimeToLive}\t{Class}\t{Type}\t{TextData}";
    }
}
