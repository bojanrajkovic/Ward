using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Ward.Dns.Records;
using static Ward.Dns.Utils;

namespace Ward.Dns
{
    public class MessageWriter
    {
        public static async Task<byte[]> SerializeMessageAsync(Message m)
        {
            // Cheat and use a memory stream.
            using (var s = new MemoryStream()) {
                var offsetMap = new Dictionary<string, ushort>();

                await WriteHeaderToStreamAsync(m.Header, s);

                foreach (var question in m.Questions)
                    await WriteQuestionToStreamAsync(question, s, offsetMap);

                var records = m.Answers.Concat(m.Authority).Concat(m.Additional);
                foreach (var record in records)
                    await WriteRecordToStreamAsync(record, s, offsetMap);

                return s.ToArray();
            }
        }

        internal static async Task WriteRecordToStreamAsync(Record r, Stream s, Dictionary<string, ushort> offsetMap)
        {
            var qname = Utils.WriteQName(r.Name, offsetMap);
            if (!offsetMap.ContainsKey(r.Name))
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
                case AddressRecord a:
                    return r.Data.ToArray();
                default:
                    // If we don't know how to serialize this record, just write the data that was given.
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

        static async Task WriteHeaderToStreamAsync(Header h, Stream s)
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
            flags |= (ushort)h.ReturnCode;
            await s.WriteAsync(BitConverter.GetBytes(SwapUInt16(flags)), 0, 2);

            await s.WriteAsync(BitConverter.GetBytes(SwapUInt16(h.TotalQuestions)), 0, 2);
            await s.WriteAsync(BitConverter.GetBytes(SwapUInt16(h.TotalAnswerRecords)), 0, 2);
            await s.WriteAsync(BitConverter.GetBytes(SwapUInt16(h.TotalAuthorityRecords)), 0, 2);
            await s.WriteAsync(BitConverter.GetBytes(SwapUInt16(h.TotalAdditionalRecords)), 0, 2);
        }
    }
}
