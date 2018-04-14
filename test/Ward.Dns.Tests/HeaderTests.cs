using System;
using System.IO;
using Xunit;

namespace Ward.Dns.Tests
{
    /// <summary>
    /// Tests DNS wire format header parsing.
    /// TODO: Come up with some way to run these tests on _many_ inputs
    /// TODO: and outputs. Need to find some storage format that has the
    /// TODO: req/resp (base64 or hex) and expected response values. TOML?
    /// </summary>
    public class HeaderTests
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
        public void Can_parse_header_from_request()
        {
            var message = Convert.FromBase64String(requestMessage);
            var messageStream = new MemoryStream(message);
            var header = Header.ParseFromStream(messageStream);

            Assert.Equal(0x1dfa, header.Id);
            Assert.True(header.Query);
            Assert.Equal(Opcode.Query, header.Opcode);
            Assert.False(header.Truncated);
            Assert.True(header.Recurse);
            Assert.False(header.RecursionAvailable);
            Assert.False(header.Z);
            Assert.True(header.Authenticated);
            Assert.False(header.CheckingDisabled);
            Assert.Equal(ReturnCode.NoError, header.ReturnCode);

            Assert.Equal(1, header.TotalQuestions);
            Assert.Equal(0, header.TotalAnswerRecords);
            Assert.Equal(0, header.TotalAuthorityRecords);
            Assert.Equal(1, header.TotalAdditionalRecords);
        }

        [Fact]
        public void Can_parse_header_from_response()
        {
            var message = Convert.FromBase64String(responseMessage);
            var messageStream = new MemoryStream(message);
            var header = Header.ParseFromStream(messageStream);

            Assert.Equal(0xAAAA, header.Id);
            Assert.False(header.Query);
            Assert.Equal(Opcode.Query, header.Opcode);
            Assert.False(header.Authoritative);
            Assert.False(header.Truncated);
            Assert.True(header.Recurse);
            Assert.True(header.RecursionAvailable);
            Assert.False(header.Z);
            Assert.False(header.Authenticated);
            Assert.False(header.CheckingDisabled);
            Assert.Equal(ReturnCode.NoError, header.ReturnCode);

            Assert.Equal(1, header.TotalQuestions);
            Assert.Equal(1, header.TotalAnswerRecords);
            Assert.Equal(0, header.TotalAuthorityRecords);
            Assert.Equal(0, header.TotalAdditionalRecords);
        }
    }
}
