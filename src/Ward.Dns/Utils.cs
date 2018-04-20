using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace Ward.Dns
{
    static class Utils
    {
        // To make life easier, ParseComplexName can update the offset
        // on its own. It's non-trivial to track for callers. Some callers
        // may not know the offset in the message, in which case they should
        // specify a non-null data array, and a 0 offset. Pointers will *always*
        // be looked up in the message, regardless of the specified offset.
        public static string ParseComplexName(byte[] message, byte[] data, ref int offset)
        {
            var readFrom = data ?? message;
            var nameBuilder = new StringBuilder();
            while (true) {
                // Read the current byte.
                var nextByte = readFrom[offset++];

                // Are the top two bits set? If so, this is a pointer somewhere
                // else in the message, and is the end of this complex name. Rewind
                // one position so we can read the 2 byte pointer.
                if ((nextByte & 0b1100_0000) == 0b1100_0000) {
                    // Read 2 bytes, _starting a byte behind us_ (because we already incremented)
                    // and then increment one byte to move past the 2-byte offset.
                    var ptrToOffset = SwapUInt16(BitConverter.ToUInt16(message, (offset++) - 1));
                    var offsetToName = (ptrToOffset & 0b0011_1111_1111_1111);

                    // Refs are into the message, not into the data.
                    nameBuilder.Append(ParseComplexName(message, null, ref offsetToName));
                    break;
                } else if (nextByte == 0) {
                    // EON
                    break;
                }

                // Otherwise, this is a number of bytes to read, so read it
                // and append the ASCII string to the builder.
                nameBuilder.Append(Encoding.ASCII.GetString(readFrom, offset, nextByte));
                nameBuilder.Append('.');
                offset += nextByte;
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
