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

        [Fact]
        public async Task Extended_rcode_is_serialized_correctly() {
            var flags = new Header.HeaderFlags(true, false, false, true, true, false, false, false);
            var header = new Header(null, Opcode.Query, ReturnCode.BadTrunc, flags, 1, 0, 0, 0);
            var question = new Question("google.com.", Type.A, Class.Internet);
            var message = new Message(header, new [] { question }, Array.Empty<Record>(), Array.Empty<Record>(), Array.Empty<Record>());
            var serializedMessage = await MessageWriter.SerializeMessageAsync(message);

            var parsedMessage = MessageParser.ParseMessage(serializedMessage, 0);

            Assert.NotNull(parsedMessage);

            // The parser should have automatically fixed up the return code in the header.
            Assert.Equal(ReturnCode.BadTrunc, parsedMessage.Header.ReturnCode);

            // But if we inspect the bytes, we should find that only the 2nd and 3rd are set
            // in the actual header RCODE field.
            var flagsBitfield = Utils.SwapUInt16(BitConverter.ToUInt16(serializedMessage, 2));
            var returnCode = (ReturnCode)(flagsBitfield & 0b0000_0000_0000_1111);
            Assert.Equal((ReturnCode)0b0000_0110, returnCode);

            // The message writer should have automatically inserted an OPT record with
            // correct values.
            Assert.Equal(1, parsedMessage.Header.TotalAdditionalRecords);
            Assert.Single(parsedMessage.Additional);
            var optRecord = Assert.IsType<OptRecord>(parsedMessage.Additional[0]);
            Assert.Equal(1, optRecord.ExtendedRcode);
            Assert.Equal(0, optRecord.Edns0Version);
            Assert.Equal(4096, optRecord.UdpPayloadSize);
            Assert.Equal(0, optRecord.Length);
        }
    }
}
