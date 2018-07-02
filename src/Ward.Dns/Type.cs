using System;

namespace Ward.Dns
{
    /// <summary>
    /// The DNS resource record type.
    /// </summary>
    public enum Type : ushort
    {
        /// <summary>
        /// A host address.
        /// </summary>
        A = 1,
        /// <summary>
        /// An authoritative name server.
        /// </summary>
        NS = 2,
        /// <summary>
        /// A mail destination.
        /// </summary>
        [Obsolete("Use MX.")]
        MD = 3,
        /// <summary>
        /// A mail forwarder .
        /// </summary>
        [Obsolete("Use MX.")]
        MF = 4,
        /// <summary>
        /// A canonical name for an alias.
        /// </summary>
        CNAME = 5,
        /// <summary>
        /// Marks the start of a zone of authority.
        /// </summary>
        SOA = 6,
        /// <summary>
        /// A mailbox domain name.
        /// </summary>
        /// <remarks>
        /// Experimental.
        /// </remarks>
        MB = 7,
        /// <summary>
        /// A mail groupp member.
        /// </summary>
        /// <remarks>
        /// Experimental.
        /// </remarks>
        MG = 8,
        /// <summary>
        /// A mail rename domain name.
        /// </summary>
        /// <remarks>
        /// Experimental.</remarks>
        MR = 9,
        /// <summary>
        /// A null RR.
        /// </summary>
        /// <remarks>
        /// Experimental.
        /// </remarks>
        NULL = 10,
        /// <summary>
        /// A well known service description.
        /// </summary>
        WKS = 11,
        /// <summary>
        /// A domain name pointer.
        /// </summary>
        PTR = 12,
        /// <summary>
        /// Host information.
        /// </summary>
        HINFO = 13,
        /// <summary>
        /// Mailbox or mail list information.
        /// </summary>
        MINFO = 14,
        /// <summary>
        /// Mail exchange.
        /// </summary>
        MX = 15,
        /// <summary>
        /// Text strings.
        /// </summary>
        TXT = 16,
        /// <summary>
        /// Responsible person.
        /// </summary>
        RP = 17,
        /// <summary>
        /// AFS Data Base location.
        /// </summary>
        AFSDB = 18,
        /// <summary>
        /// X.25 PSDN address.
        /// </summary>
        X25 = 19,
        /// <summary>
        /// ISDN address.
        /// </summary>
        ISDN = 20,
        /// <summary>
        /// Route Through
        /// </summary>
        RT = 21,
        /// <summary>
        /// NSAP-style A record.
        /// </summary>
        NSAP = 22,
        /// <summary>
        /// NSAP-style PTR record.
        /// </summary>
        NSAP_PTR = 23,
        /// <summary>
        /// Security signature (DNSSEC).
        /// </summary>
        SIG = 24,
        /// <summary>
        /// Security key (DNSSEC).
        /// </summary>
        KEY = 25,
        /// <summary>
        /// X.400 mail mapping information.
        /// </summary>
        PX = 26,
        /// <summary>
        /// Geographical position.
        /// </summary>
        GPOS = 27,
        /// <summary>
        /// IPv6 address.
        /// </summary>
        AAAA = 28,
        /// <summary>
        /// Location information.
        /// </summary>
        LOC = 29,
        /// <summary>
        /// Next domain.
        /// </summary>
        [Obsolete("Obsoleted by RFC 3755.")]
        NXT = 30,
        /// <summary>
        /// Endpoint identifier.
        /// </summary>
        EID = 31,
        /// <summary>
        /// Nimrod locator.
        /// </summary>
        NIMLOC = 32,
        /// <summary>
        /// Server selection.
        /// </summary>
        SRV = 33,
        /// <summary>
        /// ATM address.
        /// </summary>
        ATMA = 34,
        /// <summary>
        /// Naming authority pointer.
        /// </summary>
        NAPTR = 35,
        /// <summary>
        /// Key exchanger.
        /// </summary>
        KX = 36,
        /// <summary>
        /// CERT.
        /// </summary>
        /// <remarks>See RFC 4398.</remarks>
        CERT = 37,
        /// <summary>
        /// IPv6 address.
        /// </summary>
        [Obsolete("Use AAAA.")]
        A6 = 38,
        /// <summary>
        /// DNAME.
        /// </summary>
        /// <remarks>See RFC 6672.</remarks>
        DNAME = 39,
        /// <summary>
        /// SINK.
        /// </summary>
        /// <remarks>See http://tools.ietf.org/html/draft-eastlake-kitchen-sink.</remarks>
        SINK = 40,
        /// <summary>
        /// OPT pseudo-RR for EDNS0.
        /// </summary>
        OPT = 41,
        /// <summary>
        /// APL.
        /// </summary>
        /// <remarks>See RFC 3123.</remarks>
        APL = 42,
        /// <summary>
        /// Delegation signer.
        /// </summary>
        DS = 43,
        /// <summary>
        /// SSH key fingerprint.
        /// </summary>
        SSHFP = 44,
        /// <summary>
        /// IPSec key.
        /// </summary>
        IPSECKEY = 45,
        /// <summary>
        /// Resource record signature (DNSSEC).
        /// </summary>
        /// <remarks>See RFCs 3755, 4034.</remarks>
        RRSIG = 46,
        /// <summary>
        /// Next secure (DNSSEC).
        /// </summary>
        /// <remarks>See RFCs 3755, 4034.</remarks>
        NSEC = 47,
        /// <summary>
        /// DNS public key (DNSSEC).
        /// </summary>
        /// <remarks>See RFCs 3755, 4034.</remarks>
        DNSKEY = 48,
        /// <summary>
        /// DHCP ID.
        /// </summary>
        /// <remarks>See RFC 4701.</remarks>
        DHCID = 49,
        /// <summary>
        /// Next secure v3 (DNSSEC)
        /// </summary>
        /// <remarks>See RFC 5155.</remarks>
        NSEC3 = 50,
        /// <summary>
        /// NSEC3 parmeter (DNSSEC).
        /// </summary>
        /// <remarks>See RFC 5155.</remarks>
        NSEC3PARAM = 51,
        /// <summary>
        /// TLS association.
        /// </summary>
        /// <remarks>See RFC 6698.</remarks>
        TLSA = 52,
        /// <summary>
        /// S/MIME association.
        /// </summary>
        /// <remarks>See RFC 8162.</remarks>
        SMIMEA = 53,
        /// <summary>
        /// Unassigned.
        /// </summary>
        Unassigned = 54,
        /// <summary>
        /// Host Identity Protocol.
        /// </summary>
        /// <remarks>See RFC 8005.</remarks>
        HIP = 55,
        /// <summary>
        /// NINFO.
        /// </summary>
        /// <remarks>See https://www.iana.org/assignments/dns-parameters/NINFO/ninfo-completed-template.</remarks>
        NINFO = 56,
        /// <summary>
        /// RKEY.
        /// </summary>
        /// <remarks>See https://www.iana.org/assignments/dns-parameters/RKEY/rkey-completed-template.</remarks>
        RKEY = 57,
        /// <summary>
        /// Trust anchor link.
        /// </summary>
        /// <remarks>See https://www.iana.org/assignments/dns-parameters/TALINK/talink-completed-template.</remarks>
        TALINK = 58,
        /// <summary>
        /// Child DS.
        /// </summary>
        /// <remarks>See RFC 7344.</remarks>
        CDS = 59,
        /// <summary>
        /// <see cref="DNSKEY"/> the child wants reflected in DS.
        /// </summary>
        /// <remarks>See RFC 7344.</remarks>
        CDNSKEY = 60,
        /// <summary>
        /// OpenPGP key.
        /// </summary>
        /// <remarks>See RFC 7929.</remarks>
        OPENPGPKEY = 61,
        /// <summary>
        /// Child-to-parent synchronization.
        /// </summary>
        /// <remarks>See RFC 7477.</remarks>
        CSYNC = 62,
        /// <summary>
        /// Sender Policy Framework.
        /// </summary>
        /// <remarks>See RFC 7208.</remarks>
        [Obsolete("Typically, TXT records are used for SPF.")]
        SPF = 99,
        /// <summary>
        /// IANA reserved RRTYPE.
        /// </summary>
        UINFO = 100,
        /// <summary>
        /// IANA reserved RRTYPE.
        /// </summary>
        UID = 101,
        /// <summary>
        /// IANA reserved RRTYPE.
        /// </summary>
        GID = 102,
        /// <summary>
        /// IANA reserved RRTYPE.
        /// </summary>
        UNSPEC = 103,
        /// <summary>
        /// ILNP node identifier.
        /// </summary>
        /// <remarks>See RFC 6742.</remarks>
        NID = 104,
        /// <summary>
        /// ILNPv4 32-bit locator.
        /// </summary>
        /// <remarks>See RFC 6742.</remarks>
        L32 = 105,
        /// <summary>
        /// ILNPv6 64-bit locator.
        /// </summary>
        /// <remarks>See RFC 6742.</remarks>
        L64 = 106,
        /// <summary>
        /// ILNP subnetwork.
        /// </summary>
        /// <remarks>See RFC 6742.</remarks>
        LP = 107,
        /// <summary>
        /// An EUI-48 address.
        /// </summary>
        /// <remarks>See RFC 7043.</remarks>
        EUI48 = 108,
        /// <summary>
        /// An EUI-64 address.
        /// </summary>
        /// <remarks>See RFC 7043.</remarks>
        EUI64 = 109,
        /// <summary>
        /// Transaction key.
        /// </summary>
        /// <remarks>See RFC 2930.</remarks>
        TKEY = 249,
        /// <summary>
        /// Transaction signature.
        /// </summary>
        /// <remarks>See RFC 2845.</remarks>
        TSIG = 250,
        /// <summary>
        /// Incremental transfer.
        /// </summary>
        /// <remarks>See RFC 1955.</remarks>
        IXFR = 251,
        /// <summary>
        /// Transfer of an entire zone.
        /// </summary>
        /// <remarks>SAee RFCs 1035, 5936.</remarks>
        AXFR = 252,
        /// <summary>
        /// Mailbox-related RRs.
        /// </summary>
        MAILB = 253,
        /// <summary>
        /// Mail agent RRs.
        /// </summary>
        [Obsolete("Use MX.")]
        MAILA = 254,
        /// <summary>
        /// A request for all recoreds the server/cache has available.
        /// </summary>
        ALL = 255,
        /// <summary>
        /// URI
        /// </summary>
        /// <remarks>See RFC 7553.</remarks>
        URI = 256,
        /// <summary>
        /// Certificate Authority Restriction
        /// </summary>
        /// <remarks>See RFC 6844.</remarks>
        CAA = 257,
        /// <summary>
        /// Application Visibility and Control
        /// </summary>
        /// <remarks>See https://www.iana.org/assignments/dns-parameters/AVC/avc-completed-template.</remarks>
        AVC = 258,
        /// <summary>
        /// Digital Object Architecture
        /// </summary>
        /// <remarks>See https://www.iana.org/assignments/dns-parameters/DOA/doa-completed-template.</remarks>
        DOA = 259,
        /// <summary>
        /// DNSSEC trust authorities.
        /// </summary>
        /// <remarks>See http://www.watson.org/~weiler/INI1999-19.pdf.</remarks>
        TA = 32768,
        /// <summary>
        /// DNSSEC lookaside validation.
        /// </summary>
        /// <remarks>See RFC 4431.</remarks>
        DLV = 32769
    }
}
