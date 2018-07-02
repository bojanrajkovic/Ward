using System;
using System.Text;

namespace Ward.Dns.Records
{
    /// <summary>
    /// A TXT record.
    /// </summary>
    public class TxtRecord : Record
    {
        /// <summary>
        /// Gets the data in this record.
        /// </summary>
        /// <returns>The data in this record.</returns>
        public string TextData { get; }

        /// <summary>
        /// Creates a TXT record.
        /// </summary>
        /// <remarks>
        /// Only used from internal parsing code.
        /// </remarks>
        internal unsafe TxtRecord(
            string name,
            Class @class,
            uint timeToLive,
            ushort length,
            ReadOnlyMemory<byte> data
        ) : base(name, Type.TXT, @class, timeToLive, length, data) {
            using (var pin = data.Pin())
                TextData = Encoding.ASCII.GetString((byte*)pin.Pointer + 1, *((byte*)pin.Pointer));
        }

        /// <summary>
        /// Creates a TXT record.
        /// </summary>
        /// <param name="name">The owner-name (or label) to which this record belongs.</param>
        /// <param name="class">The record class.</param>
        /// <param name="timeToLive">The record time to live.</param>
        /// <param name="textData">The record data.</param>
        public TxtRecord(
            string name,
            Class @class,
            uint timeToLive,
            string textData
        ) : base(
            name,
            Type.TXT,
            @class,
            timeToLive,
            (ushort)(1+Encoding.ASCII.GetByteCount(textData)),
            Utils.Concat(
                new [] { (byte)Encoding.ASCII.GetByteCount(textData) },
                Encoding.ASCII.GetBytes(textData)
            )
        ) {
            TextData = textData;
        }

        /// <summary>
        /// Converts the current record to a string.
        /// </summary>
        /// <returns>A string version of the current record.</returns>
        [System.Diagnostics.DebuggerStepThrough]
        public override string ToString() =>
            $"{Name}\t{TimeToLive}\t{Class}\t{Type}\t{TextData}";
    }
}
