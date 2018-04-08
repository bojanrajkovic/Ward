using System;
using System.Linq;
using Xunit;

namespace Aelfweard.Dns.Tests
{
    public class RecordTests
    {
        /// <summary>
        /// A Base64-encoded DNS response from 8.8.8.8, having been asked for example.com.
        /// </summary>
        const string responseMessage = "qqqBgAABAAEAAAAAB2V4YW1wbGUDY29tAAABAAHADAABAAEAADu8AARduNgi";

        /// <summary>
        /// A Base64-encoded DNS query message, captured from Dig 9.11.3 on Fedora 29.
        /// The request is for google.com, and was sent as `dig google.com`
        /// </summary>
        const string requestMessage = "HfoBIAABAAAAAAABBmdvb2dsZQNjb20AAAEAAQAAKRAAAAAAAAAMAAoACJOxHvPq368a";

        [Fact]
        public void Can_parse_record_from_request()
        {
            var message = Convert.FromBase64String(requestMessage);
            var record = Record.ParseFromBytes(message, 28);

            Assert.Null(record.Name);
            Assert.Equal(Type.OPT, record.Type);

            // OPT records are weird, y'all.
            Assert.Equal((Class)4096, record.Class);
            Assert.Equal(0u, record.TimeToLive);
            Assert.Equal(12, record.Length);

            // Eventually, I'll add proper record classes and
            // we can check this data.
            Assert.Equal("000a000893b11ef3eadfaf1a", record.Data.Aggregate(string.Empty, (s, v) => {
                return s += v.ToString("X2").ToLower();
            }));
        }

        [Fact]
        public void Can_parse_record_from_response()
        {
            var message = Convert.FromBase64String(responseMessage);
            var record = Record.ParseFromBytes(message, 28);

            Assert.Equal("example.com", record.Name.ToString());
            Assert.Equal(Type.A, record.Type);
            Assert.Equal(Class.Internet, record.Class);
            Assert.Equal(15292u, record.TimeToLive);
            Assert.Equal(4, record.Length);
            Assert.Equal(new byte[] { 93, 184, 216, 34 }, record.Data);
        }
    }
}
