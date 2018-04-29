using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using Ward.Dns.Records;
using static Ward.Dns.Utils;

using OptData = System.Collections.Generic.IEnumerable<(
    Ward.Dns.Records.OptRecord.OptionCode optionCode,
    System.ReadOnlyMemory<byte> optionData
)>;

namespace Ward.Dns
{
    public class MessageWriter
    {
        const int edns0Version = 0;
        const bool dnsSecOk = false;

        public static async Task<byte[]> SerializeMessageAsync(
            Message m,
            bool writeOpt = false,
            ushort udpPayloadSize = 4096,
            OptData optData = null)
        {
            writeOpt = writeOpt || (ushort)m.Header.ReturnCode > 15;

            // Cheat and use a memory stream.
            using (var s = new MemoryStream()) {
                var offsetMap = new Dictionary<string, ushort>();
                await WriteHeaderToStreamAsync(m.Header, writeOpt, s);

                foreach (var question in m.Questions)
                    await WriteQuestionToStreamAsync(question, s, offsetMap);

                var records = m.Answers.Concat(m.Authority).Concat(m.Additional);
                foreach (var record in records)
                    await WriteRecordToStreamAsync(record, s, offsetMap);

                // If we've been asked to write an opt record, or if we *need* to write an
                // opt record because the header RCODE is > 15, write an OPT.
                if (writeOpt)
                    await WriteOptRecordToStreamAsync(udpPayloadSize, optData, m.Header, s);

                return s.ToArray();
            }
        }

        internal static async Task WriteOptRecordToStreamAsync(
            ushort udpPayloadSize,
            OptData optData,
            Header header,
            Stream s
        ) {
            // OPT pseudo-RR's always have a null name.
            await s.WriteAsync(new byte[] { 0 }, 0, 1);

            // Write the OPT type, and the UDP payload size as the class.
            await s.WriteAsync(BitConverter.GetBytes(SwapUInt16((ushort)Type.OPT)), 0, 2);
            await s.WriteAsync(BitConverter.GetBytes(SwapUInt16((ushort)udpPayloadSize)), 0, 2);

            // Compute the extended RCODE, which is the high 8 bits of the return code.
            // We can safely ignore the one in the OPT record for now, because the only way to create
            // a mismatch is to modify Ward internals.
            byte extendedRcode = (byte)((ushort)header.ReturnCode >> 4);

            s.WriteByte(extendedRcode);
            s.WriteByte(edns0Version);

            var remainingFlags = (dnsSecOk ? (ushort)(1 << 15) : (ushort)0);
            await s.WriteAsync(BitConverter.GetBytes(SwapUInt16(remainingFlags)), 0, 2);

            byte[] rdata;
            if (optData == null || optData.Count() == 0)
                rdata = Array.Empty<byte>();
            else {
                var arrayOfArrays = new byte[optData.Count()*3][];
                int pos = 0;
                foreach (var data in optData) {
                    arrayOfArrays[pos++] = BitConverter.GetBytes(SwapUInt16((ushort)data.optionCode));
                    arrayOfArrays[pos++] = BitConverter.GetBytes(SwapUInt16((ushort)data.optionData.Length));
                    arrayOfArrays[pos++] = data.optionData.ToArray();
                };
                rdata = Utils.Concat(arrayOfArrays);
            }

            // Now we've written type, class, and "TTL", we can write the data as normal.
            await s.WriteAsync(BitConverter.GetBytes(SwapUInt16((ushort)rdata.Length)), 0, 2);
            await s.WriteAsync(rdata, 0, rdata.Length);
        }

        internal static async Task WriteRecordToStreamAsync(Record r, Stream s, Dictionary<string, ushort> offsetMap)
        {
            var qname = Utils.WriteQName(r.Name, offsetMap);
            if (!string.IsNullOrWhiteSpace(r.Name) && !offsetMap.ContainsKey(r.Name))
                offsetMap.Add(r.Name, (ushort)s.Position);

            await s.WriteAsync(qname, 0, qname.Length);
            await s.WriteAsync(BitConverter.GetBytes(SwapUInt16((ushort)r.Type)), 0, 2);
            await s.WriteAsync(BitConverter.GetBytes(SwapUInt16((ushort)r.Class)), 0, 2);
            await s.WriteAsync(BitConverter.GetBytes(SwapUInt32(r.TimeToLive)), 0, 4);

            var data = await GetDataForRecordAsync(r, s, offsetMap);
            await s.WriteAsync(BitConverter.GetBytes(SwapUInt16((ushort)data.Length)), 0, 2);
            await s.WriteAsync(data.ToArray(), 0, data.Length);
        }

        internal static async Task<byte[]> GetDataForRecordAsync(Record r, Stream s, Dictionary<string, ushort> offsetMap)
        {
            switch (r) {
                case MailExchangerRecord mx:
                    var mxQName = Utils.WriteQName(mx.Hostname, offsetMap);
                    // The QNAME that we just serialized is going to end up at the current
                    // position of the stream (which is just before the data we're about to return)
                    // plus 4 bytes: 2 for the data length, and the 2 preference bytes.
                    if (!offsetMap.ContainsKey(mx.Hostname))
                        offsetMap.Add(mx.Hostname, (ushort)(s.Position + 4));

                    return Utils.Concat(
                        BitConverter.GetBytes(SwapUInt16(mx.Preference)),
                        mxQName
                    );
                case CaaRecord caa:
                    var critical = (byte)(caa.Critical ? 0b1000_0000 : 0);
                    var tagAscii = Encoding.ASCII.GetBytes(caa.Tag);
                    var tagLength = (byte)tagAscii.Length;
                    var value = Encoding.ASCII.GetBytes(caa.Value);

                    return Utils.Concat(
                        new byte[] { critical, tagLength },
                        tagAscii,
                        value
                    );
                case CnameRecord cname:
                    var hostname = Utils.WriteQName(cname.Hostname, offsetMap);
                    if (!offsetMap.ContainsKey(cname.Hostname))
                        offsetMap.Add(cname.Hostname, (ushort)(s.Position + 2));
                    return hostname;
                case NsRecord ns:
                    var nsname = Utils.WriteQName(ns.Hostname, offsetMap);
                    if (!offsetMap.ContainsKey(ns.Hostname))
                        offsetMap.Add(ns.Hostname, (ushort)(s.Position + 2));
                    return nsname;
                case PtrRecord ptr:
                    var ptrname = Utils.WriteQName(ptr.Hostname, offsetMap);
                    if (!offsetMap.ContainsKey(ptr.Hostname))
                        offsetMap.Add(ptr.Hostname, (ushort)(s.Position + 2));
                    return ptrname;
                case SoaRecord soa:
                    var primaryNsName = Utils.WriteQName(soa.PrimaryNameServer, offsetMap);
                    if (!offsetMap.ContainsKey(soa.PrimaryNameServer))
                        offsetMap.Add(soa.PrimaryNameServer, (ushort)(s.Position + 2));
                    var responsiblePerson = Utils.WriteQName(soa.ResponsibleName, offsetMap);
                    if (!offsetMap.ContainsKey(soa.ResponsibleName))
                        offsetMap.Add(soa.ResponsibleName, (ushort)(s.Position + 2 + primaryNsName.Length));
                    var serialBytes = BitConverter.GetBytes(Utils.SwapUInt32(soa.Serial));
                    var refreshBytes = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(soa.Refresh));
                    var retryBytes = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(soa.Retry));
                    var expireBytes = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(soa.Expire));
                    var minimumTtl = BitConverter.GetBytes(Utils.SwapUInt32(soa.MinimumTtl));
                    return Utils.Concat(
                        primaryNsName,
                        responsiblePerson,
                        serialBytes,
                        refreshBytes,
                        retryBytes,
                        expireBytes,
                        minimumTtl
                    );
                case AddressRecord a:
                case TxtRecord t:
                default:
                    return r.Data.ToArray();
            }
        }

        static async Task WriteQuestionToStreamAsync(Question q, Stream s, Dictionary<string, ushort> offsetMap)
        {
            var qname = Utils.WriteQName(q.Name, offsetMap);
            if (!offsetMap.ContainsKey(q.Name))
                offsetMap.Add(q.Name, (ushort)s.Position);

            await s.WriteAsync(qname, 0, qname.Length);

            var type = BitConverter.GetBytes(Utils.SwapUInt16((ushort) q.Type));
            await s.WriteAsync(type, 0, 2);

            var @class = BitConverter.GetBytes(Utils.SwapUInt16((ushort) q.Class));
            await s.WriteAsync(@class, 0, 2);
        }

        static async Task WriteHeaderToStreamAsync(Header h, bool writeOpt, Stream s)
        {
            await s.WriteAsync(BitConverter.GetBytes(SwapUInt16(h.Id)), 0, 2);

            ushort flags = 0;
            flags |= (ushort)((h.Flags.Query ? 0 : 1) << 15);
            flags |= (ushort)((ushort)h.Opcode << 14);
            flags |= (ushort)((h.Flags.Authoritative ? 1 : 0) << 10);
            flags |= (ushort)((h.Flags.Truncated ? 1 : 0) << 9);
            flags |= (ushort)((h.Flags.Recurse ? 1 : 0) << 8);
            flags |= (ushort)((h.Flags.RecursionAvailable ? 1 : 0) << 7);
            flags |= (ushort)((h.Flags.Z ? 1 : 0) << 6);
            flags |= (ushort)((h.Flags.Authenticated ? 1 : 0) << 5);
            flags |= (ushort)((h.Flags.CheckingDisabled ? 1 : 0) << 4);
            // We can only take the _bottom 4 bits_ here, so mask off everything but the bottom 4.
            flags |= (byte)((ushort)h.ReturnCode & 0b0000_0000_0000_1111);
            await s.WriteAsync(BitConverter.GetBytes(SwapUInt16(flags)), 0, 2);

            var optBonus = writeOpt ? 1 : 0;
            await s.WriteAsync(BitConverter.GetBytes(SwapUInt16(h.TotalQuestions)), 0, 2);
            await s.WriteAsync(BitConverter.GetBytes(SwapUInt16(h.TotalAnswerRecords)), 0, 2);
            await s.WriteAsync(BitConverter.GetBytes(SwapUInt16(h.TotalAuthorityRecords)), 0, 2);
            await s.WriteAsync(BitConverter.GetBytes(SwapUInt16((ushort)(h.TotalAdditionalRecords+optBonus))), 0, 2);
        }
    }
}
