using System;

using static Ward.Dns.Utils;

namespace Ward.Dns.Records
{
    /// <summary>
    /// A PTR record.
    /// </summary>
    /// <seealso cref="Record" />
    public class PtrRecord : Record
    {
        /// <summary>
        /// Gets a domain-name which points to some location in the domain name space.
        /// </summary>
        /// <value>
        /// A domain-name which points to some location in the domain name space.
        /// </value>
        public string Hostname { get; }

        /// <summary>
        /// Creates a PTR record.
        /// </summary>
        /// <param name="name">The owner-name or label to which this record belongs.</param>
        /// <param name="class">The resource record class.</param>
        /// <param name="timeToLive">The resource record time to live.</param>
        /// <param name="length">The length of the resource record data.</param>
        /// <param name="data">The resource record-specific data.</param>
        /// <param name="message">The complete original message.</param>
        /// <remarks>
        /// Only used from internal parsing code.
        /// </remarks>
        internal PtrRecord(
            string name,
            Class @class,
            uint timeToLive,
            ushort length,
            ReadOnlyMemory<byte> data,
            byte[] message
        ) : base(name, Type.PTR, @class, timeToLive, length, data) {
            var _ = 0;
            Hostname = ParseComplexName(message, data.ToArray(), ref _);
        }

        /// <summary>
        /// Creates a PTR record.
        /// </summary>
        /// <param name="name">The owner-name (or label) to which this record belongs.</param>
        /// <param name="class">The record class.</param>
        /// <param name="timeToLive">The record time to live.</param>
        /// <param name="hostname">A domain-name in the domain name space to point at.</param>
        public PtrRecord(
            string name,
            Class @class,
            uint timeToLive,
            string hostname
        ) : base(name, Type.PTR, @class, timeToLive, 0, Array.Empty<byte>()) {
            Hostname = hostname;
        }

        /// <inheritdoc />
        [System.Diagnostics.DebuggerStepThrough]
        public override string ToString() => $"{Name}\t{TimeToLive}\t{Class}\t{Type}\t{Hostname}";
    }
}
