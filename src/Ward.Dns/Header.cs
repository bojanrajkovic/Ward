using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

using static Ward.Dns.Utils;

namespace Ward.Dns
{
    public readonly struct Header
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

            [System.Diagnostics.DebuggerStepThrough]
            public override string ToString() {
                var sb = new StringBuilder();
                sb.Append(Query ? "" : "qr ");
                sb.Append(Authoritative ? "aa " : "");
                sb.Append(Truncated ? "tc " : "");
                sb.Append(Recurse ? "rd ": "");
                sb.Append(RecursionAvailable ? "ra " : "");
                sb.Append(Authenticated ? "ad " : "");
                sb.Append(CheckingDisabled ? "cd " : "");
                return sb.Remove(sb.Length-1, 1).ToString();
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
    }
}
