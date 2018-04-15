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
        [MemberData(nameof(GenerateHeaderTests))]
        public void Can_parse_header(TomlTable testCaseData)
        {
            var message = Convert.FromBase64String(testCaseData.Get<string>("data"));
            var header = Header.ParseFromStream(new MemoryStream(message));
            var expectedHeader = testCaseData.Get<TomlTable>("expected").Get<TomlTable>("header");

            Assert.Header(expectedHeader, header);
        }

        public static TheoryData<TomlTable> GenerateHeaderTests()
        {
            bool TestCaseMatches(TomlTable tt) =>
                tt.ContainsKey("expected") && tt.Get<TomlTable>("expected").ContainsKey("header");

            var td = new TheoryData<TomlTable>();
            foreach (var test in TestCaseLoader.FindTestCasesMatching(TestCaseMatches))
                td.Add(test);
            return td;
        }
    }
}
