using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Nett;
using Ward.Dns.Records;
using Ward.Tests.Core;
using Xunit;

namespace Ward.Dns.Tests
{
    public class MessageTests
    {
        const string responseMessage = "qqqBgAABAAEAAAAAB2V4YW1wbGUDY29tAAABAAHADAABAAEAADu8AARduNgi";

        [Fact]
        public async Task Can_serialize_message_correctly()
        {
            var headerFlags = new Header.HeaderFlags(false, false, false, true, true, false, false, false);
            var header = new Header(0xaaaa, Opcode.Query, ReturnCode.NoError, headerFlags, 1, 1, 0, 0);
            var question = new Question("example.com", Type.A, Class.Internet);
            var answer = new GenericRecord("example.com", Type.A, Class.Internet, 15292, 4, new byte[] { 93, 184, 216, 34 });
            var message = new Message(
                header,
                new [] { question },
                new IRecord[] { answer },
                Array.Empty<IRecord>(),
                Array.Empty<IRecord>()
            );

            var messageBody = Convert.FromBase64String(responseMessage);
            var serialized = await MessageWriter.SerializeMessageAsync(message);

            Assert.Equal(messageBody, serialized);
        }

        [Theory]
        [MemberData(nameof (TestGenerators.GenerateMessageTests), MemberType = typeof(TestGenerators))]
        public void Can_parse_whole_message(TestCase testCase)
        {
            var message = MessageParser.ParseMessage(testCase.MessageData, 0);
            var expected = testCase.RawTestCase.Get<TomlTable>("expected");
            var expectedHeader = expected.Get<TomlTable>("header");
            var expectedQuestions = (TomlTableArray)expected.TryGetValue("questions");
            var expectedAnswers = (TomlTableArray)expected.TryGetValue("answers");
            var expectedAuthorities = (TomlTableArray)expected.TryGetValue("authority");
            var expectedAdditional = (TomlTableArray)expected.TryGetValue("additional");

            Assert.Header(expectedHeader, message.Header);
            Assert.Equal(expectedQuestions.Count, message.Header.TotalQuestions);
            Assert.Equal(expectedAnswers?.Count ?? 0, message.Header.TotalAnswerRecords);
            Assert.Equal(expectedAuthorities?.Count ?? 0, message.Header.TotalAuthorityRecords);
            Assert.Equal(expectedAdditional?.Count ?? 0, message.Header.TotalAdditionalRecords);

            if (expectedQuestions != null)
                expectedQuestions.Items.ForEach((eq, idx) => Assert.Question(eq, message.Questions[idx]));

            if (expectedAnswers != null)
                expectedAnswers.Items.ForEach((er, idx) => Assert.Record(er, message.Answers[idx]));

            if (expectedAuthorities != null)
                expectedAuthorities.Items.ForEach((ea, idx) => Assert.Record(ea, message.Authority[idx]));

            if (expectedAdditional != null)
                expectedAdditional.Items.ForEach((ea, idx) => Assert.Record(ea, message.Additional[idx]));
        }
    }
}
