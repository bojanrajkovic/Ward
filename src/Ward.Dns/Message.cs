using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Ward.Dns
{
    /// <summary>
    /// A complete DNS message.
    /// </summary>
    public class Message
    {
        /// <summary>
        /// Gets the DNS message header.
        /// </summary>
        /// <returns>The message header.</returns>
        public Header Header { get; }

        /// <summary>
        /// Gets the list of questions in this message.
        /// </summary>
        /// <returns>The list of questions in this message.</returns>
        public IReadOnlyList<Question> Questions { get; }

        /// <summary>
        /// Gets the list of answers in this message.
        /// </summary>
        /// <returns>The list of answers in this message.</returns>
        public IReadOnlyList<Record> Answers { get; }

        /// <summary>
        /// Gets the list of entries in the authority section of this message.
        /// </summary>
        /// <returns>The entries in the authority section of the message.</returns>
        public IReadOnlyList<Record> Authority { get; }

        /// <summary>
        /// Gets the list of entries in the additional section of this message.
        /// </summary>
        /// <returns>The entries in the additional section of this message.</returns>
        public IReadOnlyList<Record> Additional { get; }

        /// <summary>
        /// Creates a new DNS message.
        /// </summary>
        /// <param name="header">The message header.</param>
        /// <param name="questions">The questions to include in the message.</param>
        /// <param name="answers">The answers to inlude in the message.</param>
        /// <param name="authority">The authority entries to include in the message.</param>
        /// <param name="additional">The additional entries to include in the message.</param>
        public Message(
            Header header,
            IReadOnlyList<Question> questions,
            IReadOnlyList<Record> answers,
            IReadOnlyList<Record> authority,
            IReadOnlyList<Record> additional
        ) {
            Header = header;
            Questions = questions;
            Answers = answers;
            Authority = authority;
            Additional = additional;
        }
    }
}
