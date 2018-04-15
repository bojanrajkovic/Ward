using Nett;
using Xunit;

namespace Ward.Dns.Tests
{
    static class Assertions
    {
        public static void AssertHeader(Header header, TomlTable expectedHeader)
        {
            Assert.Equal(expectedHeader.Get<ushort>("id"), header.Id);
            Assert.Equal(expectedHeader.Get<bool>("query"), header.Query);
            Assert.Equal(expectedHeader.Get<bool>("authoritative"), header.Authoritative);
            Assert.Equal(expectedHeader.Get<Opcode>("opcode"), header.Opcode);
            Assert.Equal(expectedHeader.Get<bool>("truncated"), header.Truncated);
            Assert.Equal(expectedHeader.Get<bool>("recurse"), header.Recurse);
            Assert.Equal(expectedHeader.Get<bool>("recursionAvailable"), header.RecursionAvailable);
            Assert.Equal(expectedHeader.Get<bool>("z"), header.Z);
            Assert.Equal(expectedHeader.Get<bool>("authenticated"), header.Authenticated);
            Assert.Equal(expectedHeader.Get<bool>("checkingDisabled"), header.CheckingDisabled);
            Assert.Equal(expectedHeader.Get<ReturnCode>("returnCode"), header.ReturnCode);

            Assert.Equal(expectedHeader.Get<int>("questions"), header.TotalQuestions);
            Assert.Equal(expectedHeader.Get<int>("answers"), header.TotalAnswerRecords);
            Assert.Equal(expectedHeader.Get<int>("authority"), header.TotalAuthorityRecords);
            Assert.Equal(expectedHeader.Get<int>("additional"), header.TotalAdditionalRecords);
        }
    }
}
