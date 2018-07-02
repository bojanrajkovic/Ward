using System.Collections.Immutable;

using Ward.Dns;

namespace Ward.DnsClient
{
    /// <summary>
    /// A DNS resolve result.
    /// </summary>
    /// <seealso cref="IResolveResult" />
    public class ResolveResult : IResolveResult
    {
        readonly Message message;

        /// <inheritdoc />
        public int MessageSize { get; }

        /// <inheritdoc />
        public Header Header => message.Header;

        /// <inheritdoc />
        public ImmutableList<Record> Answers => message.Answers;

        /// <inheritdoc />
        public ImmutableList<Record> Authority => message.Authority;

        /// <inheritdoc />
        public ImmutableList<Record> Additional => message.Additional;

        /// <inheritdoc />
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
