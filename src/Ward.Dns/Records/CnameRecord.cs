using System;
using System.Collections.Generic;
using static Ward.Dns.Utils;

namespace Ward.Dns.Records
{
    /// <summary>
    /// A CNAME record.
    /// </summary>
    /// <seealso cref="Record" />
    public class CnameRecord : Record
    {
        /// <summary>
        /// Gets the canonical or primary name for the owner.
        /// </summary>
        /// <value>
        /// The canonical or primary name for the owner.
        /// </value>
        public string Hostname { get; }

        /// <summary>
        /// Creates a CNAME record.
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
        internal CnameRecord(
            string name,
            Class @class,
            uint timeToLive,
            ushort length,
            ReadOnlyMemory<byte> data,
            ReadOnlySpan<byte> message,
            Dictionary<int, string> reverseOffsetMap
        ) : base(name, Type.CNAME, @class, timeToLive, length, data)
        {
            var _ = 0;
            Hostname = ParseComplexName(message, data.Span, ref _, reverseOffsetMap);
        }

        /// <summary>
        /// Creates a CNAME record.
        /// </summary>
        /// <param name="name">The owner-name (or label) to which this record belongs.</param>
        /// <param name="class">The record class.</param>
        /// <param name="timeToLive">The record time to live.</param>
        /// <param name="cname">The canonical or primary name for the owner.</param>
        public CnameRecord(
            string name,
            Class @class,
            uint timeToLive,
            string cname
        ) : base(name, Type.CNAME, @class, timeToLive, 0, Array.Empty<byte>())
        {
            Hostname = cname;
        }

        /// <inheritdoc />
        [System.Diagnostics.DebuggerStepThrough]
        public override string ToString() =>
            $"{Name}\t{TimeToLive}\t{Class}\t{Type}\t{Hostname}";
    }
}
