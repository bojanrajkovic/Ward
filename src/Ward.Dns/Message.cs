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
        public ImmutableList<IRecord> Answers { get; }
        public ImmutableList<IRecord> Authority { get; }
        public ImmutableList<IRecord> Additional { get; }

        public Message(
            Header header,
            IEnumerable<Question> questions,
            IEnumerable<IRecord> answers,
            IEnumerable<IRecord> authority,
            IEnumerable<IRecord> additional
        ) {
            Header = header;
            Questions = questions.ToImmutableList();
            Answers = answers.ToImmutableList();
            Authority = authority.ToImmutableList();
            Additional = additional.ToImmutableList();
        }
    }
}
