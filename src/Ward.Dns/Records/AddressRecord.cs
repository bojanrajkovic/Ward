using System;
using System.Buffers.Binary;
using System.Net;

namespace Ward.Dns.Records
{
    /// <summary>
    /// An A or AAAA DNS record.
    /// </summary>
    /// <seealso cref="Record" />
    public class AddressRecord : Record
    {
        /// <summary>
        /// Gets the IP address contained in this record.
        /// </summary>
        /// <value>
        /// The IP address contained in this record.
        /// </value>
        public IPAddress Address { get; }

        /// <summary>
        /// Creates an A/AAAA record.
        /// </summary>
        /// <param name="name">The owner-name or label to which this record belongs.</param>
        /// <param name="type">The resource record type.</param>
        /// <param name="class">The resource record class.</param>
        /// <param name="timeToLive">The resource record time to live.</param>
        /// <param name="length">The length of the resource record data.</param>
        /// <param name="data">The resource record-specific data.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if <paramref name="type"/> is not <code>Type.A</code> or
        /// <code>Type.AAAA</code>.
        /// </exception>
        /// <remarks>
        /// Only used from internal parsing code.
        /// </remarks>
        internal AddressRecord(
            string name,
            Type type,
            Class @class,
            uint timeToLive,
            ushort length,
            ReadOnlyMemory<byte> data
        ) : base(name, type, @class, timeToLive, length, data)
        {
            if (type != Type.A && type != Type.AAAA)
                throw new ArgumentOutOfRangeException(nameof(type));

            // IPv4 addresses we can fast-path via reading a big-endian value
            // out of the data, because they fit within one. AAAA we have to
            // convert to bytes here and take the allocation hit.
            if (type == Type.A)
                Address = new IPAddress(BinaryPrimitives.ReadUInt32LittleEndian(data.Span));
            else
                Address = new IPAddress(data.ToArray());
        }

        /// <summary>
        /// Creates an A/AAAA record.
        /// </summary>
        /// <param name="name">The owner-name (or label) to which this record belongs.</param>
        /// <param name="type">The type of this record, constrained to <see cref="Type.A" /> or <see cref="Type.AAAA" />.</param>
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
        )
        {
        }


        /// <inheritdoc />
        [System.Diagnostics.DebuggerStepThrough]
        public override string ToString() => $"{Name}\t{TimeToLive}\t{Class}\t{Type}\t{Address}";
    }
}
