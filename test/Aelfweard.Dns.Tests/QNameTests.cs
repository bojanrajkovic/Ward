using Xunit;

namespace Aelfweard.Dns.Tests
{
    public class QNameTests
    {
        [Theory]
        [InlineData("🍆👊.ws", new byte[] { 0x0a, 0x78, 0x6e, 0x2d, 0x2d, 0x67, 0x69, 0x38, 0x68, 0x69, 0x71, 0x02, 0x77, 0x73, 0x00 })]
        [InlineData("ssl.gstatic.com", new byte[] { 0x03, 0x73, 0x73, 0x6c, 0x07, 0x67, 0x73, 0x74, 0x61, 0x74, 0x69, 0x63, 0x03, 0x63, 0x6f, 0x6d, 0x00 })]
        public void Can_write_qname(string name, byte[] expectedQName)
        {
            var qname = Utils.WriteQName(name);
            Assert.Equal(expectedQName, qname);
        }
    }
}
