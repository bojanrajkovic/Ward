using System;
using System.Collections.Generic;
using System.Linq;
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
    /// <summary>
    /// A DNS-over-HTTPS client.
    /// </summary>
    /// <seealso cref="Ward.DnsClient.IDnsClient" />
    public class HttpsDnsClient : IDnsClient
    {
        const int MaxConnections = 10;

        static readonly Version http20Version = new Version(2, 0);

        readonly string host;
        readonly ushort port;
        readonly string tlsHost;
        readonly string expectedSpkiPin;
        readonly HttpClient httpClient;

        /// <summary>
        /// Creates a new <see cref="HttpsDnsClient"/>.
        /// </summary>
        /// <param name="host">The server host.</param>
        /// <param name="port">The server port.</param>
        /// <param name="tlsHost">The hostname to use for TLS.</param>
        /// <param name="expectedSpkiPin">The expected SPKI hash of the server certificate.</param>
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

        /// <summary>
        /// Checks if the given server certificate matches the expected SPKI hash.
        /// </summary>
        /// <param name="req">The HTTP request.</param>
        /// <param name="cert">The server certificate.</param>
        /// <param name="chain">The certificate chain.</param>
        /// <param name="policyErrors">Any policy errors in validation of the certificate</param>
        /// <returns></returns>
        bool CheckServerCertificateMatchesExpectedHash(
            HttpRequestMessage req,
            X509Certificate2 cert,
            X509Chain chain,
            SslPolicyErrors policyErrors
        ) => policyErrors == SslPolicyErrors.None && cert.GetSpkiPinHash() == expectedSpkiPin;

        /// <inheritdoc />
        public Task<IResolveResult> ResolveAsync(Question question, CancellationToken cancellationToken = default) =>
                    ResolveAsync(new[] { question }, cancellationToken);

        /// <inheritdoc />
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

        /// <inheritdoc />
        public Task<IResolveResult> ResolveAsync(string host, Type type, Class @class, CancellationToken cancellationToken = default) =>
            ResolveAsync(new Question(host, type, @class), cancellationToken);
    }
}
