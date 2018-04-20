using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static Ward.Dns.Utils;

namespace Ward.Dns
{
    public class MessageWriter
    {
        public static async Task<byte[]> SerializeMessageAsync(Message m)
        {
            // Cheat and use a memory stream.
            using (var s = new MemoryStream()) {
                var offsetMap = new Dictionary<string, int>();

                await WriteHeaderToStreamAsync(m.Header, s);

                foreach (var question in m.Questions) {
                    offsetMap.Add(question.Name, (int)s.Position);
                    await WriteQuestionToStreamAsync(question, s);
                }

                var records = m.Answers.Concat(m.Authority).Concat(m.Additional);
                foreach (var record in records)
                    await WriteRecordToStreamAsync(record, s, offsetMap);

                return s.ToArray();
            }
        }

        static async Task WriteRecordToStreamAsync(Record r, Stream s, Dictionary<string, int> offsetMap)
        {
            if (string.IsNullOrWhiteSpace(r.Name)) {
                s.WriteByte(0);
            } else {
                // TODO: Fix this scheme. We need to look for subprefixes in the
                // TODO: offset map and compact the names.
                var nameOffset = (ushort)(0b1100_0000_0000_0000 | offsetMap[r.Name]);

                if (!offsetMap.ContainsKey(r.Name))
                    offsetMap.Add(r.Name, (int)s.Position);

                await s.WriteAsync(BitConverter.GetBytes(SwapUInt16(nameOffset)), 0, 2);
            }

            await s.WriteAsync(BitConverter.GetBytes(SwapUInt16((ushort)r.Type)), 0, 2);
            await s.WriteAsync(BitConverter.GetBytes(SwapUInt16((ushort)r.Class)), 0, 2);
            await s.WriteAsync(BitConverter.GetBytes(SwapUInt32(r.TimeToLive)), 0, 4);
            await s.WriteAsync(BitConverter.GetBytes(SwapUInt16(r.Length)), 0, 2);
            await s.WriteAsync(r.Data.ToArray(), 0, r.Data.Length);
        }

        static async Task WriteQuestionToStreamAsync(Question q, Stream s)
        {
            var qname = Utils.WriteQName(q.Name);
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
