using System;
using System.Collections.Generic;

using static Ward.Dns.Utils;

namespace Ward.Dns.Records
{
    /// <summary>
    /// An EDNS0 OPT pseudo-RR.
    /// </summary>
    /// <seealso cref="Record" />
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
        /// Gets a list of option code/option data pairs.
        /// </summary>
        /// <value>
        /// The list of option code/option data pairs.
        /// </value>
        /// <param name="optionCode">The option code.</param>
        /// <param name="optionData">The option data.</param>
        public IReadOnlyList<(OptionCode optionCode, ReadOnlyMemory<byte> optionData)> OptionalData { get; }

        /// <summary>
        /// Gets the UDP payload size.
        /// </summary>
        /// <value>
        /// The size of the UDP payload.
        /// </value>
        public ushort UdpPayloadSize { get; }

        /// <summary>
        /// Gets the EDNS0 version number.
        /// </summary>
        /// <value>
        /// The EDNS0 version number.
        /// </value>
        public byte Edns0Version { get; }

        /// <summary>
        /// Gets the extended RCODE bits.
        /// </summary>
        /// <value>
        /// The extended RCODE bits.
        /// </value>
        public byte ExtendedRcode { get; }

        /// <summary>
        /// Gets a value indicating if DNSSEC RRs should be included in the response message.
        /// </summary>
        /// <value>
        ///   <c>true</c> if DNSSEC RRs should be included in the response; otherwise, <c>false</c>.
        /// </value>
        public bool DnsSecOk { get; }

        /// <summary>
        /// Creates an OPT EDNS0 pseudo-RR.
        /// </summary>
        /// <param name="name">The owner-name or label to which this record belongs.</param>
        /// <param name="payloadSize">The max UDP payload size supported by this resolver.</param>
        /// <param name="rcodeAndFlags">The RCODE and flags data.</param>
        /// <param name="length">The length of the optional data.</param>
        /// <param name="data">The optional data.</param>
        /// <remarks>
        /// Only used fron internal parsing code.
        /// </remarks>
        internal unsafe OptRecord(
            string name,
            ushort payloadSize,
            uint rcodeAndFlags,
            ushort length,
            ReadOnlyMemory<byte> data
        ) : base(name, Type.OPT, (Class)payloadSize, rcodeAndFlags, length, data) {
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

        /// <inheritdoc />
        [System.Diagnostics.DebuggerStepThrough]
        public override string ToString() => $"";
    }
}
