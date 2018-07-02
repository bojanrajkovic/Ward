using System;

namespace Ward.Dns
{
    /// <summary>
    /// A DNS resource record.
    /// </summary>
    public class Record
    {
        /// <summary>
        /// Gets the owner-name or label to which this record belongs.
        /// </summary>
        /// <value>
        /// The owner-name or label to which this record belongs.
        /// </value>
        public string Name { get; }

        /// <summary>
        /// Gets the resource record type.
        /// </summary>
        /// <value>
        /// The resource record type.
        /// </value>
        public Type Type { get; }

        /// <summary>
        /// Gets the resource record class.
        /// </summary>
        /// <value>
        /// The resource record class.
        /// </value>
        public Class Class { get; }

        /// <summary>
        /// Gets the resource record time to live.
        /// </summary>
        /// <value>
        /// The resource record time to live.
        /// </value>
        public uint TimeToLive { get; }

        /// <summary>
        /// Gets the lenth of the resource record data.
        /// </summary>
        /// <value>
        /// The length of the resource record data.
        /// </value>
        public ushort Length { get; }

        /// <summary>
        /// Gets the resource record-specific data.
        /// </summary>
        /// <value>
        /// The resource record-specific data.
        /// </value>
        public ReadOnlyMemory<byte> Data { get; }

        /// <summary>
        /// Creates a new <see cref="Record"/>.
        /// </summary>
        /// <param name="name">The owner-name or label to which this record belongs.</param>
        /// <param name="type">The resource record type.</param>
        /// <param name="class">The resource record class.</param>
        /// <param name="timeToLive">The resource record time to live.</param>
        /// <param name="length">The length of the resource record data.</param>
        /// <param name="data">The resource record-specific data.</param>
        public Record(
            string name,
            Type type,
            Class @class,
            uint timeToLive,
            ushort length,
            ReadOnlyMemory<byte> data
        )
        {
            Name = name;
            Type = type;
            Class = @class;
            TimeToLive = timeToLive;
            Length = length;
            Data = data;
        }

        /// <summary>
        /// Converts the <see cref="Record"/> to a <see cref="System.String"/>.
        /// </summary>
        /// <returns>A generic resource record string.</returns>
        [System.Diagnostics.DebuggerStepThrough]
        public override string ToString() =>
            $"{Name}\t{TimeToLive}\t{Class}\t{Type}\t{Length} bytes of data: {Convert.ToBase64String(Data.ToArray())}";
    }
}
