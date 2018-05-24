using System;

using static Ward.Dns.Utils;

namespace Ward.Dns.Records
{
    /// <summary>
    /// A CNAME record.
    /// </summary>
    public class CnameRecord : Record
    {
        /// <summary>The canonical or primary name for the owner.</summary>
        /// <returns>The canonical or primary name for the owner.</returns>
        public string Hostname { get; }

        /// <summary>
        /// Creates a CNAME record.
        /// </summary>
        /// <remarks>
        /// Only used from internal parsing code.
        /// </remarks>
        internal CnameRecord(
            string name,
            Class @class,
            uint timeToLive,
            ushort length,
            ReadOnlyMemory<byte> data,
            byte[] message
        ) : base(name, Dns.Type.CNAME, @class, timeToLive, length, data) {
            var _ = 0;
            Hostname = ParseComplexName(message, data.ToArray(), ref _);
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
        ) : base(name, Dns.Type.CNAME, @class, timeToLive, 0, Array.Empty<byte>()) {
            Hostname = cname;
        }

        /// <summary>
        /// Converts the current record to a string.
        /// </summary>
        /// <returns>A string version of the current record.</returns>
        [System.Diagnostics.DebuggerStepThrough]
        public override string ToString() =>
            $"{Name}\t{TimeToLive}\t{Class}\t{Type}\t{Hostname}";
    }
}
