using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using static Ward.Dns.Utils;

namespace Ward.Dns.Records
{
    /// <summary>
    /// An MX record.
    /// </summary>
    /// <seealso cref="Record" />
    public class MailExchangerRecord : Record
    {
        /// <summary>
        /// Gets the value for the preference given to this RR among others
        /// at the same owner.
        /// </summary>
        /// <value>
        /// The preference given to this RR among others at the same owner.
        /// </value>
        public ushort Preference { get; }

        /// <summary>
        /// Gets a domain name which specifies a host willing to act as a
        /// mail exchange for the owner name.
        /// </summary>
        /// <value>
        /// A domain name which specifies a host willing to act as a mail
        /// exchange for the owner name.
        /// </value>
        public string Hostname { get; }

        /// <summary>
        /// Creates an MX record.
        /// </summary>
        /// <param name="name">The owner-name or label to which this record belongs.</param>
        /// <param name="class">The resource record class.</param>
        /// <param name="timeToLive">The resource record time to live.</param>
        /// <param name="length">The length of the resource record data.</param>
        /// <param name="data">The resource record-specific data.</param>
        /// <param name="message">The complete original message.</param>
        /// <param name="reverseOffsetMap">The reverse offset map.</param>
        /// <remarks>
        /// Only used from internal parsing code.
        /// </remarks>
        internal unsafe MailExchangerRecord(
            string name,
            Class @class,
            uint timeToLive,
            ushort length,
            ReadOnlyMemory<byte> data,
            ReadOnlySpan<byte> message,
            Dictionary<int, string> reverseOffsetMap
        ) : base(name, Type.MX, @class, timeToLive, length, data)
        {
            Preference = BinaryPrimitives.ReadUInt16BigEndian(data.Span);
            var _ = 2;
            Hostname = ParseComplexName(message, data.Span, ref _, reverseOffsetMap);
        }

        /// <summary>
        /// Creates an MX record.
        /// </summary>
        /// <param name="name">The owner-name (or label) to which this record belongs.</param>
        /// <param name="class">The record class.</param>
        /// <param name="timeToLive">The record time to live.</param>
        /// <param name="preference">The preference to give this record amongst others at the same owner-name.</param>
        /// <param name="hostname">A domain name willing to act as a mail exchanger for this owner-name.</param>
        public MailExchangerRecord(
            string name,
            Class @class,
            uint timeToLive,
            ushort preference,
            string hostname
        ) : base(name, Type.MX, @class, timeToLive, 0, Array.Empty<byte>()) {
            Preference = preference;
            Hostname = hostname;
        }

        /// <inheritdoc />
        [System.Diagnostics.DebuggerStepThrough]
        public override string ToString() =>
            $"{Name}\t{TimeToLive}\t{Class}\t{Type}\t{Preference}\t{Hostname}";
    }
}
