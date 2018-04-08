using System.IO;
using System.Text;

using static Aelfweard.Dns.Utils;

namespace Aelfweard.Dns
{
    public class Record
    {
        public Name Name { get; }
        public Type Type { get; }
        public Class Class { get; }
        public uint TimeToLive { get; }
        public ushort Length { get; }
        public byte[] Data { get; }

        public static Record ParseFromBytes(byte[] messageBytes, int offset)
        {
            using (var stream = new MemoryStream(messageBytes)) {
                stream.Position = offset;
                return ParseFromStream(messageBytes, stream);
            }
        }

        // Record is special, because the `Name` is actually a pointer to somewhere
        // else in the message. Thus, Record needs the entire message, even when
        // stream parsing.
        public static Record ParseFromStream(byte[] messageBytes, Stream stream)
        {
            using (var binReader = new BinaryReader(stream, Encoding.ASCII, true)) {
                // Records may not have names, if they don't, there'll be a single
                // zero byte where the offset would be.
                var nextByte = binReader.ReadByte();
                Name name;
                if (nextByte == 0) {
                    // This record doesn't _have_ a name.
                    name = null;
                } else {
                    var nameOffset = SwapUInt16(binReader.ReadUInt16());
                    name = new Name(messageBytes, nameOffset);
                }

                var type = (Type)SwapUInt16(binReader.ReadUInt16());
                var @class = (Class)SwapUInt16(binReader.ReadUInt16());
                var ttl = SwapUInt32(binReader.ReadUInt32());
                var dataLength = SwapUInt16(binReader.ReadUInt16());
                var data = binReader.ReadBytes(dataLength);

                return new Record(
                    name,
                    type,
                    @class,
                    ttl,
                    dataLength,
                    data
                );
            }
        }

        public Record(
            Name name,
            Type type,
            Class @class,
            uint timeToLive,
            ushort length,
            byte[] data
        ) {
            Name = name;
            Type = type;
            Class = @class;
            TimeToLive = timeToLive;
            Length = length;
            Data = data;
        }
    }
}
