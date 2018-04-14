using System;

using static Ward.Dns.Utils;

namespace Ward.Dns.Records
{
    public class CnameRecord : Record
    {
        readonly byte[] message;

        public CnameRecord (
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

        public string Hostname => ParseComplexName(message, Data, 0);

        public override string ToString() =>
            $"{Name}\t{TimeToLive}\t{Class}\t{Type}\t{Hostname}";
    }
}
