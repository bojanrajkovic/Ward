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

        public static Message ParseFromBytes(byte[] message, int offset)
        {
            // Parsing messages requires parsing records, which means requiring
            // the entire message to work with the DNS compression scheme.
            using (var stream = new MemoryStream(message)) {
                stream.Position = offset;
                return ParseFromStream(message, stream);
            }
        }

        public static Message ParseFromStream(byte[] originalMessage, Stream stream)
        {
            var header = Header.ParseFromStream(stream);

            var questions = new Question[header.TotalQuestions];
            for (var i = 0; i < header.TotalQuestions; i++)
                questions[i] = Question.ParseFromStream(stream);

            var answers = new Record[header.TotalAnswerRecords];
            for (var i = 0; i < header.TotalAnswerRecords; i++)
                answers[i] = Record.ParseFromStream(originalMessage, stream);

            var authorities = new Record[header.TotalAuthorityRecords];
            for (var i = 0; i < header.TotalAuthorityRecords; i++)
                authorities[i] = Record.ParseFromStream(originalMessage, stream);

            var additional = new Record[header.TotalAdditionalRecords];
            for (var i = 0; i < header.TotalAdditionalRecords; i++)
                additional[i] = Record.ParseFromStream(originalMessage, stream);

            return new Message(header, questions, answers, authorities, additional);
        }

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

        public async Task<byte[]> SerializeAsync()
        {
            using (var s = new MemoryStream()) {
                var offsetMap = new Dictionary<string, int>();

                await Header.WriteToStreamAsync(s);

                foreach (var question in Questions) {
                    offsetMap.Add(question.Name, (int)s.Position);
                    await question.WriteToStreamAsync(s);
                }

                var records = Answers.Concat(Authority).Concat(Additional);
                foreach (var record in records)
                    await record.WriteToStreamAsync(s, offsetMap);

                return s.ToArray();
            }
        }
    }
}
