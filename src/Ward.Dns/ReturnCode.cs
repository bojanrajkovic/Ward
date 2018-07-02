using System.ComponentModel;

namespace Ward.Dns
{
    /// <summary>
    /// A DNS return code.
    /// </summary>
    public enum ReturnCode : ushort
    {
        /// <summary>
        /// The request completed successfully.
        /// </summary>
        [Description("The request completed successfully.")]
        NoError = 0,
        /// <summary>
        /// The name server was unable to interpret the query.
        /// </summary>
        [Description("The name server was unable to interpret the query.")]
        FormatError = 1,
        /// <summary>
        /// The name server was unable to process this query due
        /// to a problem with the name server.
        /// </summary>
        [Description("The name server was unable to process this query due to a problem with the name server.")]
        ServerFailure = 2,
        /// <summary>
        /// The domain name referenced in the query does not exist.
        /// </summary>
        [Description("The domain name referenced in the query does not exist.")]
        NameError = 3,
        /// <summary>
        /// The name server does not support the requested kind of query.
        /// </summary>
        [Description("The name server does not support the requested kind of query.")]
        NotImplemented = 4,
        /// <summary>
        /// The name server refuses to perform the specified operation for policy reasons.
        /// </summary>
        [Description("The name server refuses to perform the specified operation for policy reasons.")]
        Refused = 5,
        /// <summary>
        /// Name exists when it should not.
        /// </summary>
        [Description("Name exists when it should not.")]
        YXDomain = 6,
        /// <summary>
        /// RR set exists when it should not.
        /// </summary>
        [Description("RR set exists when it should not.")]
        YXRRSet = 7,
        /// <summary>
        /// RR set that should exist does not.
        /// </summary>
        [Description("RR set that should exist does not.")]
        NXRRSet = 8,
        /// <summary>
        /// Server not authoritative for zone.
        /// </summary>
        [Description("Server not authoritative for zone.")]
        NotAuth = 9,
        /// <summary>
        /// Name not contained in zone.
        /// </summary>
        [Description("Name not contained in zone.")]
        NotZone = 10,
        /// <summary>
        /// Bad OPT version.
        /// </summary>
        [Description("Bad OPT version.")]
        BadVers = 16,
        /// <summary>
        /// TSIG signature failure.
        /// </summary>
        [Description("TSIG signature failure.")]
        BadSig = BadVers,
        /// <summary>
        /// Key not recognized.
        /// </summary>
        [Description("Key not recognized.")]
        BadKey = 17,
        /// <summary>
        /// Signature out of time window.
        /// </summary>
        [Description("Signature out of time window.")]
        BadTime = 18,
        /// <summary>
        /// Bad TKEY mode.
        /// </summary>
        [Description("Bad TKEY mode.")]
        BadMode = 19,
        /// <summary>
        /// Duplicate key name.
        /// </summary>
        [Description("Duplicate key name.")]
        BadName = 20,
        /// <summary>
        /// Algorithm not supported.
        /// </summary>
        [Description("Algorithm not supported.")]
        BadAlg = 21,
        /// <summary>
        /// Bad truncation.
        /// </summary>
        [Description("Bad truncation.")]
        BadTrunc = 22

    }
}
