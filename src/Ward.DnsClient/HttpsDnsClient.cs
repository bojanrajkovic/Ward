﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Security;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

using Ward.Dns;
using Type = Ward.Dns.Type;

namespace Ward.DnsClient
{
    public class HttpsDnsClient : IDnsClient
    {
        const int MaxConnections = 10;

        static readonly Version http20Version = new Version(2, 0);

        readonly string host;
        readonly ushort port;
        readonly string tlsHost;
        readonly string expectedSpkiPin;
        readonly HttpClient httpClient;

        public HttpsDnsClient(string host, ushort port, string tlsHost, string expectedSpkiPin = null)
        {
            this.host = host;
            this.port = port;
            this.tlsHost = tlsHost;
            this.expectedSpkiPin = expectedSpkiPin;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
                var handler = new WinHttpHandler();
                handler.MaxConnectionsPerServer = MaxConnections;
                if (!string.IsNullOrWhiteSpace(expectedSpkiPin))
                    handler.ServerCertificateValidationCallback = CheckServerCertificateMatchesExpectedHash;
                httpClient = new HttpClient(handler);
            } else {
                var handler = new HttpClientHandler();
                handler.MaxConnectionsPerServer = MaxConnections;
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

        public Task<IResolveResult> ResolveAsync(Question question, CancellationToken cancellationToken = default) =>
            ResolveAsync(new[] { question }, cancellationToken);

        public async Task<IResolveResult> ResolveAsync(IEnumerable<Question> questions, CancellationToken cancellationToken = default)
        {
            if (questions.Count() > ushort.MaxValue)
                throw new ArgumentException("Too many questions for a single message.");

            var flags = new Header.HeaderFlags(true, false, false, true, true, false, false, false);
            var message = new Message(
                new Header(null, Opcode.Query, ReturnCode.NoError, flags, (ushort)questions.Count(), 0, 0, 0),
                questions.ToArray(),
                Array.Empty<Record>(),
                Array.Empty<Record>(),
                Array.Empty<Record>()
            );

            var msg = new HttpRequestMessage {
                Version = http20Version,
                Content = new ByteArrayContent(await MessageWriter.SerializeMessageAsync(message)) {
                    Headers = {
                        ContentType = MediaTypeHeaderValue.Parse("application/dns-udpwireformat")
                    }
                },
                RequestUri = new UriBuilder("https", host.ToString(), port, "dns-query").Uri,
                Method = HttpMethod.Post
            };
            msg.Headers.TryAddWithoutValidation("Accept", "application/dns-udpwireformat");
            msg.Headers.TryAddWithoutValidation("Host", tlsHost);
            var response = await httpClient.SendAsync(msg, cancellationToken);
            var content = await response.Content.ReadAsByteArrayAsync();
            var result = MessageParser.ParseMessage(content, 0);

            return new ResolveResult(result, content.Length);
        }

        public Task<IResolveResult> ResolveAsync(string host, Type type, Class @class, CancellationToken cancellationToken = default) =>
            ResolveAsync(new Question(host, type, @class), cancellationToken);
    }
}
