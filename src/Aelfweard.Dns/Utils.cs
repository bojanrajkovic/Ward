using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace Aelfweard.Dns
{
    static class Utils
    {
        public static string ParseComplexName(byte[] message, byte[] name, int offset)
        {
            var pos = offset;
            using (var memoryStream = new MemoryStream(name)) {
                memoryStream.Position = offset;
                return ParseComplexName(message, memoryStream);
            }
        }

        public static string ParseComplexName(byte[] message, Stream stream)
        {
            var nameBuilder = new StringBuilder();
            using (var binaryReader = new BinaryReader(stream, Encoding.ASCII, true)) {
                while (true) {
                    // Read the current byte.
                    var nextByte = binaryReader.ReadByte();

                    // Are the top two bits set? If so, this is a pointer somewhere
                    // else in the message, and is the end of this complex name. Rewind
                    // one position so we can read the 2 byte pointer.
                    if ((nextByte & 0b1100_0000) == 0b1100_0000) {
                        stream.Position--;
                        var ptrOffset = SwapUInt16(binaryReader.ReadUInt16());
                        var offsetToName = (ushort)(ptrOffset & 0b0011_1111_1111_1111);
                        nameBuilder.Append(Utils.ParseComplexName(message, message, offsetToName));
                        break;
                    } if (nextByte == 0) {
                        // EOF
                        break;
                    } else {
                        // Otherwise, this is a number of bytes to read, so read it
                        // and append the ASCII string to the builder.
                        nameBuilder.Append(Encoding.ASCII.GetString(binaryReader.ReadBytes(nextByte)));
                        nameBuilder.Append('.');
                    }
                }
            }

            return nameBuilder.ToString();
        }

        public static byte[] WriteQName(string name)
        {
            var punycode = new IdnMapping().GetAscii(name);
            var labels = punycode.Split('.');
            var qname = new byte[labels.Sum(l => 1+l.Length) + 1];

            var pos = 0;
            foreach (var label in labels) {
                qname[pos++] = (byte)label.Length;
                Buffer.BlockCopy(
                    Encoding.ASCII.GetBytes(label),
                    0,
                    qname,
                    pos,
                    label.Length
                );
                pos += label.Length;
            }

            return qname;
        }

        public static ushort SwapUInt16(ushort x) =>
            BitConverter.IsLittleEndian ? (ushort)((ushort)((x & 0xff) << 8) | ((x >> 8) & 0xff)) : x;

        public static uint SwapUInt32(uint x) =>
            BitConverter.IsLittleEndian ? ((x & 0x000000ff) << 24) +
                                          ((x & 0x0000ff00) << 8) +
                                          ((x & 0x00ff0000) >> 8) +
                                          ((x & 0xff000000) >> 24) : x;
    }
}
