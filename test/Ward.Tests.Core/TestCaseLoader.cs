using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

using Nett;

namespace Ward.Tests.Core
{
    public static class TestCaseLoader
    {
        static readonly TomlTable testCaseData;
        static readonly Dictionary<string, TestCase> testCaseMapping = new Dictionary<string, TestCase>();

        static TestCaseLoader()
        {
            var asm = typeof(TestCaseLoader).GetTypeInfo().Assembly;
            var resource = asm.GetManifestResourceStream("Ward.Tests.Core.TestCaseData.toml");

            using (var ms = new MemoryStream()) {
                resource.CopyTo(ms);
                ms.Position = 0;
                testCaseData = Toml.ReadStream(ms);
            }

            var testCases = testCaseData.Get<TomlTableArray>("testcase");
            foreach (var tomlTestCase in testCases.Items)
                testCaseMapping.Add(
                    tomlTestCase.Get<string>("name"),
                    new TestCase(
                        tomlTestCase.Get<string>("name"),
                        Convert.FromBase64String(tomlTestCase.Get<string>("data")),
                        tomlTestCase
                    )
                );
        }

        public static TestCase LoadTestCase(string testCaseName) =>
            testCaseMapping[testCaseName];

        public static IEnumerable<TestCase> FindTestCasesMatching(Predicate<TestCase> predicate) =>
            testCaseMapping.Values.Where(testCase => predicate(testCase));
    }
}
