using System;

namespace Ward.Dns
{
    /// <summary>
    /// Specifies the kind of query in the message. Set by the originator
    /// of a query, and copied into the response.
    /// </summary>
    public enum Opcode : byte
    {
        /// <summary>
        /// A standard query.
        /// </summary>
        Query = 0,
        /// <summary>
        /// An inverse query.
        /// </summary>
        [Obsolete("Obsoleted by RFC3425.")]
        IQuery = 1,
        /// <summary>
        /// A server status request.
        /// </summary>
        Status = 2,
        /// <summary>
        /// An RR update notification.
        /// </summary>
        /// <remarks>
        /// See https://tools.ietf.org/html/rfc1996.
        /// </remarks>
        Notify = 4,
        /// <summary>
        /// An RRset update notification.
        /// </summary>
        /// <remarks>
        /// See https://tools.ietf.org/html/rfc2136.
        /// </remarks>
        Update = 5
    }
}
