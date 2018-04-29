using System.Threading.Tasks;

using Ward.Dns;
using Ward.Dns.Records;
using Xunit;

namespace Ward.DnsClient.Tests
{
    public class UdpClientTests
    {
        [Fact]
        public async Task Can_resolve_A_record()
        {
            var client = new UdpDnsClient("1.1.1.1", 53);
            var resolve = await client.ResolveAsync("example.com", Type.A, Class.Internet);

            Assert.NotNull(resolve);
            Assert.Single(resolve.Answers);
            Assert.Empty(resolve.Authority);
            Assert.Single(resolve.Additional);

            var opt = Assert.IsType<OptRecord>(resolve.Additional[0]);
            Assert.Equal(0, opt.Edns0Version);
            Assert.False(opt.DnsSecOk);
            Assert.Empty(opt.OptionalData);
            Assert.True(opt.UdpPayloadSize > 512);

            Assert.Single(resolve.Questions);
            Assert.NotNull(resolve.Header);
            Assert.True(resolve.MessageSize > 0);

            var a = Assert.IsType<AddressRecord>(resolve.Answers[0]);
            Assert.Equal("93.184.216.34", a.Address.ToString());
            Assert.Equal(Class.Internet, a.Class);
        }

        [Fact]
        public async Task Can_resolve_MX_record()
        {
            var client = new UdpDnsClient("1.1.1.1", 53);
            var resolve = await client.ResolveAsync("google.com", Type.MX, Class.Internet);

            Assert.NotNull(resolve);
            Assert.NotEmpty(resolve.Answers);
            Assert.Equal(5, resolve.Answers.Count);
            Assert.Empty(resolve.Authority);
            Assert.Single(resolve.Additional);

            var opt = Assert.IsType<OptRecord>(resolve.Additional[0]);
            Assert.Equal(0, opt.Edns0Version);
            Assert.False(opt.DnsSecOk);
            Assert.Empty(opt.OptionalData);
            Assert.True(opt.UdpPayloadSize > 512);

            Assert.Single(resolve.Questions);
            Assert.NotNull(resolve.Header);
            Assert.True(resolve.MessageSize > 0);
        }

        [Fact]
        public async Task Multiple_threads_can_safely_query()
        {
            var client = new UdpDnsClient("1.1.1.1", 53);
            var tasks = new Task<IResolveResult>[5];
            for (var i = 0; i < 5; i++)
                tasks[i] = client.ResolveAsync("example.com", Dns.Type.A, Class.Internet);
            var responses = await Task.WhenAll(tasks);

            Assert.All(responses, resp => {
                Assert.Collection(resp.Answers, answer => {
                    Assert.Equal("example.com.", answer.Name);
                    var a = Assert.IsType<AddressRecord>(answer);
                    Assert.Equal("93.184.216.34", a.Address.ToString());
                });
            });
        }
    }
}
