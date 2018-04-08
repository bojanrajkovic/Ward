using System;
using System.IO;

using Xunit;

namespace Aelfweard.Dns.Tests
{
    public class QuestionTests
    {
        const string requestMessage = "HfoBIAABAAAAAAABBmdvb2dsZQNjb20AAAEAAQAAKRAAAAAAAAAMAAoACJOxHvPq368a";

        [Fact]
        public void Can_parse_question_section_from_request()
        {
            var message = Convert.FromBase64String(requestMessage);
            var messageStream = new MemoryStream(message);

            // The header ends 12 bytes in, so set the stream there.
            messageStream.Position = 12;

            var question = Question.ParseFromStream(messageStream);

            Assert.Equal("google.com.", question.Name);
            Assert.Equal(Class.Internet, question.Class);
            Assert.Equal(Type.A, question.Type);
        }
    }
}
