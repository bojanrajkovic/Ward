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
            foreach (var tomlTestCase in testCases.Items)
            {
                var name = tomlTestCase.Get<string>("name");
                var dataString = tomlTestCase.Get<string>("data");
                // Convert hex to bytes.
                var expectedData = Enumerable.Range(0, dataString.Length / 2)
                                             .Select(x => Byte.Parse(dataString.Substring(2 * x, 2), NumberStyles.HexNumber))
                                             .ToArray(); ;
                var offsetMap = tomlTestCase.Get<TomlTable>("offsetMap").ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Get<ushort>()
                );

                var recordType = tomlTestCase.Get<Dns.Type>("type");
                string rName = tomlTestCase.Get<string>("rName");
                uint timeToLive = tomlTestCase.Get<uint>("timeToLive");
                var record = GetRecord(tomlTestCase, recordType, rName, timeToLive);

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

        private static Record GetRecord(TomlTable data, Dns.Type recordType, string rName, uint timeToLive)
        {
            switch (recordType) {
                case Dns.Type.A:
                case Dns.Type.AAAA:
                    return new AddressRecord(rName, recordType, Class.Internet, timeToLive, IPAddress.Parse(data.Get<string>("address")));
                case Dns.Type.CAA:
                    return new CaaRecord(rName, Class.Internet, timeToLive, data.Get<bool>("critical"), data.Get<string>("tag"), data.Get<string>("value"));
                case Dns.Type.CNAME:
                    return new CnameRecord(rName, Class.Internet, timeToLive, data.Get<string>("hostname"));
                case Dns.Type.MX:
                    return new MailExchangerRecord(rName, Class.Internet, timeToLive, 5, data.Get<string>("hostname"));
                case Dns.Type.NS:
                    return new NsRecord(rName, Class.Internet, timeToLive, data.Get<string>("hostname"));
                case Dns.Type.PTR:
                    return new PtrRecord(rName, Class.Internet, timeToLive, data.Get<string>("hostname"));
                case Dns.Type.TXT:
                    return new TxtRecord(rName, Class.Internet, timeToLive, data.Get<string>("text"));
                case Dns.Type.SOA:
                    return new SoaRecord(
                        rName, Class.Internet, timeToLive,
                        data.Get<string>("primaryNs"), data.Get<string>("responsibleName"),
                        data.Get<uint>("serial"), data.Get<int>("refresh"),
                        data.Get<int>("retry"), data.Get<int>("expire"), data.Get<uint>("minimumTtl")
                    );
                default:
                    throw new Exception($"Don't know how to deal with record test case of type {recordType}.");
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
