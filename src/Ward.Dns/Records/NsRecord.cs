using System;
using System.Collections.Generic;
using static Ward.Dns.Utils;

namespace Ward.Dns.Records
{
    /// <summary>
    /// An NS record.
    /// </summary>
    /// <seealso cref="Record" />
    public class NsRecord : Record
    {
        /// <summary>
        /// Gets a domain-name which specifies a host which should be authoritative
        /// for the specified class and domain.
        /// </summary>
        /// <value>
        /// A domain-name which specifies a host which should be authoritative for
        /// the specified class and domain.
        /// </value>
        public string Hostname { get; }

        /// <summary>
        /// Creates an NS record.
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
        internal NsRecord(
            string name,
            Class @class,
            uint timeToLive,
            ushort length,
            ReadOnlyMemory<byte> data,
            ReadOnlySpan<byte> message,
            Dictionary<int, string> reverseOffsetMap
        ) : base(name, Type.NS, @class, timeToLive, length, data) {
            var _ = 0;
            Hostname = ParseComplexName(message, data.Span, ref _, reverseOffsetMap);
        }

        /// <summary>
        /// Creates an NS record.
        /// </summary>
        /// <param name="name">The owner-name (or label) to which this record belongs.</param>
        /// <param name="class">The record class.</param>
        /// <param name="timeToLive">The record time to live.</param>
        /// <param name="hostname">A domain-name which specifies a host which should be authoritative
        /// for the specified class and domain.</param>
        public NsRecord(
            string name,
            Class @class,
            uint timeToLive,
            string hostname
        ) : base(name, Type.NS, @class, timeToLive, 0, Array.Empty<byte>()) {
            Hostname = hostname;
        }

        /// <inheritdoc />
        [System.Diagnostics.DebuggerStepThrough]
        public override string ToString() => $"{Name}\t{TimeToLive}\t{Class}\t{Type}\t{Hostname}";
    }
}
