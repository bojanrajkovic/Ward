using System.ComponentModel;

namespace Ward.Dns
{
    public enum ReturnCode
    {
        [Description("The request completed successfully.")]
        NoError = 0,
        [Description("The name server was unable to interpret the query.")]
        FormatError = 1,
        [Description("The name server was unable to process this query due to a problem with the name server.")]
        ServerFailure = 2,
        [Description("The domain name referenced in the query does not exist.")]
        NameError = 3,
        [Description("The name server does not support the requested kind of query.")]
        NotImplemented = 4,
        [Description("The name server refuses to perform the specified operation for policy reasons.")]
        Refused = 5,
        [Description("Name exists when it should not.")]
        YXDomain = 6,
        [Description("RR Set Exists when it should not.")]
        YXRRSet = 7,
        [Description("RR Set that should exist does not.")]
        NXRRSet = 8,
        [Description("Server Not Authoritative for zone.")]
        NotAuth = 9,
        [Description("Name not contained in zone.")]
        NotZone = 10,
        [Description("Bad OPT version.")]
        BadVers = 16,
        [Description("TSIG signature failure.")]
        BadSig = BadVers,
        [Description("Key not recognized.")]
        BadKey = 17,
        [Description("Signature out of time window.")]
        BadTime = 18,
        [Description("Bad TKEY mode.")]
        BadMode = 19,
        [Description("Duplicate key name.")]
        BadName = 20,
        [Description("Algorithm not supported.")]
        BadAlg = 21,
        [Description("Bad truncation.")]
        BadTrunc = 22

    }
}
