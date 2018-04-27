using System;
using System.Collections.Generic;

using Nett;
using Ward.Tests.Core;
using Xunit;

namespace Ward.Dns.Tests
{
    static class TestGenerators
    {
        public static TheoryData<MessageManipulationTestCase> GenerateMessageTests()
        {
            bool TestCaseMatches(MessageManipulationTestCase tt) =>
                tt.RawTestCase.ContainsKey("complete") && tt.RawTestCase.Get<bool>("complete");

            var td = new TheoryData<MessageManipulationTestCase>();
            foreach (var test in TestCaseLoader.FindMessageTestCasesMatching(TestCaseMatches))
                td.Add(test);
            return td;
        }

        public static TheoryData<RecordSerializationTestCase> GenerateRecordSerializationTests()
        {
            var td = new TheoryData<RecordSerializationTestCase>();
            foreach (var serializationTestCase in TestCaseLoader.RecordSerializationTestCases)
                td.Add(serializationTestCase);
            return td;
        }
    }
}
