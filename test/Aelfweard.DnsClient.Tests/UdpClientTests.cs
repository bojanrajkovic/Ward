using System;
using System.Threading.Tasks;

using Aelfweard.Dns;
using Aelfweard.Dns.Records;
using Xunit;
using Xunit.Abstractions;

namespace Aelfweard.DnsClient.Tests
{
    public class UdpClientTests
    {
        readonly ITestOutputHelper output;

        public UdpClientTests(ITestOutputHelper output) =>
            this.output = output;

        [Fact]
        public async Task Can_resolve_A_record()
        {
            var client = new UdpDnsClient("1.1.1.1", 53);
            var resolve = await client.ResolveAsync("example.com", Dns.Type.A, Class.Internet);

            Assert.NotNull(resolve);
            Assert.Single(resolve.Results);

            var a = Assert.IsType<AddressRecord>(resolve.Results[0]);
            Assert.Equal("93.184.216.34", a.Address.ToString());
            Assert.Equal(Class.Internet, a.Class);
        }

        [Fact]
        public async Task Can_resolve_MX_record()
        {
            var client = new UdpDnsClient("1.1.1.1", 53);
            var resolve = await client.ResolveAsync("google.com", Dns.Type.MX, Class.Internet);

            Assert.NotNull(resolve);
            Assert.NotEmpty(resolve.Results);
            Assert.Equal(5, resolve.Results.Count);

            resolve.Results.ForEach(tup => System.Diagnostics.Debug.WriteLine(tup.ToString()));
        }
    }
}