﻿using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

using Aelfweard.Dns;
using Type = Aelfweard.Dns.Type;

namespace Aelfweard.DnsClient
{
    public class HttpsDnsClient : IDnsClient
    {
        static readonly Version http20Version = new Version(2, 0);

        readonly IPAddress address;
        readonly ushort port;
        readonly string tlsHost;
        readonly string expectedCertificateHash;
        readonly HttpClient httpClient;

        public HttpsDnsClient(IPAddress address, ushort port, string tlsHost, string expectedCertificateHash = null)
        {
            this.address = address;
            this.port = port;
            this.tlsHost = tlsHost;
            this.expectedCertificateHash = expectedCertificateHash;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                httpClient = new HttpClient(new WinHttpHandler());
            else
                httpClient = new HttpClient();
        }

        public async Task<IResolveResult> ResolveAsync(Question question)
        {
            var message = new Message(
                new Header(
                    null,
                    true,
                    Opcode.Query,
                    false,
                    false,
                    true,
                    true,
                    false,
                    false,
                    false,
                    ReturnCode.NoError,
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
