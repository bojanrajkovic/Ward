using System;
using System.Buffers;
using Ward.Dns.Records;
using static Ward.Dns.Utils;

namespace Ward.Dns
{
    /// <summary>
    /// A parser for DNS messages.
    /// </summary>
    public class MessageParser
    {
        /// <summary>
        /// Parses a DNS message from <paramref name="bytes" />, starting at
        /// <paramref name="offset" />.
        /// </summary>
        /// <param name="bytes">The bytes to parse the message from.</param>
        /// <param name="offset">The offset in the array to start parsing from.</param>
        /// <returns>A parsed DNS message.</returns>
        public static Message ParseMessage(byte[] bytes, int offset)
        {
            var pos = offset;

            // First, the header. The header is 12 bytes long, and consists
            // of a 2 byte ID, 2 bytes of flags, and 8 bytes of "lengths" for
            // the 4 sections.
            var id = SwapUInt16(BitConverter.ToUInt16(bytes, pos));
            pos += 2;

            // Parse the flags out of the header--the easiest way to do this is
            // read all 16 bits, then bit-bang the flags out.
            var flagsBitfield = SwapUInt16(BitConverter.ToUInt16(bytes, pos));
            var opcode = (Opcode)(flagsBitfield & 0b0111_1000_0000_0000);
            var returnCode = (ReturnCode)(flagsBitfield & 0b0000_0000_0000_1111);
            var flags = new Header.HeaderFlags(flagsBitfield);
            pos += 2;

            // Read the section counts (questions, answers, authority, additional)
            var qCount = SwapUInt16(BitConverter.ToUInt16(bytes, pos));
            var anCount = SwapUInt16(BitConverter.ToUInt16(bytes, pos + 2));
            var auCount = SwapUInt16(BitConverter.ToUInt16(bytes, pos + 4));
            var adCount = SwapUInt16(BitConverter.ToUInt16(bytes, pos + 6));
            pos += 8;

            // Now parse the questions and records out.
            var questions = new Question[qCount];
            Record[] answers = new Record[anCount],
                     authorities = new Record[auCount],
                     additionals = new Record[adCount];

            // Questions are easy parsing--name, type, and class.
            for (var q = 0; q < qCount; q++) {
                // ParseComplexName will advance `pos`,
                var name = ParseComplexName(bytes, null, ref pos);
                var type = (Type)SwapUInt16(BitConverter.ToUInt16(bytes, pos));
                var @class = (Class)SwapUInt16(BitConverter.ToUInt16(bytes, pos + 2));
                pos += 4;

                questions[q] = new Question(name, type, @class);
            }

            // Parse each individual record for the answer, authority, and
            // additional sections.
            for (var a = 0; a < anCount; a++)
                answers[a] = ParseRecord(bytes, ref pos);

            for (var a = 0; a < auCount; a++)
                authorities[a] = ParseRecord(bytes, ref pos);

            bool haveSeenOptRecord = false;
            for (var a = 0; a < adCount; a++) {
                var newRecord = ParseRecord(bytes, ref pos);
                additionals[a] = newRecord;

                // Validate, and update RCODE if OPT contains extended RCODE bits.
                if (newRecord is OptRecord opt) {
                    if (haveSeenOptRecord)
                        throw new InvalidOperationException("Invalid DNS message, contains multiple OPT pseudo-RRs!");
                    haveSeenOptRecord = true;
                    if (opt.ExtendedRcode != 0)
                        returnCode = (ReturnCode)(opt.ExtendedRcode << 4 | (ushort)returnCode);
                }

            }

            // Build the header now that we've computed the entire RCODE,
            // and return the full message.
            var header = new Header(id, opcode, returnCode, flags, qCount, anCount, auCount, adCount);
            return new Message(header, questions, answers, authorities, additionals);
        }

        /// <summary>
        /// Parses a record from <paramref name="bytes"/>, starting at
        /// <paramref name="offset"/>.
        /// </summary>
        /// <param name="bytes">The array to parse from.</param>
        /// <param name="offset">The offset to start parsing at.</param>
        /// <returns>A parsed record.</returns>
        static Record ParseRecord(byte[] bytes, ref int offset)
        {
            var name = ParseComplexName(bytes, null, ref offset);
            var type = (Type)SwapUInt16(BitConverter.ToUInt16(bytes, offset));
            var @class = (Class)SwapUInt16(BitConverter.ToUInt16(bytes, offset + 2));
            var ttl = SwapUInt32(BitConverter.ToUInt32(bytes, offset + 4));
            var dataLength = SwapUInt16(BitConverter.ToUInt16(bytes, offset + 8));
            var data = new ReadOnlyMemory<byte>(bytes, offset + 10, dataLength);

            // 2 bytes of type, 2 bytes of class, 4 bytes of TTL, 2 bytes of data length,
            // dataLength bytes of data.
            offset += 10 + dataLength;

            return RecordFactory.Create(
                name,
                type,
                @class,
                ttl,
                dataLength,
                data,
                bytes
            );
        }
    }
}
