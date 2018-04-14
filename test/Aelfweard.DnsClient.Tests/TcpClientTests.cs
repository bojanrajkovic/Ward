using System.Net;
using System.Threading.Tasks;

using Aelfweard.Dns;
using Aelfweard.Dns.Records;
using Xunit;

namespace Aelfweard.DnsClient.Tests
{
    public class TcpClientTests
    {
        [Fact]
        public async Task Can_resolve_over_non_TLS_TCP_connection()
        {
            var client = new TcpDnsClient(IPAddress.Parse("1.1.1.1"), 53, false);
            var resolve = await client.ResolveAsync("example.com", Type.A, Class.Internet);

            Assert.NotNull(resolve);
            Assert.Single(resolve.Results);

            var a = Assert.IsType<AddressRecord>(resolve.Results[0]);
            Assert.Equal("93.184.216.34", a.Address.ToString());
            Assert.Equal(Class.Internet, a.Class);
        }

        [Fact]
        public async Task Can_resolve_over_TLS_TCP_connection()
        {
            var client = new TcpDnsClient(IPAddress.Parse("1.1.1.1"), 853, true, "cloudflare-dns.com");
            var resolve = await client.ResolveAsync("example.com", Type.A, Class.Internet);

            Assert.NotNull(resolve);
            Assert.Single(resolve.Results);

            var a = Assert.IsType<AddressRecord>(resolve.Results[0]);
            Assert.Equal("93.184.216.34", a.Address.ToString());
            Assert.Equal(Class.Internet, a.Class);
        }

        [Fact]
        public async Task Can_resolve_over_TLS_TCP_connection_with_spki()
        {
            var client = new TcpDnsClient(IPAddress.Parse("145.100.185.15"), 853, true, "dnsovertls.sinodun.com", "62lKu9HsDVbyiPenApnc4sfmSYTHOVfFgL3pyB+cBL4=");
            var resolve = await client.ResolveAsync("example.com", Type.A, Class.Internet);

            Assert.NotNull(resolve);
            Assert.Single(resolve.Results);

            var a = Assert.IsType<AddressRecord>(resolve.Results[0]);
            Assert.Equal("93.184.216.34", a.Address.ToString());
            Assert.Equal(Class.Internet, a.Class);
        }
    }
}
