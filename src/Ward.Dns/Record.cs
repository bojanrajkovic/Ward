using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

using Ward.Dns.Records;
using static Ward.Dns.Utils;

namespace Ward.Dns
{
    public class Record
    {
        public string Name { get; }
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
                string name;
                if (nextByte == 0) {
                    // This record doesn't _have_ a name.
                    name = null;
                } else {
                    // Rewind a byte so we can actually read the 2-byte name offset.
                    stream.Position -= 1;
                    name = ParseComplexName(messageBytes, stream);
                }

                var type = (Type)SwapUInt16(binReader.ReadUInt16());
                var @class = (Class)SwapUInt16(binReader.ReadUInt16());
                var ttl = SwapUInt32(binReader.ReadUInt32());
                var dataLength = SwapUInt16(binReader.ReadUInt16());
                var data = binReader.ReadBytes(dataLength);

                return RecordFactory.Create(
                    name,
                    type,
                    @class,
                    ttl,
                    dataLength,
                    data,
                    messageBytes
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
            Name = name;
            Type = type;
            Class = @class;
            TimeToLive = timeToLive;
            Length = length;
            Data = data;
        }

        internal async Task WriteToStreamAsync(Stream s, Dictionary<string, int> offsetMap)
        {
            if (string.IsNullOrWhiteSpace(Name)) {
                s.WriteByte(0);
            } else {
                // TODO: Fix this scheme. We need to look for subprefixes in the
                // TODO: offset map and compact the names.
                var nameOffset = (ushort)(0b1100_0000_0000_0000 | offsetMap[Name]);

                if (!offsetMap.ContainsKey(Name))
                    offsetMap.Add(Name, (int)s.Position);

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
