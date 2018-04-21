using System.Net;
using System.Threading.Tasks;

using Ward.Dns;
using Ward.Dns.Records;
using Xunit;

namespace Ward.DnsClient.Tests
{
    public class HttpsClientTests
    {
        [Fact]
        public async Task Can_resolve_A_records_via_HTTPS()
        {
            var client = new HttpsDnsClient(IPAddress.Parse("1.1.1.1"), 443, "cloudflare-dns.com");
            var resolve = await client.ResolveAsync("example.com", Type.A, Class.Internet);

            Assert.NotNull(resolve);
            Assert.Single(resolve.Answers);

            var a = Assert.IsType<AddressRecord>(resolve.Answers[0]);
            Assert.Equal("93.184.216.34", a.Address.ToString());
            Assert.Equal(Class.Internet, a.Class);
        }

        [Fact]
        public async Task Can_resolve_A_records_via_HTTPS_with_SPKI()
        {
            var client = new HttpsDnsClient(IPAddress.Parse("1.1.1.1"), 443, "cloudflare-dns.com", "yioEpqeR4WtDwE9YxNVnCEkTxIjx6EEIwFSQW+lJsbc=");
            var resolve = await client.ResolveAsync("example.com", Type.A, Class.Internet);

            Assert.NotNull(resolve);
            Assert.Single(resolve.Answers);

            var a = Assert.IsType<AddressRecord>(resolve.Answers[0]);
            Assert.Equal("93.184.216.34", a.Address.ToString());
            Assert.Equal(Class.Internet, a.Class);
        }
    }
}
