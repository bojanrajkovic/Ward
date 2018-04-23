using System.Collections.Generic;

using Ward.Dns;

namespace Ward.Tests.Core
{
    public class RecordSerializationTestCase
    {

        public string Name { get; }
        public Record Record { get; }
        public byte[] ExpectedData { get; }
        public Dictionary<string, ushort> OffsetMap { get; }

        public override string ToString() => Name;

        public RecordSerializationTestCase(string name, Record record, byte[] expectedData, Dictionary<string, ushort> offsetMap)
        {
            Name = name;
            Record = record;
            ExpectedData = expectedData;
            OffsetMap = offsetMap;
        }
    }
}
