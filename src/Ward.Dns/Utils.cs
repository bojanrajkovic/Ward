using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Infrastructure;

using static System.Buffers.Binary.BinaryPrimitives;

namespace Ward.Dns
{
    /// <summary>
    /// DNS utilities.
    /// </summary>
    static class Utils
    {
        /// <summary>
        /// Parses an encoded DNS QNAME.
        /// </summary>
        /// <param name="message">The whole message being parsed.</param>
        /// <param name="data">The data subset to parse the QNAME from, if parsing from a subset.</param>
        /// <param name="offset">The offset at which to start parsing.</param>
        /// <param name="reverseOffsetMap">A reverse offset map for caching offset lookups.</param>
        /// <returns>
        /// A parsed name.
        /// </returns>
        /// <remarks>
        /// To make life easier, <see cref="ParseComplexName" /> can update the offset
        /// on its own. It's non-trivial to track for callers. Some callers
        /// may not know the offset in the message, in which case they should
        /// specify a non-null data array, and a 0 offset. Pointers will *always*
        /// be looked up in the message, regardless of the specified offset.
        /// </remarks>
        public static unsafe string ParseComplexName(
            ReadOnlySpan<byte> message,
            ReadOnlySpan<byte> data,
            ref int offset,
            Dictionary<int, string> reverseOffsetMap = null
        )
        {
            if (reverseOffsetMap == null)
                reverseOffsetMap = new Dictionary<int, string>();

            var readFrom = data.IsEmpty ? message : data;
            var nameBuilder = new StringBuilder(253);
            while (true) {
                // Read the current byte.
                var nextByte = readFrom[offset++];

                // Are the top two bits set? If so, this is a pointer somewhere
                // else in the message, and is the end of this complex name. Rewind
                // one position so we can read the 2 byte pointer.
                if ((nextByte & 0b1100_0000) == 0b1100_0000) {
                    // Read 2 bytes, _starting a byte behind us_ (because we already incremented)
                    // and then increment one byte to move past the 2-byte offset.
                    var ptrToOffset = ReadUInt16BigEndian(readFrom.Slice(offset++ - 1, sizeof(ushort)));
                    var offsetToName = ptrToOffset & 0b0011_1111_1111_1111;

                    nameBuilder.Append(
                        reverseOffsetMap.TryGetValue(offsetToName, out var existingName)
                            ? existingName
                            : ParseComplexName(message, null, ref offsetToName, reverseOffsetMap)
                    );
                    break;
                }

                // EON
                if (nextByte == 0)
                    break;

                // Otherwise, this is a number of bytes to read, so read it
                // and append the ASCII string to the builder. To read it from
                // ReadOnlySpan<byte>, we have to slice the buffer correctly at the current
                // read offset, then get a reference and use the standard GetString
                // call against byte*.
                var nameStr = new string('\0', nextByte);
                fixed (char *name = nameStr)
                fixed (byte *buffer = &MemoryMarshal.GetReference(readFrom.Slice(offset, nextByte))) {
                    StringUtilities.TryGetAsciiString(buffer, name, nextByte);
                    nameBuilder.Append(name, nextByte);
                    nameBuilder.Append('.');
                    offset += nextByte;
                }
            }

            return nameBuilder.ToString();
        }

        /// <summary>
        /// Writes an encoded QNAME to a byte[].
        /// </summary>
        /// <param name="name">The name to write.</param>
        /// <param name="offsetMap">The offset map to use for compressing the QNAME.</param>
        /// <returns>An encoded/compressed name.</returns>
        /// <remarks>
        /// Any names passed into WriteQName will be punycoded if need be, via the
        /// <see cref="IdnMapping"/> class.
        /// </remarks>
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

            if (offset != 0)
                WriteUInt16BigEndian(new Span<byte>(qname).Slice(pos), offset);

            return qname;
        }

        /// <summary>
        /// Efficiently concatenates an unbounded number of arrays via
        /// Buffer.BlockCopy.
        /// </summary>
        /// <param name="arrays">The arrays to concatenate.</param>
        /// <returns>A concatenated array consisting of all the input arrays.</returns>
        public static byte[] Concat(params byte[][] arrays)
        {
            var final = new byte[arrays.Sum(a => a.Length)];
            var offset = 0;
            foreach (byte[] array in arrays) {
                Buffer.BlockCopy(array, 0, final, offset, array.Length);
                offset += array.Length;
            }
            return final;
        }
    }
}
