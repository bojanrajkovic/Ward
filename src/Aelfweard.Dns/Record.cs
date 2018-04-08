using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

using static Aelfweard.Dns.Utils;

namespace Aelfweard.Dns
{
    public class Record
    {
        public Name Name { get; }
        public string StringName { get; }
        public Type Type { get; }
        public Class Class { get; }
        public uint TimeToLive { get; }
        public ushort Length { get; }
        public byte[] Data { get; }

        // Record is special, because the `Name` is actually a pointer to somewhere
        // else in the message. Thus, Record needs the entire message, even when
        // stream parsing.
        internal static Record ParseFromStream(byte[] messageBytes, Stream stream)
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
                    // Rewind a byte so we can actually read the 2-byte name offset.
                    stream.Position -= 1;
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
            string name,
            Type type,
            Class @class,
            uint timeToLive,
            ushort length,
            byte[] data
        ) {
            Name = null;
            StringName = name;
            Type = type;
            Class = @class;
            TimeToLive = timeToLive;
            Length = length;
            Data = data;
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

        internal async Task WriteToStreamAsync(Stream s, Dictionary<string, int> offsetMap)
        {
            if (string.IsNullOrWhiteSpace(StringName) && Name == null) {
                s.WriteByte(0);
            } else {
                var name = StringName ?? Name.ToString();
                var nameOffset = (ushort)(0b1100_0000_0000_0000 | offsetMap[name]);
                await s.WriteAsync(BitConverter.GetBytes(SwapUInt16(nameOffset)), 0, 2);
            }

            await s.WriteAsync(BitConverter.GetBytes(SwapUInt16((ushort)Type)), 0, 2);
            await s.WriteAsync(BitConverter.GetBytes(SwapUInt16((ushort)Class)), 0, 2);
            await s.WriteAsync(BitConverter.GetBytes(SwapUInt32(TimeToLive)), 0, 4);
            await s.WriteAsync(BitConverter.GetBytes(SwapUInt16(Length)), 0, 2);
            await s.WriteAsync(Data, 0, Data.Length);
        }
    }
}
