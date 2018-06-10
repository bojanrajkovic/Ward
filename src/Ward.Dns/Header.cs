using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

using static Ward.Dns.Utils;

namespace Ward.Dns
{
    /// <summary>
    /// A DNS message header.
    /// </summary>
    public readonly struct Header
    {
        /// <summary>
        /// A container for the various flags in a DNS message header.
        /// </summary>
        public readonly struct HeaderFlags
        {
            /// <summary>
            /// A flag indicating whether this message is a query or a response.
            /// </summary>
            /// <returns>
            /// <c>true</c> if this is a query, <c>false</c> otherwise.
            /// </returns>
            public bool Query { get; }

            /// <summary>
            /// A flag indicating whether this is an authoritative answer.
            /// </summary>
            /// <returns>
            /// <c>true</c> if the answer is authoritatie, <c>false</c> othrewise.
            /// </returns>
            public bool Authoritative { get; }

            /// <summary>
            /// A flag indicating whether this response was truncated.
            /// </summary>
            /// <returns>
            /// <c>true</c> if the response was truncated, <c>false</c> otherwise.
            /// </returns>
            public bool Truncated { get; }

            /// <summary>
            /// Pursue queries recursively. Valid in queries, copied into the response.
            /// </summary>
            /// <returns>
            /// <c>true</c> if the server should pursue the query recursively.
            /// <c>false</c> if the server should not recurse. Recursion support
            /// is optional in servers.
            /// </returns>
            public bool Recurse { get; }

            /// <summary>
            /// Denotes whether recursive query support is enabled in the DNS server.
            /// </summary>
            /// <returns>
            /// <c>true</c> if the server supports recursion, <c>false</c> otherwise.
            /// </returns>
            public bool RecursionAvailable { get; }

            /// <summary>
            /// A reserved flag value.
            /// </summary>
            /// <returns>
            /// The reserved flag's value.
            /// </returns>
            public bool Z { get; }

            /// <summary>
            /// Indicates that the data included has been verified by the
            /// server providing it. Only relevant in DNSSEC scenarios.
            /// </summary>
            /// <returns>
            /// <c>true</c> if the data has been verified, <c>false</c> otherwise.
            /// </returns>
            /// <remarks>
            /// Ward has no DNSSEC support, thus this bit is entirely ignored.
            /// </remarks>
            public bool Authenticated { get; }

            /// <summary>
            /// In a query, indicates that non-authenticated data is acceptable
            /// to the resolver sending the query.
            /// </summary>
            /// <returns>
            /// <c>true</c> if unauthenticated data is acceptable, <c>false</c> otherwise.
            /// </returns>
            /// <remarks>
            /// See the remarks for <see cref="Authenticated" />.
            /// </remarks>
            public bool CheckingDisabled { get; }

            /// <summary>
            /// Creates a header flags container.
            /// </summary>
            /// <param name="query">Is this message a query?</param>
            /// <param name="authoritative">Is the responding server authoritative?</param>
            /// <param name="truncated">Is the response truncated?</param>
            /// <param name="recurse">Should the server recursively pursue this query.</param>
            /// <param name="recursionAvailable">Is recursive querying available?</param>
            /// <param name="z">Reserved. Should always be set to false.</param>
            /// <param name="authenticated">Has the data included been verified by the server?</param>
            /// <param name="checkingDisabled">Is unauthenticated data acceptable to the resolver?</param>
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

            /// <summary>
            /// Parses two bytes of flags into a header flags container.
            /// </summary>
            /// <param name="flags">The two bytes of flags in the DNS wire format.</param>
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

            /// <summary>
            /// Prints a string representation of the set flags.
            /// </summary>
            /// <returns>A string with the set flags printed.</returns>
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

        /// <summary>
        /// The ID of this message. Used to match up requests/responses by clients.
        /// </summary>
        /// <returns>The message ID.</returns>
        public ushort Id { get; }

        /// <summary>
        /// Specifies the kind of query in this message.
        /// </summary>
        /// <returns>The kind of query in this message.</returns>
        public Opcode Opcode { get; }

        /// <summary>
        /// A set of header flags indicating various aspects of the message state.
        /// </summary>
        /// <returns>The header flags.</returns>
        public HeaderFlags Flags { get; }

        /// <summary>
        /// The response code. Set to <see cref="ReturnCode.NoError" />
        /// for queries.
        /// </summary>
        /// <returns>A return/response code for the message.</returns>
        public ReturnCode ReturnCode { get; }

        /// <summary>
        /// A counter for the number of entries in the question section.
        /// </summary>
        /// <returns>The number of entries in the question section.</returns>
        public ushort TotalQuestions { get; }

        /// <summary>
        /// A counter for the number of entries in the answer section.
        /// </summary>
        /// <returns>The number of entries in the answer section.</returns>
        public ushort TotalAnswerRecords { get; }

        /// <summary>
        /// A counter for the number of entries in the authority section.
        /// </summary>
        /// <returns>The number of entries in the authority section.</returns>
        public ushort TotalAuthorityRecords { get; }

        /// <summary>
        /// A counter for the number of entries in the additional section.
        /// </summary>
        /// <returns>The number of entries in the additional section.</returns>
        public ushort TotalAdditionalRecords { get; }

        /// <summary>
        /// Creates a new message header.
        /// </summary>
        /// <param name="id">The message ID.</param>
        /// <param name="opcode">The kind of query in this message.</param>
        /// <param name="returnCode">The return/response code.</param>
        /// <param name="flags">The header flags.</param>
        /// <param name="qCount">The number of entries in the question section.</param>
        /// <param name="anCount">The number of entries in the answer section.</param>
        /// <param name="auCount">The number of entries in the authority section.</param>
        /// <param name="adCount">The number of entries in the additional section.</param>
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
