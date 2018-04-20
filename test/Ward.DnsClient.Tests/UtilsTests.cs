using System;
using Xunit;

namespace Ward.DnsClient.Tests
{
    public class UtilsTests
    {
        [Fact]
        public void Hash_throws_ANE_if_hash_null()
        {
            Assert.Throws<ArgumentNullException>(() => Utils.Hash(null, Array.Empty<byte>()));
        }
    }
}
