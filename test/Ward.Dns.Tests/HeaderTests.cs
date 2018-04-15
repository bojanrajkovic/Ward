using System;
using System.IO;

using Nett;
using Ward.Tests.Core;
using Xunit;

namespace Ward.Dns.Tests
{
    /// <summary>
    /// Tests DNS wire format header parsing.
    /// </summary>
    public class HeaderTests
    {
        [Fact]
        public void Can_parse_header_from_request()
        {
            var testCaseData = TestCaseLoader.LoadTestCase("google.com-a-query");
            var message = Convert.FromBase64String(testCaseData.Get<string>("data"));
            var header = Header.ParseFromStream(new MemoryStream(message));
            var expectedHeader = testCaseData.Get<TomlTable>("expected").Get<TomlTable>("header");

            Assertions.AssertHeader(header, expectedHeader);
        }

        [Fact]
        public void Can_parse_header_from_response()
        {
            var testCaseData = TestCaseLoader.LoadTestCase("example.com-a-response-from-8.8.8.8");
            var messageData = testCaseData.Get<string>("data");
            var messageStream = new MemoryStream(Convert.FromBase64String(messageData));
            var header = Header.ParseFromStream(messageStream);

            Assertions.AssertHeader(header, testCaseData.Get<TomlTable>("expected").Get<TomlTable>("header"));
        }
    }
}
