using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Xunit;

namespace Aelfweard.Dns.Tests
{
    public class MessageTests
    {
        const string requestMessage = "HfoBIAABAAAAAAABBmdvb2dsZQNjb20AAAEAAQAAKRAAAAAAAAAMAAoACJOxHvPq368a";
        const string responseMessage = "qqqBgAABAAEAAAAAB2V4YW1wbGUDY29tAAABAAHADAABAAEAADu8AARduNgi";

        [Fact]
        public async Task Can_serialize_message_correctly()
        {
            var header = new Header(
                0xaaaa,
                false,
                Opcode.Query,
                false,
                false,
                true,
                true,
                false,
                false,
                false,
                ReturnCode.NoError,
                1,
                1,
                0,
                0
            );
            var question = new Question("example.com", Type.A, Class.Internet);
            var answer = new Record("example.com", Type.A, Class.Internet, 15292, 4, new byte[] { 93, 184, 216, 34 });
            var message = new Message(header, new [] { question }, new [] { answer }, Array.Empty<Record>(), Array.Empty<Record>());

            var messageBody = Convert.FromBase64String(responseMessage);
            var serialized = await message.SerializeAsync();

            Assert.Equal(messageBody, serialized);
        }

        [Fact]
        public void Can_parse_whole_message()
        {
            var messageBytes = Convert.FromBase64String(requestMessage);
            var messageStream = new MemoryStream(messageBytes);
            var message = Message.ParseFromStream(messageBytes, messageStream);

            Assert.Single(message.Questions);
            Assert.Empty(message.Answers);
            Assert.Empty(message.Authority);
            Assert.Single(message.Additional);

            var question = message.Questions[0];
            Assert.Equal("google.com", question.Name);
            Assert.Equal(Class.Internet, question.Class);
            Assert.Equal(Type.A, question.Type);

            var addtl = message.Additional[0];
            Assert.Null(addtl.Name);
            Assert.Equal(Type.OPT, addtl.Type);

            // OPT records are weird, y'all.
            Assert.Equal((Class)4096, addtl.Class);
            Assert.Equal(0u, addtl.TimeToLive);
            Assert.Equal(12, addtl.Length);

            // Eventually, I'll add proper record classes and
            // we can check this data.
            Assert.Equal("000a000893b11ef3eadfaf1a", addtl.Data.Aggregate(string.Empty, (s, v) => {
                return s += v.ToString("X2").ToLower();
            }));
        }
    }
}
