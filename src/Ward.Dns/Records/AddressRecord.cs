using System.Net;

namespace Ward.Dns.Records
{
    public class AddressRecord : Record
    {
        public AddressRecord(
            string name,
            Type type,
            Class @class,
            uint timeToLive,
            ushort length,
            byte[] data
        ) : base(name, type, @class, timeToLive, length, data) {
        }

        public IPAddress Address => new IPAddress(Data);
        public override string ToString() =>
            $"{base.Name}\t{base.TimeToLive}\t{base.Class}\t{base.Type}\t{Address}";
    }
}
