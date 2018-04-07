using System;
using Xunit;

using Aelfweard.Dns;

namespace Aelfweard.Dns.Tests
{
    public class DnsNameTests
    {
        const string testMessage = "qqqBgAABAAEAAAAAB2V4YW1wbGUDY29tAAABAAHADAABAAEAADu8AARduNgi";

        [Fact]
        public void Can_read_name_from_RR_correctly()
        {
            var message = Convert.FromBase64String(testMessage);
            var name = new Name(message, (ushort)0xc00c);
            var nameString = name.ToString();

            Assert.Equal("example.com", nameString);
        }
    }
}
