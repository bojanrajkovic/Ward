using Nett;

namespace Ward.Tests.Core
{
    public class MessageManipulationTestCase
    {
        public string MessageName { get; }
        public byte[] MessageData { get; }
        public TomlTable RawTestCase { get; }

        public MessageManipulationTestCase(string messageName, byte[] messageData, TomlTable rawTestCase)
        {
            MessageName = messageName;
            MessageData = messageData;
            RawTestCase = rawTestCase;
        }

        public override string ToString() => MessageName;
    }
}
