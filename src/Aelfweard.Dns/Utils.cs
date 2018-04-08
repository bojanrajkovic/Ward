using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace Aelfweard.Dns
{
    static class Utils
    {
        public static string ParseQName(Stream stream)
        {
            using (var reader = new BinaryReader(stream, Encoding.ASCII, true)) {
                var nameBuilder = new StringBuilder();

                while (true) {
                    var nextByte = reader.ReadByte();

                    // Null terminator ends the QNAME section.
                    if (nextByte == 0)
                        break;

                    var labelBytes = reader.ReadBytes(nextByte);
                    nameBuilder.Append(Encoding.ASCII.GetString(labelBytes));
                    nameBuilder.Append('.');
                }

                return nameBuilder.Remove(nameBuilder.Length-1, 1).ToString();
            }
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
