namespace Ward.Dns.Records
{
    class RecordFactory
    {
        public static Record Create(string name, Type type, Class @class, uint ttl, ushort length, byte[] data, byte[] message)
        {
            switch (type) {
                case Type.A:
                    return new AddressRecord(name, type, @class, ttl, length, data);
                case Type.MX:
                    return new MailExchangerRecord(name, type, @class, ttl, length, data, message);
                case Type.CNAME:
                    return new CnameRecord(name, type, @class, ttl, length, data, message);
                case Type.NS:
                    return new NsRecord(name, type, @class, ttl, length, data, message);
                case Type.SOA:
                    return new SoaRecord(name, type, @class, ttl, length, data, message);
                case Type.PTR:
                    return new PtrRecord(name, type, @class, ttl, length, data, message);
                default:
                    return new Record(name, type, @class, ttl, length, data);
            }
        }
    }
}
