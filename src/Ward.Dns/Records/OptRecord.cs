using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using static Ward.Dns.Utils;

namespace Ward.Dns.Records
{
    public class OptRecord : Record
    {
        public enum OptionCode : ushort
        {
            LLQ = 1,
            UL = 2,
            NSID = 3,
            DAU = 5,
            DHU = 6,
            N3U = 7,
            EdnsClientSubnet = 8,
            EdnsExpire = 9,
            Cookie = 10,
            EdnsTcpKeepalive = 11,
            Padding = 12,
            Chain = 13,
            EdnsKeyTag = 14
        }

        public IReadOnlyList<(OptionCode optionCode, ReadOnlyMemory<byte> optionData)> OptionalData { get; }
        public ushort UdpPayloadSize { get; }
        public byte Edns0Version { get; }
        public byte ExtendedRcode { get; }
        public bool DnsSecOk { get; }

        internal unsafe OptRecord(
            string name,
            ushort payloadSize,
            uint rcodeAndFlags,
            ushort length,
            ReadOnlyMemory<byte> data
        ) : base(name, Dns.Type.OPT, (Class)payloadSize, rcodeAndFlags, length, data) {
            UdpPayloadSize = payloadSize;
            ExtendedRcode = (byte)(rcodeAndFlags >> 24);
            Edns0Version = (byte)(rcodeAndFlags >> 16);
            DnsSecOk = (((short)rcodeAndFlags) & 0b1000_0000_0000_0000) == 1;

            var optionalData = new List<(OptionCode optionCode, ReadOnlyMemory<byte> optionData)>();
            using (var pin = data.Pin()) {
                var dataPtr = (byte*)pin.Pointer;
                var pos = 0;

                while (pos < data.Length) {
                    var optionCode = (OptionCode)SwapUInt16(*(ushort*)(dataPtr + pos));
                    pos += 2;
                    var optionLength = SwapUInt16(*(ushort*)(dataPtr + pos));
                    pos += 2;
                    var optionData = data.Slice(pos, optionLength);
                    pos += optionLength;
                    optionalData.Add((optionCode, optionData));
                }
            }
            OptionalData = optionalData.AsReadOnly();
        }

        public OptRecord(
            ushort payloadSize,
            uint rcodeAndFlags,
            byte extendedRcode,
            byte edns0Version,
            bool dnsSecOk,
            IEnumerable<(OptionCode optionCode, ReadOnlyMemory<byte> optionData)> data
        ) : base(null, Dns.Type.OPT, (Class)payloadSize, rcodeAndFlags, 0, Array.Empty<byte>()) {
            UdpPayloadSize = payloadSize;
            ExtendedRcode = extendedRcode;
            Edns0Version = edns0Version;
            DnsSecOk = dnsSecOk;
            OptionalData = data.ToList().AsReadOnly();
        }

        [System.Diagnostics.DebuggerStepThrough]
        public override string ToString() => $"";
    }
}
