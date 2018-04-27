using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

string Str(byte[] bytes) => BitConverter.ToString(bytes).Replace("-", string.Empty).ToLower();
byte[] Ascii(string str) => Encoding.ASCII.GetBytes(str);

byte[] Concat(params byte[][] arrays)
{
    var final = new byte[arrays.Sum(a => a.Length)];
    var offset = 0;
    foreach (byte[] array in arrays) {
        Buffer.BlockCopy(array, 0, final, offset, array.Length);
        offset += array.Length;
    }
    return final;
}

ushort SwapUInt16(ushort x) =>
    BitConverter.IsLittleEndian ? (ushort)((ushort)((x & 0xff) << 8) | ((x >> 8) & 0xff)) : x;
uint SwapUInt32(uint x) =>
    BitConverter.IsLittleEndian ? ((x & 0x000000ff) << 24) +
                                  ((x & 0x0000ff00) << 8) +
                                  ((x & 0x00ff0000) >> 8) +
                                  ((x & 0xff000000) >> 24) : x;
byte[] WriteQName(string name, Dictionary<string, ushort> offsetMap = null)
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
