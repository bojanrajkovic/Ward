using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;

using Nett;
using Ward.Dns;
using Ward.Dns.Records;

namespace Ward.Tests.Core
{
    public static class TestCaseLoader
    {
        static readonly TomlTable testCaseData;
        static readonly Dictionary<string, MessageManipulationTestCase> messageTestCases =
            new Dictionary<string, MessageManipulationTestCase>();

        static readonly Dictionary<string, RecordSerializationTestCase> recordTestCases =
            new Dictionary<string, RecordSerializationTestCase>();

        static TestCaseLoader()
        {
            var asm = typeof(TestCaseLoader).GetTypeInfo().Assembly;
            var resource = asm.GetManifestResourceStream("Ward.Tests.Core.TestCaseData.toml");

            using (var ms = new MemoryStream()) {
                resource.CopyTo(ms);
                ms.Position = 0;
                testCaseData = Toml.ReadStream(ms);
            }

            PopulateMessageManipulationTests();
            PopulateRecordSerializationTests();
        }

        static void PopulateMessageManipulationTests()
        {
            var testCases = testCaseData.Get<TomlTableArray>("message");
            foreach (var tomlTestCase in testCases.Items)
                messageTestCases.Add(
                    tomlTestCase.Get<string>("name"),
                    new MessageManipulationTestCase(
                        tomlTestCase.Get<string>("name"),
                        Convert.FromBase64String(tomlTestCase.Get<string>("data")),
                        tomlTestCase
                    )
                );
        }

        static void PopulateRecordSerializationTests()
        {
            var testCases = testCaseData.Get<TomlTableArray>("records");
            foreach (var tomlTestCase in testCases.Items) {
                var name = tomlTestCase.Get<string>("name");
                var dataString = tomlTestCase.Get<string>("data");
                // Convert hex to bytes.
                var expectedData = Enumerable.Range(0, dataString.Length/2)
                                             .Select(x => Byte.Parse(dataString.Substring(2*x, 2), NumberStyles.HexNumber))
                                             .ToArray();;
                var offsetMap = tomlTestCase.Get<TomlTable>("offsetMap").ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Get<ushort>()
                );

                Record record;
                var recordType = tomlTestCase.Get<Dns.Type>("type");
                switch (recordType) {
                    case Dns.Type.A:
                    case Dns.Type.AAAA:
                        record = new AddressRecord(
                            tomlTestCase.Get<string>("rName"),
                            recordType,
                            Class.Internet, // Maybe someday I will care about any other class.
                            tomlTestCase.Get<uint>("timeToLive"),
                            IPAddress.Parse(tomlTestCase.Get<string>("address"))
                        );
                        break;
                    case Dns.Type.CAA:
                        record = new CaaRecord(
                            tomlTestCase.Get<string>("rName"),
                            Class.Internet,
                            tomlTestCase.Get<uint>("timeToLive"),
                            tomlTestCase.Get<bool>("critical"),
                            tomlTestCase.Get<string>("tag"),
                            tomlTestCase.Get<string>("value")
                        );
                        break;
                    default:
                        throw new Exception($"Don't know how to deal with record test case of type {recordType}.");
                }

                recordTestCases.Add(
                    name,
                    new RecordSerializationTestCase(
                        name,
                        record,
                        expectedData,
                        offsetMap
                    )
                );
            }
        }

        public static IReadOnlyCollection<RecordSerializationTestCase> RecordSerializationTestCases =>
            recordTestCases.Values;

        public static MessageManipulationTestCase LoadMessageTestCase(string testCaseName) =>
            messageTestCases[testCaseName];

        public static IEnumerable<MessageManipulationTestCase> FindMessageTestCasesMatching(
            Predicate<MessageManipulationTestCase> predicate
        ) => messageTestCases.Values.Where(testCase => predicate(testCase));
    }
}
