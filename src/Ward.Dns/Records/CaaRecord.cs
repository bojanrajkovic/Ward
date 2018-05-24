using System;
using System.Text;

using static Ward.Dns.Utils;

namespace Ward.Dns.Records
{
    /// <summary>
    /// A CAA record.
    /// </summary>
    public class CaaRecord : Record
    {
        /// <summary>
        /// Indicates whether the corresponding property tag MUST
        /// be understood if the semantics of the CAA record are
        /// to be correctly interpreted by the issuer.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the property must be understood, false
        /// otherwise.
        /// </returns>
        public bool Critical { get; }

        /// <summary>The property tag.</summary>
        /// <returns>The property tag.</returns>
        public string Tag { get; }

        /// <summary>The property value.</summary>
        /// <returns>The property value.</returns>
        public string Value { get; }

        /// <summary>
        /// Creates a CAA record.
        /// </summary>
        /// <remarks>
        /// Only used from internal parsing code.
        /// </remarks>
        internal unsafe CaaRecord(
            string name,
            Class @class,
            uint timeToLive,
            ushort length,
            ReadOnlyMemory<byte> data
        ) : base(name, Dns.Type.CAA, @class, timeToLive, length, data) {
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
        /// <param name="name">The owner-name (or label) to which this record belongs.</param>
        /// <param name="class">The record class.</param>
        /// <param name="timeToLive">The record time to live.</param>
        /// <param name="critical">Is this tag critical. See <see cref="Critical"/>.</param>
        /// <param name="tag">The property tag.</param>
        /// <param name="value">The property value.</param>
        public CaaRecord(
            string name,
            Class @class,
            uint timeToLive,
            bool critical,
            string tag,
            string value
        ) : base(name, Dns.Type.CAA, @class, timeToLive, 0, Array.Empty<byte>()) {
            Critical = critical;
            Tag = tag;
            Value = value;
        }

        /// <summary>
        /// Converts the current record to a string.
        /// </summary>
        /// <returns>A string version of the current record.</returns>
        [System.Diagnostics.DebuggerStepThrough]
        public override string ToString() =>
            $"{Name}\t{TimeToLive}\t{Class}\t{Type}\t{(Critical ? 1 : 0)} {Tag} {Value}";
    }
}
