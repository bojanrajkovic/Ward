using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

using Nett;

namespace Ward.Tests.Core
{
    public static class TestCaseLoader
    {
        static readonly TomlTable testCaseData;
        static readonly Dictionary<string, TomlTable> testCaseMapping = new Dictionary<string, TomlTable>();

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
                testCaseMapping.Add(tomlTestCase.Get<string>("name"), tomlTestCase);
        }

        public static TomlTable LoadTestCase(string testCaseName) =>
            testCaseMapping[testCaseName];
    }
}
