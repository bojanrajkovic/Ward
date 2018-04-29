using System;
using System.Net;
using System.Security;
using System.Threading.Tasks;

using Ward.Dns;
using Ward.Dns.Records;
using Xunit;

namespace Ward.DnsClient.Tests
{
    public class TcpClientTests
    {
        [Fact]
        public async Task Can_resolve_over_non_TLS_TCP_connection()
        {
            var client = new TcpDnsClient(IPAddress.Parse("1.1.1.1"), 53, false);
            var resolve = await client.ResolveAsync("example.com", Dns.Type.A, Class.Internet);

            Assert.NotNull(resolve);
            Assert.Single(resolve.Answers);

            var a = Assert.IsType<AddressRecord>(resolve.Answers[0]);
            Assert.Equal("93.184.216.34", a.Address.ToString());
            Assert.Equal(Class.Internet, a.Class);
        }

        [Fact]
        public async Task Can_resolve_over_TLS_TCP_connection()
        {
            var client = new TcpDnsClient(IPAddress.Parse("1.1.1.1"), 853, true, "cloudflare-dns.com");
            var resolve = await client.ResolveAsync("example.com", Dns.Type.A, Class.Internet);

            Assert.NotNull(resolve);
            Assert.Single(resolve.Answers);
            Assert.Empty(resolve.Authority);
            Assert.Empty(resolve.Additional);
            Assert.Single(resolve.Questions);
            Assert.NotNull(resolve.Header);
            Assert.True(resolve.MessageSize > 0);

            var a = Assert.IsType<AddressRecord>(resolve.Answers[0]);
            Assert.Equal("93.184.216.34", a.Address.ToString());
            Assert.Equal(Class.Internet, a.Class);
        }

        [Fact]
        public async Task Can_resolve_over_TLS_TCP_connection_with_spki()
        {
            var client = new TcpDnsClient(IPAddress.Parse("145.100.185.15"), 853, true, "dnsovertls.sinodun.com", "62lKu9HsDVbyiPenApnc4sfmSYTHOVfFgL3pyB+cBL4=");
            var resolve = await client.ResolveAsync("example.com", Dns.Type.A, Class.Internet);

            Assert.NotNull(resolve);
            Assert.Single(resolve.Answers);
            Assert.Empty(resolve.Authority);
            Assert.Empty(resolve.Additional);
            Assert.Single(resolve.Questions);
            Assert.NotNull(resolve.Header);
            Assert.True(resolve.MessageSize > 0);

            var a = Assert.IsType<AddressRecord>(resolve.Answers[0]);
            Assert.Equal("93.184.216.34", a.Address.ToString());
            Assert.Equal(Class.Internet, a.Class);
        }

        [Fact]
        public async Task Bad_spki_throws_security_exception()
        {
            const string badSpkiHash = "72lKu9HsDVbyiPenApnc4sfmSYTHOVfFgL3pyB+cBL4=";

            var client = new TcpDnsClient(IPAddress.Parse("145.100.185.15"), 853, true, "dnsovertls.sinodun.com", badSpkiHash);
            var ex = await Assert.ThrowsAsync<SecurityException>(async () => {
                await client.ResolveAsync("example.com", Dns.Type.A, Class.Internet);
            });

            const string goodSpkiHash = "62lKu9HsDVbyiPenApnc4sfmSYTHOVfFgL3pyB+cBL4=";
            Assert.Equal($"SPKI hash {goodSpkiHash} did not match expected SPKI hash {badSpkiHash}", ex.Message);
        }

        [Fact]
        public async Task Timeout_connecting_actually_works()
        {
            var client = new TcpDnsClient(IPAddress.Parse("13.82.93.245"), 53, false, connectTimeout: 1000);
            var ex = await Assert.ThrowsAsync<TimeoutException>(async () => {
                await client.ResolveAsync("example.com", Dns.Type.A, Class.Internet);
            });
        }

        [Fact]
        public async Task Multiple_threads_can_safely_query()
        {
            var client = new TcpDnsClient(IPAddress.Parse("1.1.1.1"), 853, true, "cloudflare-dns.com");
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
