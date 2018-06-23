using System.Collections.Immutable;

using Ward.Dns;

namespace Ward.DnsClient
{
    /// <summary>
    /// A DNS resolve result.
    /// </summary>
    /// <seealso cref="Ward.DnsClient.IResolveResult" />
    public class ResolveResult : IResolveResult
    {
        readonly Message message;

        /// <summary>
        /// Gets the size of the message.
        /// </summary>
        /// <value>
        /// The size of the message.
        /// </value>
        public int MessageSize { get; }

        /// <summary>
        /// Gets the header.
        /// </summary>
        /// <value>
        /// The header.
        /// </value>
        public Header Header => message.Header;

        /// <summary>
        /// Gets the answers.
        /// </summary>
        /// <value>
        /// The answers.
        /// </value>
        public ImmutableList<Record> Answers => message.Answers;

        /// <summary>
        /// Gets the authoritative servers.
        /// </summary>
        /// <value>
        /// The authoritative servers.
        /// </value>
        public ImmutableList<Record> Authority => message.Authority;

        /// <summary>
        /// Gets the additional records.
        /// </summary>
        /// <value>
        /// The additional records.
        /// </value>
        public ImmutableList<Record> Additional => message.Additional;

        /// <summary>
        /// Gets the questions.
        /// </summary>
        /// <value>
        /// The questions.
        /// </value>
        public ImmutableList<Question> Questions => message.Questions;

        /// <summary>
        /// Initializes a new instance of the <see cref="ResolveResult"/> class.
        /// </summary>
        /// <param name="responseMessage">The response message.</param>
        /// <param name="messageSize">Size of the message.</param>
        public ResolveResult(Message responseMessage, int messageSize) {
            message = responseMessage;
            MessageSize = messageSize;
        }
    }
}
