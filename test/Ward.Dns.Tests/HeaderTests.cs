using System;
using System.Collections.Generic;
using System.IO;

using Nett;
using Ward.Tests.Core;
using Xunit;

namespace Ward.Dns.Tests
{
    public class HeaderTests
    {
        [Theory]
        [MemberData(nameof(TestGenerators.GenerateHeaderTests), MemberType = typeof(TestGenerators))]
        public void Can_parse_header(string testCaseName, byte[] messageData, TomlTable testCaseData)
        {
            var header = Header.ParseFromStream(new MemoryStream(messageData));
            var expectedHeader = testCaseData.Get<TomlTable>("expected").Get<TomlTable>("header");

            Assert.Header(expectedHeader, header);
        }
    }
}
