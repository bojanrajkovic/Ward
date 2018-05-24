using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using static Ward.Dns.Utils;

namespace Ward.Dns.Records
{
    /// <summary>
    /// Creates an EDNS0 OPT pseudo-RR.
    /// </summary>
    public class OptRecord : Record
    {
        /// <summary>
        /// An EDNS0 option code.
        /// </summary>
        public enum OptionCode : ushort
        {
            LLQ = 1,
            UL = 2,
            NSID = 3,
            DAU = 5,
            DHU = 6,
            N3U = 7,
            EdnsClientSubnet = 8,
            EdnsExpire = 9,
            Cookie = 10,
            EdnsTcpKeepalive = 11,
            Padding = 12,
            Chain = 13,
            EdnsKeyTag = 14
        }

        /// <summary>
        /// A list of option code/option data pairs.
        /// </summary>
        /// <param name="optionCode">The option code.</param>
        /// <param name="optionData">The option data.</param>
        public IReadOnlyList<(OptionCode optionCode, ReadOnlyMemory<byte> optionData)> OptionalData { get; }

        /// <summary>The UDP payload size.</summary>
        /// <returns>The UDP payload size.</returns>
        public ushort UdpPayloadSize { get; }

        /// <summary>The EDNS0 version number.</summary>
        /// <returns>The EDNS0 version number.</returns>
        public byte Edns0Version { get; }

        /// <summary>
        /// The extended RCODE bits.
        /// </summary>
        /// <returns>The extended RCODE bits.</returns>
        public byte ExtendedRcode { get; }

        /// <summary>
        /// Indicates if DNSSEC RRs should be included in the response message.
        /// </summary>
        /// <returns>
        /// <c>true</c> if DNSSEC RRs should be included, <c>false</c> otherwise.
        /// </returns>
        public bool DnsSecOk { get; }

        /// <summary>
        /// Creates an OPT EDNS0 pseudo-RR.
        /// </summary>
        /// <remarks>
        /// Only used fron internal parsing code.
        /// </remarks>
        internal unsafe OptRecord(
            string name,
            ushort payloadSize,
            uint rcodeAndFlags,
            ushort length,
            ReadOnlyMemory<byte> data
        ) : base(name, Dns.Type.OPT, (Class)payloadSize, rcodeAndFlags, length, data) {
            UdpPayloadSize = payloadSize;
            ExtendedRcode = (byte)(rcodeAndFlags >> 24);
            Edns0Version = (byte)(rcodeAndFlags >> 16);
            DnsSecOk = (((ushort)rcodeAndFlags) >> 15) == 1;

            var optionalData = new List<(OptionCode optionCode, ReadOnlyMemory<byte> optionData)>();
            using (var pin = data.Pin()) {
                var dataPtr = (byte*)pin.Pointer;
                var pos = 0;

                while (pos < data.Length) {
                    var optionCode = (OptionCode)SwapUInt16(*(ushort*)(dataPtr + pos));
                    pos += 2;
                    var optionLength = SwapUInt16(*(ushort*)(dataPtr + pos));
                    pos += 2;
                    var optionData = data.Slice(pos, optionLength);
                    pos += optionLength;
                    optionalData.Add((optionCode, optionData));
                }
            }
            OptionalData = optionalData.AsReadOnly();
        }

        /// <summary>
        /// Converts the current record to a string.
        /// </summary>
        /// <returns>A string version of the current record.</returns>
        [System.Diagnostics.DebuggerStepThrough]
        public override string ToString() => $"";
    }
}
