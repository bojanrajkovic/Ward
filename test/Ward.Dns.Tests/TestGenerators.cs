using System;

using Nett;
using Ward.Tests.Core;
using Xunit;

namespace Ward.Dns.Tests
{
    static class TestGenerators
    {
        public static TheoryData<TestCase> GenerateMessageTests()
        {
            bool TestCaseMatches(TestCase tt) =>
                tt.RawTestCase.ContainsKey("complete") && tt.RawTestCase.Get<bool>("complete");

            var td = new TheoryData<TestCase>();
            foreach (var test in TestCaseLoader.FindTestCasesMatching(TestCaseMatches))
                td.Add(test);
            return td;
        }
    }
}
