using System;
using System.Collections.Generic;

namespace Ward.Dns.Records
{
    class RecordFactory
    {
        public static Record Create(
            string name,
            Type type,
            Class @class,
            uint ttl,
            ushort length,
            ReadOnlyMemory<byte> data,
            ReadOnlySpan<byte> message,
            Dictionary<int, string> reverseOffsetMap
        ) {
            switch (type) {
                case Type.A:
                case Type.AAAA:
                    return new AddressRecord(name, type, @class, ttl, length, data);
                case Type.MX:
                    return new MailExchangerRecord(name, @class, ttl, length, data, message, reverseOffsetMap);
                case Type.CAA:
                    return new CaaRecord(name, @class, ttl, length, data);
                case Type.CNAME:
                    return new CnameRecord(name, @class, ttl, length, data, message, reverseOffsetMap);
                case Type.NS:
                    return new NsRecord(name, @class, ttl, length, data, message, reverseOffsetMap);
                case Type.SOA:
                    return new SoaRecord(name, @class, ttl, length, data, message, reverseOffsetMap);
                case Type.PTR:
                    return new PtrRecord(name, @class, ttl, length, data, message, reverseOffsetMap);
                case Type.TXT:
                    return new TxtRecord(name, @class, ttl, length, data);
                case Type.OPT:
                    return new OptRecord(name, (ushort)@class, ttl, length, data);
                default:
                    return new Record(name, type, @class, ttl, length, data);
            }
        }
    }
}
