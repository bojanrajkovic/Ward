using System;
using System.Security.Cryptography;

namespace Aelfweard.DnsClient
{
    static class Utils
    {
        public static byte[] Hash(HashAlgorithm hash, byte[] data)
        {
            if (hash == null)
                throw new ArgumentNullException(nameof(hash));

            return hash.ComputeHash(data);
        }
    }
}
