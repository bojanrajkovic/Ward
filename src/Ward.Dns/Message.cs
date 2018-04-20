using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Ward.Dns
{
    public class Message
    {
        public Header Header { get; }
        public ImmutableList<Question> Questions { get; }
        public ImmutableList<Record> Answers { get; }
        public ImmutableList<Record> Authority { get; }
        public ImmutableList<Record> Additional { get; }

        public Message(
            Header header,
            IEnumerable<Question> questions,
            IEnumerable<Record> answers,
            IEnumerable<Record> authority,
            IEnumerable<Record> additional
        ) {
            Header = header;
            Questions = questions.ToImmutableList();
            Answers = answers.ToImmutableList();
            Authority = authority.ToImmutableList();
            Additional = additional.ToImmutableList();
        }
    }
}
