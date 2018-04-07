using System;
using Xunit;

namespace Aelfweard.Dns.Tests
{
    public class HeaderTests
    {
        const string testMessage = "qqqBgAABAAEAAAAAB2V4YW1wbGUDY29tAAABAAHADAABAAEAADu8AARduNgi";

        [Fact]
        public void Can_parse_header()
        {
            var message = Convert.FromBase64String(testMessage);
        }
    }
}
