using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

using static Ward.Dns.Utils;

namespace Ward.Dns
{
    public class Header
    {
        static readonly Random idRand = new Random();

        public ushort Id { get; }
        public bool Query { get; }
        public Opcode Opcode { get; }
        public bool Authoritative { get; }
        public bool Truncated { get; }
        public bool Recurse { get; }
        public bool RecursionAvailable { get; }
        public bool Z { get; }
        public bool Authenticated { get; }
        public bool CheckingDisabled { get; }
        public ReturnCode ReturnCode { get; }
        public ushort TotalQuestions { get; }
        public ushort TotalAnswerRecords { get; }
        public ushort TotalAuthorityRecords { get; }
        public ushort TotalAdditionalRecords { get; }

        internal static Header ParseFromStream(Stream messageStream)
        {
            using (var reader = new BinaryReader(messageStream, Encoding.ASCII, true)) {
                var id = SwapUInt16(reader.ReadUInt16());

                var flags = SwapUInt16(reader.ReadUInt16());
                var query = (flags & 0b1000_0000_0000_0000) == 0;
                var opcode = (Opcode)(flags & 0b0111_1000_0000_0000);
                var authoritative = (flags & 0b0000_0100_0000_0000) != 0;
                var truncated = (flags & 0b0000_0010_0000_0000) != 0;
                var recurse = (flags & 0b0000_0001_0000_0000) != 0;
                var recursionAvailable = (flags & 0b0000_0000_1000_0000) != 0;
                var z = (flags & 0b0000_0000_0100_0000) != 0;
                var authenticated = (flags & 0b0000_0000_0010_0000) != 0;
                var checkingDisabled = (flags & 0b0000_0000_0001_0000) != 0;
                var returnCode = (ReturnCode)(flags & 0b0000_0000_0000_1111);

                var totalQuestions = SwapUInt16(reader.ReadUInt16());
                var totalAnswerRecords = SwapUInt16(reader.ReadUInt16());
                var totalAuthorityRecords = SwapUInt16(reader.ReadUInt16());
                var totalAdditionalRecords = SwapUInt16(reader.ReadUInt16());

                return new Header(
                    id,
                    query,
                    opcode,
                    authoritative,
                    truncated,
                    recurse,
                    recursionAvailable,
                    z,
                    authenticated,
                    checkingDisabled,
                    returnCode,
                    totalQuestions,
                    totalAnswerRecords,
                    totalAuthorityRecords,
                    totalAdditionalRecords
                );
            }
        }

        public Header(
            ushort? id,
            bool query,
            Opcode opcode,
            bool authoritative,
            bool truncated,
            bool recurse,
            bool recursionAvailable,
            bool z,
            bool authenticated,
            bool checkingDisabled,
            ReturnCode returnCode,
            ushort totalQuestions,
            ushort totalAnswerRecords,
            ushort totalAuthorityRecords,
            ushort totalAdditionalRecords
        ) {
            Id = id.GetValueOrDefault((ushort)idRand.Next(0, ushort.MaxValue+1));
            Query = query;
            Opcode = opcode;
            Authoritative = authoritative;
            Truncated = truncated;
            Recurse = recurse;
            RecursionAvailable = recursionAvailable;
            Z = z;
            Authenticated = authenticated;
            CheckingDisabled = checkingDisabled;
            ReturnCode = returnCode;
            TotalQuestions = totalQuestions;
            TotalAnswerRecords = totalAnswerRecords;
            TotalAuthorityRecords = totalAuthorityRecords;
            TotalAdditionalRecords = totalAdditionalRecords;
        }

        internal async Task WriteToStreamAsync(Stream stream)
        {
            await stream.WriteAsync(BitConverter.GetBytes(SwapUInt16(Id)), 0, 2);

            ushort flags = 0;
            flags |= (ushort)((Query ? 0 : 1) << 15);
            flags |= (ushort)((ushort)Opcode << 14);
            flags |= (ushort)((Authoritative ? 1 : 0) << 10);
            flags |= (ushort)((Truncated ? 1 : 0) << 9);
            flags |= (ushort)((Recurse ? 1 : 0) << 8);
            flags |= (ushort)((RecursionAvailable ? 1 : 0) << 7);
            flags |= (ushort)((Z ? 1 : 0) << 6);
            flags |= (ushort)((Authenticated ? 1 : 0) << 5);
            flags |= (ushort)((CheckingDisabled ? 1 : 0) << 4);
            flags |= (ushort)ReturnCode;
            await stream.WriteAsync(BitConverter.GetBytes(SwapUInt16(flags)), 0, 2);

            await stream.WriteAsync(BitConverter.GetBytes(SwapUInt16(TotalQuestions)), 0, 2);
            await stream.WriteAsync(BitConverter.GetBytes(SwapUInt16(TotalAnswerRecords)), 0, 2);
            await stream.WriteAsync(BitConverter.GetBytes(SwapUInt16(TotalAuthorityRecords)), 0, 2);
            await stream.WriteAsync(BitConverter.GetBytes(SwapUInt16(TotalAdditionalRecords)), 0, 2);
        }
    }
}
