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
        ImmutableList<Record> Answers { get; }

        /// <summary>
        /// Gets the authoritative servers.
        /// </summary>
        /// <value>
        /// The authoritative servers.
        /// </value>
        ImmutableList<Record> Authority { get; }

        /// <summary>
        /// Gets the additional records.
        /// </summary>
        /// <value>
        /// The additional records.
        /// </value>
        ImmutableList<Record> Additional { get; }

        /// <summary>
        /// Gets the questions.
        /// </summary>
        /// <value>
        /// The questions.
        /// </value>
        ImmutableList<Question> Questions { get; }
    }
}
