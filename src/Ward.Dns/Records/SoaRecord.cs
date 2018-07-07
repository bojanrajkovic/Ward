using System;
using System.Collections.Generic;

using static System.Buffers.Binary.BinaryPrimitives;

namespace Ward.Dns.Records
{
    /// <summary>
    /// A SOA record.
    /// </summary>
    /// <seealso cref="Record" />
    public class SoaRecord : Record
    {
        /// <summary>
        /// Gets the domain-name of the name server that was the original or
        /// primary source of data for this zone.
        /// </summary>
        /// <returns>
        /// The domain-name of the name server that was the original or
        /// primary source of data for this zone.
        /// </returns>
        public string PrimaryNameServer { get; }

        /// <summary>
        /// Gets a domain-name which specifies the mailbox of the person responsible
        /// for this zone.
        /// </summary>
        /// <returns>
        /// A domain-name which specifies the mailbox of the person responsible for this zone.
        /// </returns>
        public string ResponsibleName { get; }

        /// <summary>
        /// Gets the unsigned 32-bit version number of the original copy of
        /// the zone. Zone transfers preserve this value. This value wraps
        /// around and should be compared using sequence space arithmetic.
        /// </summary>
        /// <returns>
        /// The unsigned 32-bit version number of the original copy of the zone.
        /// Zone transfers preserve this value. This value wraps around and should be
        /// compared using sequence space arithmetic.
        /// </returns>
        public uint Serial { get; }

        /// <summary>
        /// Gets the 32-bit time interval before the zone should be refreshed.
        /// </summary>
        /// <returns>The 32-bit time interval before the zone should be refreshed.</returns>
        public int Refresh { get; }

        /// <summary>
        /// Gets the 32-bit time interval that should elapse before a failed
        /// refresh should be retried.
        /// </summary>
        /// <returns>
        /// The 32-bit time interval that should elapse before a failed refresh
        /// should be retried.
        /// </returns>
        public int Retry { get; }

        /// <summary>
        /// Gets a 32-bit time value that specifies the upper limit on
        /// the time interval that can elapse before the zone is no
        /// longer authoritative.
        /// </summary>
        /// <returns>
        /// A 32-bit time value that specifies the upper limit on the time
        /// interval that can elapse before the zone is no longer authoritative.
        /// </returns>
        public int Expire { get; }

        /// <summary>
        /// Gets the unsigned 32-bit minimum TTL field that should be
        /// exported with any RR from this zone.
        /// </summary>
        /// <returns>
        /// The unsigned 32-bit minimum TTL field that should be exported
        /// with any RR from this zone.
        /// </returns>
        public uint MinimumTtl { get; }

        /// <summary>
        /// Creates a SOA record.
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
        internal SoaRecord(
            string name,
            Class @class,
            uint timeToLive,
            ushort length,
            ReadOnlyMemory<byte> data,
            ReadOnlySpan<byte> message,
            Dictionary<int, string> reverseOffsetMap
        ) : base(name, Type.SOA, @class, timeToLive, length, data) {
            var offset = 0;
            PrimaryNameServer = Utils.ParseComplexName(message, data.Span, ref offset, reverseOffsetMap);
            ResponsibleName = Utils.ParseComplexName(message, data.Span, ref offset, reverseOffsetMap);

            Serial = ReadUInt32BigEndian(data.Slice(offset, 4).Span);
            Refresh = ReadInt32BigEndian(data.Slice(offset + 4, 4).Span);
            Retry = ReadInt32BigEndian(data.Slice(offset + 8, 4).Span);
            Expire = ReadInt32BigEndian(data.Slice(offset + 12, 4).Span);
            MinimumTtl = ReadUInt32BigEndian(data.Slice(offset + 16, 4).Span);
        }

        /// <summary>
        /// Creates a SOA record.
        /// </summary>
        /// <param name="name">The owner-name (or label) to which this record belongs.</param>
        /// <param name="class">The record class.</param>
        /// <param name="timeToLive">The record time to live.</param>
        /// <param name="primaryNameServer">
        /// The domain-name of the server that was the original source of data for this zone.
        /// </param>
        /// <param name="responsibleName">
        /// A domain-name which specifies the mailbox of the person responsible for this zone.
        /// </param>
        /// <param name="serial">The version number of this zone.</param>
        /// <param name="refresh">How long until the zone should be refreshed.</param>
        /// <param name="retry">How long before failed refreshes should be retried.</param>
        /// <param name="expire">How long before this zone is no longer authoritative.</param>
        /// <param name="minimumTtl">The minimum TTL field for any RRs exported from this zone.</param>
        public SoaRecord(
            string name,
            Class @class,
            uint timeToLive,
            string primaryNameServer,
            string responsibleName,
            uint serial,
            int refresh,
            int retry,
            int expire,
            uint minimumTtl
        ) : base(name, Type.SOA, @class, timeToLive, 0, Array.Empty<byte>()) {
            PrimaryNameServer = primaryNameServer;
            ResponsibleName = responsibleName;
            Serial = serial;
            Refresh = refresh;
            Retry = retry;
            Expire = expire;
            MinimumTtl = minimumTtl;
        }

        /// <inheritdoc />
        [System.Diagnostics.DebuggerStepThrough]
        public override string ToString() =>
            $"{Name}\t{TimeToLive}\t{Class}\t{Type}\t{PrimaryNameServer} {ResponsibleName} " +
            $"{Serial} {Refresh} {Retry} {Expire} {MinimumTtl}";
    }
}
