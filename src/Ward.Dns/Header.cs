using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

using static Ward.Dns.Utils;

namespace Ward.Dns
{
    public class Header
    {
        public readonly struct HeaderFlags
        {
            public bool Query { get; }
            public bool Authoritative { get; }
            public bool Truncated { get; }
            public bool Recurse { get; }
            public bool RecursionAvailable { get; }
            public bool Z { get; }
            public bool Authenticated { get; }
            public bool CheckingDisabled { get; }

            public HeaderFlags(
                bool query,
                bool authoritative,
                bool truncated,
                bool recurse,
                bool recursionAvailable,
                bool z,
                bool authenticated,
                bool checkingDisabled
            ) {
                Query = query;
                Authoritative = authoritative;
                Truncated = truncated;
                Recurse = recurse;
                RecursionAvailable = recursionAvailable;
                Z = z;
                Authenticated = authenticated;
                CheckingDisabled = checkingDisabled;
            }

            internal HeaderFlags(ushort flags)
            {
                Query = (flags & 0b1000_0000_0000_0000) == 0;
                Authoritative = (flags & 0b0000_0100_0000_0000) != 0;
                Truncated = (flags & 0b0000_0010_0000_0000) != 0;
                Recurse = (flags & 0b0000_0001_0000_0000) != 0;
                RecursionAvailable = (flags & 0b0000_0000_1000_0000) != 0;
                Z = (flags & 0b0000_0000_0100_0000) != 0;
                Authenticated = (flags & 0b0000_0000_0010_0000) != 0;
                CheckingDisabled = (flags & 0b0000_0000_0001_0000) != 0;
            }
        }

        static readonly Random idRand = new Random();

        public ushort Id { get; }
        public Opcode Opcode { get; }
        public HeaderFlags Flags { get; }
        public ReturnCode ReturnCode { get; }
        public ushort TotalQuestions { get; }
        public ushort TotalAnswerRecords { get; }
        public ushort TotalAuthorityRecords { get; }
        public ushort TotalAdditionalRecords { get; }

        internal static Header ParseFromStream(Stream messageStream)
        {
            using (var reader = new BinaryReader(messageStream, Encoding.ASCII, true)) {
                var id = SwapUInt16(reader.ReadUInt16());
                var flagsBitfield = SwapUInt16(reader.ReadUInt16());
                var opcode = (Opcode)(flagsBitfield & 0b0111_1000_0000_0000);
                var returnCode = (ReturnCode)(flagsBitfield & 0b0000_0000_0000_1111);
                var flags = new HeaderFlags(flagsBitfield);
                var qCount = SwapUInt16(reader.ReadUInt16());
                var anCount = SwapUInt16(reader.ReadUInt16());
                var auCount = SwapUInt16(reader.ReadUInt16());
                var adCount = SwapUInt16(reader.ReadUInt16());

                return new Header(id, opcode, returnCode, flags, qCount, anCount, auCount, adCount);
            }
        }

        public Header(
            ushort? id,
            Opcode opcode,
            ReturnCode returnCode,
            HeaderFlags flags,
            ushort qCount,
            ushort anCount,
            ushort auCount,
            ushort adCount
        ) {
            Id = id.GetValueOrDefault((ushort)idRand.Next(0, ushort.MaxValue+1));
            Flags = flags;
            Opcode = opcode;
            ReturnCode = returnCode;
            TotalQuestions = qCount;
            TotalAnswerRecords = anCount;
            TotalAuthorityRecords = auCount;
            TotalAdditionalRecords = adCount;
        }

        internal async Task WriteToStreamAsync(Stream stream)
        {
            await stream.WriteAsync(BitConverter.GetBytes(SwapUInt16(Id)), 0, 2);

            ushort flags = 0;
            flags |= (ushort)((Flags.Query ? 0 : 1) << 15);
            flags |= (ushort)((ushort)Opcode << 14);
            flags |= (ushort)((Flags.Authoritative ? 1 : 0) << 10);
            flags |= (ushort)((Flags.Truncated ? 1 : 0) << 9);
            flags |= (ushort)((Flags.Recurse ? 1 : 0) << 8);
            flags |= (ushort)((Flags.RecursionAvailable ? 1 : 0) << 7);
            flags |= (ushort)((Flags.Z ? 1 : 0) << 6);
            flags |= (ushort)((Flags.Authenticated ? 1 : 0) << 5);
            flags |= (ushort)((Flags.CheckingDisabled ? 1 : 0) << 4);
            flags |= (ushort)ReturnCode;
            await stream.WriteAsync(BitConverter.GetBytes(SwapUInt16(flags)), 0, 2);

            await stream.WriteAsync(BitConverter.GetBytes(SwapUInt16(TotalQuestions)), 0, 2);
            await stream.WriteAsync(BitConverter.GetBytes(SwapUInt16(TotalAnswerRecords)), 0, 2);
            await stream.WriteAsync(BitConverter.GetBytes(SwapUInt16(TotalAuthorityRecords)), 0, 2);
            await stream.WriteAsync(BitConverter.GetBytes(SwapUInt16(TotalAdditionalRecords)), 0, 2);
        }
    }
}
