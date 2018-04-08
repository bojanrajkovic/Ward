using System;
using System.IO;
using System.Linq;
using Xunit;

namespace Aelfweard.Dns.Tests
{
    public class MessageTests
    {
        const string requestMessage = "HfoBIAABAAAAAAABBmdvb2dsZQNjb20AAAEAAQAAKRAAAAAAAAAMAAoACJOxHvPq368a";

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
