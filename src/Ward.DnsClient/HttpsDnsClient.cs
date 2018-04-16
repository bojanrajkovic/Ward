using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Security;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

using Ward.Dns;
using Type = Ward.Dns.Type;

namespace Ward.DnsClient
{
    public class HttpsDnsClient : IDnsClient
    {
        static readonly Version http20Version = new Version(2, 0);

        readonly IPAddress address;
        readonly ushort port;
        readonly string tlsHost;
        readonly string expectedSpkiPin;
        readonly HttpClient httpClient;

        public HttpsDnsClient(IPAddress address, ushort port, string tlsHost, string expectedSpkiPin = null)
        {
            this.address = address;
            this.port = port;
            this.tlsHost = tlsHost;
            this.expectedSpkiPin = expectedSpkiPin;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
                var handler = new WinHttpHandler();
                if (!string.IsNullOrWhiteSpace(expectedSpkiPin))
                    handler.ServerCertificateValidationCallback = CheckServerCertificateMatchesExpectedHash;
                httpClient = new HttpClient(handler);
            } else {
                var handler = new HttpClientHandler();
                if (!string.IsNullOrWhiteSpace(expectedSpkiPin))
                    handler.ServerCertificateCustomValidationCallback = CheckServerCertificateMatchesExpectedHash;
                httpClient = new HttpClient(handler);
            }
        }

        bool CheckServerCertificateMatchesExpectedHash(
            HttpRequestMessage req,
            X509Certificate2 cert,
            X509Chain chain,
            SslPolicyErrors policyErrors
        )
        {
            return policyErrors == SslPolicyErrors.None && cert.GetSpkiPinHash() == expectedSpkiPin;
        }

        public async Task<IResolveResult> ResolveAsync(Question question)
        {
            var message = new Message(
                new Header(
                    null,
                    Opcode.Query,
                    ReturnCode.NoError,
                    new Header.HeaderFlags(true, false, false, true, true, false, false, false),
                    1,
                    0,
                    0,
                    0
                ),
                new [] { question },
                Array.Empty<Record>(),
                Array.Empty<Record>(),
                Array.Empty<Record>()
            );

            var msg = new HttpRequestMessage {
                Version = http20Version,
                Content = new ByteArrayContent(await message.SerializeAsync()) {
                    Headers = {
                        ContentType = MediaTypeHeaderValue.Parse("application/dns-udpwireformat")
                    }
                },
                RequestUri = new UriBuilder("https", address.ToString(), port, "dns-query").Uri,
                Method = HttpMethod.Post
            };
            msg.Headers.TryAddWithoutValidation("Accept", "application/dns-udpwireformat");
            msg.Headers.TryAddWithoutValidation("Host", tlsHost);
            var response = await httpClient.SendAsync(msg);
            var content = await response.Content.ReadAsByteArrayAsync();
            var result = Message.ParseFromBytes(content, 0);

            return new ResolveResult(result.Answers);
        }

        public Task<IResolveResult> ResolveAsync(string host, Type type, Class @class) =>
            ResolveAsync(new Question(host, type, @class));
    }
}
