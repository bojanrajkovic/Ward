using System;
using System.Text;

namespace Ward.Dns.Records
{
    /// <summary>
    /// A CAA record.
    /// </summary>
    /// <seealso cref="Record" />
    public class CaaRecord : Record
    {
        /// <summary>
        /// Gets a flag indicating whether the corresponding property tag MUST
        /// be understood if the semantics of the CAA record areto be correctly
        /// interpreted by the issuer.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the property MUST be understood; otherwise, <c>false</c>.
        /// </value>
        public bool Critical { get; }

        /// <summary>
        /// Gets the property tag.
        /// </summary>
        /// <value>
        /// The property tag.
        /// </value>
        public string Tag { get; }

        /// <summary>
        /// Gets the property value.
        /// </summary>
        /// <value>
        /// The property value.
        /// </value>
        public string Value { get; }

        /// <summary>
        /// Creates a CAA record.
        /// </summary>
        /// <param name="name">The owner-name or label to which this record belongs.</param>
        /// <param name="class">The resource record class.</param>
        /// <param name="timeToLive">The resource record time to live.</param>
        /// <param name="length">The length of the resource record data.</param>
        /// <param name="data">The resource record-specific data.</param>
        /// <remarks>
        /// Only used from internal parsing code.
        /// </remarks>
        internal unsafe CaaRecord(
            string name,
            Class @class,
            uint timeToLive,
            ushort length,
            ReadOnlyMemory<byte> data
        ) : base(name, Type.CAA, @class, timeToLive, length, data) {
            // The top bit is set if the critical flag is true, all other
            // bit positions are reserved per RFC 6844.
            Critical = (data.Span[0] & 0b1000_0000) == 0b1000_0000;
            var tagLength = data.Span[1];

            var dataPointer = data.Pin().Pointer;
            Tag = Encoding.ASCII.GetString((byte*)dataPointer + 2, tagLength);
            Value = Encoding.ASCII.GetString((byte*)dataPointer + 2 + tagLength, (length - 2 - tagLength));
        }

        /// <summary>
        /// Creates a CAA record.
        /// </summary>
        /// <param name="name">The owner-name or label to which this record belongs.</param>
        /// <param name="class">The record class.</param>
        /// <param name="timeToLive">The record time to live.</param>
        /// <param name="critical">Is this tag critical. See <see cref="Critical" />.</param>
        /// <param name="tag">The property tag.</param>
        /// <param name="value">The property value.</param>
        public CaaRecord(
            string name,
            Class @class,
            uint timeToLive,
            bool critical,
            string tag,
            string value
        ) : base(name, Type.CAA, @class, timeToLive, 0, Array.Empty<byte>()) {
            Critical = critical;
            Tag = tag;
            Value = value;
        }

        /// <inheritdoc />
        [System.Diagnostics.DebuggerStepThrough]
        public override string ToString() =>
            $"{Name}\t{TimeToLive}\t{Class}\t{Type}\t{(Critical ? 1 : 0)} {Tag} {Value}";
    }
}
