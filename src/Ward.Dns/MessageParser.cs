using System;
using System.Collections.Generic;
using Ward.Dns.Records;

using static System.Buffers.Binary.BinaryPrimitives;
using static Ward.Dns.Utils;

namespace Ward.Dns
{
    /// <summary>
    /// A parser for DNS messages.
    /// </summary>
    public static class MessageParser
    {
        /// <summary>
        /// Parses a DNS message from <paramref name="bytes" />, starting at
        /// <paramref name="offset" />.
        /// </summary>
        /// <param name="bytes">The bytes to parse the message from.</param>
        /// <param name="offset">The offset in the array to start parsing from.</param>
        /// <returns>A parsed DNS message.</returns>
        public static Message ParseMessage(ReadOnlySpan<byte> bytes, int offset = 0)
        {
            var reverseOffsetMap = new Dictionary<int, string>();

            // First, the header. The header is 12 bytes long, and consists
            // of a 2 byte ID, 2 bytes of flags, and 8 bytes of "lengths" for
            // the 4 sections.
            var id = ReadUInt16BigEndian(bytes);
            offset += 2;

            // Parse the flags out of the header--the easiest way to do this is
            // read all 16 bits, then bit-bang the flags out.
            var flagsBitfield = ReadUInt16BigEndian(bytes.Slice(offset, 2));
            var opcode = (Opcode)(flagsBitfield & 0b0111_1000_0000_0000);
            var returnCode = (ReturnCode)(flagsBitfield & 0b0000_0000_0000_1111);
            var flags = new Header.HeaderFlags(flagsBitfield);
            offset += 2;

            // Read the section counts (questions, answers, authority, additional)
            var qCount = ReadUInt16BigEndian(bytes.Slice(offset, 2));
            var anCount = ReadUInt16BigEndian(bytes.Slice(offset + 2, 2));
            var auCount = ReadUInt16BigEndian(bytes.Slice(offset + 4, 2));
            var adCount = ReadUInt16BigEndian(bytes.Slice(offset + 6, 2));
            offset += 8;

            // Now parse the questions and records out.
            var questions = new Question[qCount];
            Record[] answers = new Record[anCount],
                     authorities = new Record[auCount],
                     additionals = new Record[adCount];

            // Questions are easy parsing--name, type, and class.
            for (var q = 0; q < qCount; q++) {
                // ParseComplexName will advance `pos`,
                var origOffset = offset;
                var name = ParseComplexName(bytes, ReadOnlySpan<byte>.Empty, ref offset, reverseOffsetMap);

                reverseOffsetMap.Add(origOffset, name);

                var type = (Type)ReadUInt16BigEndian(bytes.Slice(offset, 2));
                var @class = (Class)ReadUInt16BigEndian(bytes.Slice(offset + 2, 2));
                offset += 4;

                questions[q] = new Question(name, type, @class);
            }

            // Parse each individual record for the answer, authority, and
            // additional sections.
            for (var a = 0; a < anCount; a++)
                answers[a] = ParseRecord(bytes, ref offset, reverseOffsetMap);

            for (var a = 0; a < auCount; a++)
                authorities[a] = ParseRecord(bytes, ref offset, reverseOffsetMap);

            bool haveSeenOptRecord = false;
            for (var a = 0; a < adCount; a++) {
                var newRecord = ParseRecord(bytes, ref offset, reverseOffsetMap);
                additionals[a] = newRecord;

                // Validate, and update RCODE if OPT contains extended RCODE bits.
                if (!(newRecord is OptRecord opt))
                    continue;

                if (haveSeenOptRecord)
                    throw new InvalidOperationException("Invalid DNS message, contains multiple OPT pseudo-RRs!");

                haveSeenOptRecord = true;
                if (opt.ExtendedRcode != 0)
                    returnCode = (ReturnCode)(opt.ExtendedRcode << 4 | (ushort)returnCode);
            }

            // Build the header now that we've computed the entire RCODE,
            // and return the full message.
            var header = new Header(id, opcode, returnCode, flags, qCount, anCount, auCount, adCount);
            return new Message(header, questions, answers, authorities, additionals);
        }

        /// <summary>
        /// Parses a record from <paramref name="bytes" />, starting at
        /// <paramref name="offset" />.
        /// </summary>
        /// <param name="bytes">The array to parse from.</param>
        /// <param name="offset">The offset to start parsing at.</param>
        /// <param name="reverseOffsetMap">The reverse offset map.</param>
        /// <returns>
        /// A parsed record.
        /// </returns>
        static Record ParseRecord(
            ReadOnlySpan<byte> bytes,
            ref int offset,
            Dictionary<int, string> reverseOffsetMap
        ) {
            // Read the name first, without touching the full array.
            var origOffset = offset;
            var name = ParseComplexName(bytes, null, ref offset, reverseOffsetMap);
            reverseOffsetMap.Add(origOffset, name);

            // Now slice this away for easier offsetting.
            var origBytes = bytes;
            bytes = bytes.Slice(offset);

            var type = (Type)ReadUInt16BigEndian(bytes);
            var @class = (Class)ReadUInt16BigEndian(bytes.Slice(2, 2));
            var ttl = ReadUInt32BigEndian(bytes.Slice(4, 4));
            var dataLength = ReadUInt16BigEndian(bytes.Slice(8, 2));
            var data = bytes.Slice(10, dataLength);

            // 2 bytes of type, 2 bytes of class, 4 bytes of TTL, 2 bytes of data length,
            // dataLength bytes of data.
            offset += 10 + dataLength;

            return RecordFactory.Create(
                name,
                type,
                @class,
                ttl,
                dataLength,
                new ReadOnlyMemory<byte>(data.ToArray()),
                origBytes,
                reverseOffsetMap
            );
        }
    }
}
