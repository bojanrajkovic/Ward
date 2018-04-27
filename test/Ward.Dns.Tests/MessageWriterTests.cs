using System;
using System.IO;
using System.Threading.Tasks;

using Ward.Dns.Records;
using Ward.Tests.Core;
using Xunit;

namespace Ward.Dns.Tests
{

    public class MessageWriterTests
    {
        [Theory]
        [MemberData(nameof (TestGenerators.GenerateRecordSerializationTests), MemberType = typeof (TestGenerators))]
        public async Task Can_serialize_record_correctly(RecordSerializationTestCase testCase) {
            var fakeStream = new MemoryStream();
            await MessageWriter.WriteRecordToStreamAsync(
                testCase.Record,
                fakeStream,
                testCase.OffsetMap
            );
            var recordData = fakeStream.ToArray();
            Assert.Equal(testCase.ExpectedData, recordData);
        }
    }
}
