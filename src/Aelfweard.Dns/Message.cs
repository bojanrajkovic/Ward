using System.Collections.Generic;
using System.Collections.Immutable;

namespace Aelfweard.Dns
{
    public class Message
    {
        public Header Header { get; }
        public ImmutableList<Question> Questions { get; }
        public ImmutableList<Record> Answers { get; }
        public ImmutableList<Record> Authority { get; }
        public ImmutableList<Record> Additional { get; }

        public Message()
        {
        }
    }
}
