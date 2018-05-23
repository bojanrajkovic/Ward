using System;
using System.Net;

namespace Ward.Dns.Records
{
    /// <summary>
    /// An A or AAAA DNS record.
    /// </summary>
    public class AddressRecord : Record
    {
        /// <summary>
        /// The IP address contained in this record.
        /// </summary>
        /// <returns>An IP address.</returns>
        public IPAddress Address { get; }

        /// <summary>
        /// Creates a <see cref="AddressRecord"/>.
        /// </summary>
        /// <remarks>
        /// Only used from internal parsing code&mdash;<see cref="RecordFactory"/> and callers
        /// of <see cref="RecordFactory.Create"/>. For constructing an address record outside of
        /// the message parsing loop, <see cref="AddressRecord.AddressRecord(string, Type, Class, uint, IPAddress)"/>.
        /// </remarks>
        /// <param name="name">The owner-name (or label) to which this record belongs.</param>
        /// <param name="type">
        /// The type of this record, constrained to <see cref="Type.A"/> or <see cref="Type.AAAA"/>.
        /// </param>
        /// <param name="class">The record class.</param>
        /// <param name="timeToLive">The record time to live.</param>
        /// <param name="length">Length of the record data.</param>
        /// <param name="data">
        /// A pointer to a <see cref="ReadOnlyMemory{T}"/> containing the record data.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if a <see cref="Type"/> other than <c>Type.A</c> or <c>Type.AAAA</c>
        /// is passed to the constructor.
        /// </exception>
        internal AddressRecord(
            string name,
            Type type,
            Class @class,
            uint timeToLive,
            ushort length,
            ReadOnlyMemory<byte> data
        ) : base(name, type, @class, timeToLive, length, data) {
            if (type != Type.A && type != Type.AAAA)
                throw new ArgumentOutOfRangeException(nameof(type));

            Address = new IPAddress(data.ToArray());
        }

        /// <summary>
        /// Creates a <see cref="AddressRecord"/>.
        /// </summary>
        /// <param name="name">The owner name (or label) to which this record belongs.</param>
        /// <param name="type">
        /// The type of this record, constrained to <see cref="Type.A"/> or <see cref="Type.AAAA"/>.
        /// </param>
        /// <param name="class">The record class.</param>
        /// <param name="timeToLive">The record time to live.</param>
        /// <param name="address">The address stored in this record.</param>
        public AddressRecord(
            string name,
            Type type,
            Class @class,
            uint timeToLive,
            IPAddress address
        ) : this(
            name,
            type,
            @class,
            timeToLive,
            (ushort)address.GetAddressBytes().Length,
            address.GetAddressBytes()
        ) {
        }

        [System.Diagnostics.DebuggerStepThrough]
        public override string ToString() => $"{Name}\t{TimeToLive}\t{Class}\t{Type}\t{Address}";
    }
}
