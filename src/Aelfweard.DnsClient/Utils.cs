using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

using Asn1;

namespace Aelfweard.DnsClient
{
    static class Utils
    {
        static byte[] Hash(HashAlgorithm hash, byte[] data)
        {
            if (hash == null)
                throw new ArgumentNullException(nameof(hash));

            return hash.ComputeHash(data);
        }

        public static string GetSpkiPinHash(this X509Certificate certificate)
        {
            // The things I do for love.
            var asnTree = AsnElt.Decode(certificate.GetRawCertData());
            // SPKI comes after version, serial number, signature algorithm, issuer, validity, subject
            var spki = asnTree.Sub[0].Sub[6].Encode();
            return Convert.ToBase64String(Hash(SHA256.Create(), spki));
        }
    }
}
