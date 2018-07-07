using System.Collections.Generic;
using System.Collections.Immutable;

using Ward.Dns;

namespace Ward.DnsClient
{
    /// <summary>
    /// A DNS resolve result.
    /// </summary>
    public interface IResolveResult
    {
        /// <summary>
        /// Gets the size of the message.
        /// </summary>
        /// <value>
        /// The size of the message.
        /// </value>
        int MessageSize { get; }

        /// <summary>
        /// Gets the header.
        /// </summary>
        /// <value>
        /// The header.
        /// </value>
        Header Header { get; }

        /// <summary>
        /// Gets the answers.
        /// </summary>
        /// <value>
        /// The answers.
        /// </value>
        IReadOnlyList<Record> Answers { get; }

        /// <summary>
        /// Gets the authoritative servers.
        /// </summary>
        /// <value>
        /// The authoritative servers.
        /// </value>
        IReadOnlyList<Record> Authority { get; }

        /// <summary>
        /// Gets the additional records.
        /// </summary>
        /// <value>
        /// The additional records.
        /// </value>
        IReadOnlyList<Record> Additional { get; }

        /// <summary>
        /// Gets the questions.
        /// </summary>
        /// <value>
        /// The questions.
        /// </value>
        IReadOnlyList<Question> Questions { get; }
    }
}
