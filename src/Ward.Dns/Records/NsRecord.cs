using System;

using static Ward.Dns.Utils;

namespace Ward.Dns.Records
{
    public class NsRecord : Record
    {
        readonly byte[] message;

        public NsRecord (
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

        public string NsName => ParseComplexName(message, Data, 0);

        public override string ToString() =>
            $"{Name}\t{TimeToLive}\t{Class}\t{Type}\t{NsName}";
    }
}
