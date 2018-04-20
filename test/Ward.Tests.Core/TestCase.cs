using Nett;

namespace Ward.Tests.Core
{
    public class TestCase
    {
        public string MessageName { get; }
        public byte[] MessageData { get; }
        public TomlTable RawTestCase { get; }

        public TestCase(string messageName, byte[] messageData, TomlTable rawTestCase)
        {
            MessageName = messageName;
            MessageData = messageData;
            RawTestCase = rawTestCase;
        }

        public override string ToString() => MessageName;
    }
}
