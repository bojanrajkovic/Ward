using System;
using System.Collections.Generic;
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
                    var ptrToOffset = SwapUInt16(BitConverter.ToUInt16(readFrom, (offset++) - 1));
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

        public static byte[] WriteQName(string name, Dictionary<string, ushort> offsetMap)
        {
            if (string.IsNullOrWhiteSpace(name))
                return new byte[] { 0 };

            offsetMap = offsetMap ?? new Dictionary<string, ushort>();

            // Convert the name to Punycode.
            name = new IdnMapping().GetAscii(name);

            // Find the longest name that's a suffix of this one
            string longestSuffix = null;
            foreach (var existingName in offsetMap.Keys) {
                if (!name.EndsWith(existingName))
                    continue;

                if (existingName.Length > (longestSuffix?.Length ?? 0))
                    longestSuffix = existingName;
            }

            // Strip the longest suffix from the end of the name.
            name = name.Substring(0, name.Length - (longestSuffix?.Length ?? 0));
            var offset = (ushort)(longestSuffix != null ? (0b1100_0000_0000_0000 | offsetMap[longestSuffix]) : 0);

            var labels = name.Split(new [] { '.', }, StringSplitOptions.RemoveEmptyEntries);
            var qnameLength = labels.Sum(l => 1+l.Length);

            // If the offset is non-zero, append 2 bytes to the length of the labels.
            // We'll write the ASCII labels, and then the 2 byte offset. If the offset is 0,
            // we're writing the entire string, so we only need to add 1 byte to the length,
            // for the null terminator.
            qnameLength += offset == 0 ? 1 : 2;

            var qname = new byte[qnameLength];

            // Write any labels to the output.
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

            if (offset != 0) {
                var offsetBytes = BitConverter.GetBytes(SwapUInt16(offset));
                Buffer.BlockCopy(offsetBytes, 0, qname, pos, offsetBytes.Length);
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
