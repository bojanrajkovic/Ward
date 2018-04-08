using System;

using static Aelfweard.Dns.Utils;

namespace Aelfweard.Dns.Records
{
    public class MailExchangerRecord : Record
    {
        readonly byte[] message;

        public MailExchangerRecord (
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

        public ushort Preference => SwapUInt16(BitConverter.ToUInt16(Data, 0));
        public string Hostname => ParseComplexName(message, Data, 2);

        public override string ToString() =>
            $"{Name}\t{TimeToLive}\t{Class}\t{Type}\t{Preference}\t{Hostname}";
    }
}
