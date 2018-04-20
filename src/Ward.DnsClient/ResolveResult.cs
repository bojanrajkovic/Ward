using System.Collections.Generic;
using System.Collections.Immutable;

using Ward.Dns;

namespace Ward.DnsClient
{
    public class ResolveResult : IResolveResult
    {
        readonly Message message;

        public int MessageSize { get; }
        public Header Header => message.Header;
        public ImmutableList<Record> Answers => message.Answers;
        public ImmutableList<Record> Authority => message.Authority;
        public ImmutableList<Record> Additional => message.Additional;
        public ImmutableList<Question> Questions => message.Questions;

        public ResolveResult(Message responseMessage, int messageSize) {
            message = responseMessage;
            MessageSize = messageSize;
        }
    }
}
