using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

using Ward.Dns.Records;
using static Ward.Dns.Utils;

namespace Ward.Dns
{
    /// <summary>
    /// A DNS resource record.
    /// </summary>
    public class Record
    {
        /// <summary>
        /// The owner-name or label to which this record belongs.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The resource record type.
        /// </summary>
        public Type Type { get; }

        /// <summary>
        /// The resource record class.
        /// </summary>
        public Class Class { get; }

        /// <summary>
        /// The resource record time to live, in seconds.
        /// </summary>
        public uint TimeToLive { get; }

        /// <summary>
        /// The lenth of the resource record data.
        /// </summary>
        public ushort Length { get; }

        /// <summary>
        /// The resource record-specific data.
        /// </summary>
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
        ) {
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
