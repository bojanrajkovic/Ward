using System;

using Nett;
using Ward.Tests.Core;
using Xunit;

namespace Ward.Dns.Tests
{
    static class TestGenerators
    {
        public static TheoryData<string, byte[], TomlTable> GenerateMessageTests()
        {
            bool TestCaseMatches(TomlTable tt) =>
                tt.ContainsKey("complete") && tt.Get<bool>("complete");

            var td = new TheoryData<string, byte[], TomlTable>();
            foreach (var test in TestCaseLoader.FindTestCasesMatching(TestCaseMatches))
                td.Add(test.Get<string>("name"), Convert.FromBase64String(test.Get<string>("data")), test);
            return td;
        }

        public static TheoryData<string, byte[], TomlTable> GenerateHeaderTests()
        {
            bool TestCaseMatches(TomlTable tt) =>
                tt.ContainsKey("expected") && tt.Get<TomlTable>("expected").ContainsKey("header");

            var td = new TheoryData<string, byte[], TomlTable>();
            foreach (var test in TestCaseLoader.FindTestCasesMatching(TestCaseMatches))
                td.Add(test.Get<string>("name"), Convert.FromBase64String(test.Get<string>("data")), test);
            return td;
        }
    }
}
