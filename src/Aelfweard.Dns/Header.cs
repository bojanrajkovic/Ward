using System;

namespace Aelfweard.Dns
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
    }
}
