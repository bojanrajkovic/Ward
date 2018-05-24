using System;

using static Ward.Dns.Utils;

namespace Ward.Dns.Records
{
    public class MailExchangerRecord : Record
    {
        /// <summary>
        /// The preference given to this RR among others at the same owner.
        /// </summary>
        /// <returns>The preference value.</returns>
        public ushort Preference { get; }

        /// <summary>
        /// A domain name which specifies a host willing to act as a
        /// mail exchange for the owner name.
        /// </summary>
        /// <returns>A domain name.</returns>
        public string Hostname { get; }

        /// <summary>
        /// Creates an MX record.
        /// </summary>
        /// <remarks>
        /// Only used from internal parsing code.
        /// </remarks>
        internal unsafe MailExchangerRecord(
            string name,
            Dns.Type type,
            Class @class,
            uint timeToLive,
            ushort length,
            ReadOnlyMemory<byte> data,
            byte[] message
        ) : base(name, type, @class, timeToLive, length, data) {
            var dataArray = data.ToArray();
            Preference = SwapUInt16(BitConverter.ToUInt16(dataArray, 0));
            var _ = 2;
            Hostname = ParseComplexName(message, dataArray, ref _);
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

        /// <summary>
        /// Converts the current record to a string.
        /// </summary>
        /// <returns>A string version of the current record.</returns>
        [System.Diagnostics.DebuggerStepThrough]
        public override string ToString() =>
            $"{Name}\t{TimeToLive}\t{Class}\t{Type}\t{Preference}\t{Hostname}";
    }
}
