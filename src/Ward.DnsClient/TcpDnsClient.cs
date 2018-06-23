using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Runtime.ExceptionServices;
using System.Security;
using System.Security.Authentication;
using System.Threading;
using System.Threading.Tasks;

using Ward.Dns;
using Type = Ward.Dns.Type;

namespace Ward.DnsClient
{
    /// <summary>
    /// A TCP DNS client.
    /// </summary>
    /// <seealso cref="Ward.DnsClient.IDnsClient" />
    public class TcpDnsClient : IDnsClient
    {
        static readonly ArrayPool<byte> BufferPool = ArrayPool<byte>.Shared;

        readonly SemaphoreSlim semaphore = new SemaphoreSlim(1);
        readonly string host;
        readonly ushort port;
        readonly string tlsHost;
        readonly string expectedSpkiPin;
        readonly bool tls;
        readonly TcpClient tcpClient;
        Stream stream;

        /// <summary>
        /// Gets the connect timeout.
        /// </summary>
        /// <value>
        /// The connect timeout.
        /// </value>
        public TimeSpan ConnectTimeout { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TcpDnsClient"/> class.
        /// </summary>
        /// <param name="host">The DNS server host.</param>
        /// <param name="port">The DNS server port.</param>
        /// <param name="tls">If set to <c>true</c>, initiate a TLS connection.</param>
        /// <param name="tlsHost">The TLS host.</param>
        /// <param name="expectedSpkiPin">The expected SPKI pin.</param>
        /// <param name="connectTimeout">The connection timeout.</param>
        public TcpDnsClient(string host, ushort port, bool tls, string tlsHost = "", string expectedSpkiPin = "", int connectTimeout = 5000)
        {
            this.host = host;
            this.port = port;
            this.tls = tls;
            this.tlsHost = tlsHost;
            this.expectedSpkiPin = expectedSpkiPin;

            tcpClient = new TcpClient();
            ConnectTimeout = TimeSpan.FromMilliseconds(connectTimeout);
        }

        /// <summary>
        /// Asynchronously connect to the server..
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The connected stream, ready for use.</returns>
        /// <exception cref="TimeoutException">
        /// If the connection timeout elapses without the connection having
        /// been established.
        /// </exception>
        /// <exception cref="Exception">
        /// If the client fails to connect for some unknown reason.
        /// </exception>
        /// <exception cref="SecurityException">
        /// If there is an error establishing a secure connection.
        /// </exception>
        async Task<Stream> ConnectAsync(CancellationToken cancellationToken)
        {
            if (tcpClient.Connected)
                return stream;

            var connectTask = tcpClient.ConnectAsync(host, port);
            var timeout = Task.Delay(ConnectTimeout);

            cancellationToken.ThrowIfCancellationRequested();

            var first = await Task.WhenAny(connectTask, timeout);

            cancellationToken.ThrowIfCancellationRequested();

            if (first == timeout)
                throw new TimeoutException($"Timeout connecting to {host}:{port}");

            if (first.IsFaulted)
                ExceptionDispatchInfo.Capture (first.Exception.InnerException).Throw ();

            if (!tcpClient.Connected)
                throw new Exception($"Failed to connect to {host}:{port}");

            cancellationToken.ThrowIfCancellationRequested();

            stream = tcpClient.GetStream();

            if (!tls)
                return stream;

            cancellationToken.ThrowIfCancellationRequested();

            var sslStream = new SslStream(stream, false);
            await sslStream.AuthenticateAsClientAsync(tlsHost ?? host.ToString(), null, SslProtocols.Tls12, true);

            stream = sslStream;

            cancellationToken.ThrowIfCancellationRequested();

            if (string.IsNullOrWhiteSpace(expectedSpkiPin))
                return stream;

            var remoteCert = sslStream.RemoteCertificate;
            var spkiPinHash = remoteCert.GetSpkiPinHash();

            if (spkiPinHash != expectedSpkiPin)
                throw new SecurityException($"SPKI hash {spkiPinHash} did not match expected SPKI hash {expectedSpkiPin}");

            return stream;
        }

        /// <summary>
        /// Asynchronously resolves the given question.
        /// </summary>
        /// <param name="question">The question to resolve.</param>
        /// <param name="cancellationToken">A cancellation token for the operation.</param>
        /// <returns>
        /// A DNS resolve result.
        /// </returns>
        public Task<IResolveResult> ResolveAsync(Question question, CancellationToken cancellationToken = default) =>
            ResolveAsync(new[] { question }, cancellationToken);

        /// <summary>
        /// Resolves all of the given questions.
        /// </summary>
        /// <param name="questions">The list of questions.</param>
        /// <param name="cancellationToken">A cancellation token for the operation.</param>
        /// <returns>
        /// A DNS resolve result for all the questions.
        /// </returns>
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

            var messageData = await MessageWriter.SerializeMessageAsync(message);
            var messageLengthOctet = BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)messageData.Length));

            byte[] responseLengthBuf, responseBuf;
            try {
                await semaphore.WaitAsync(cancellationToken);

                var stream = await ConnectAsync(cancellationToken);

                await stream.WriteAsync(messageLengthOctet, 0, messageLengthOctet.Length, cancellationToken);
                await stream.WriteAsync(messageData, 0, messageData.Length, cancellationToken);

                responseLengthBuf = BufferPool.Rent(2);
                await stream.ReadAsync(responseLengthBuf, 0, 2, cancellationToken);
                var responseLength = IPAddress.NetworkToHostOrder(BitConverter.ToInt16(responseLengthBuf, 0));
                responseBuf = BufferPool.Rent(responseLength);
                await stream.ReadAsync(responseBuf, 0, responseLength, cancellationToken);
            } finally {
                semaphore.Release();
            }

            var response = MessageParser.ParseMessage(responseBuf, 0);
            var result = new ResolveResult(response, responseBuf.Length);

            BufferPool.Return(responseLengthBuf);
            BufferPool.Return(responseBuf);

            return result;
        }

        /// <summary>
        /// Asynchronously looks up the <paramref name="type" /> record in the
        /// given <paramref name="class" /> for the <paramref name="host" />.
        /// </summary>
        /// <param name="host">The host to resolve.</param>
        /// <param name="type">The record type to resolve.</param>
        /// <param name="class">The record class to resolve.</param>
        /// <param name="cancellationToken">A cancellation token for the operation.</param>
        /// <returns>
        /// A DNS resolve result.
        /// </returns>
        public Task<IResolveResult> ResolveAsync(string host, Type type, Class @class, CancellationToken cancellationToken = default) =>
            ResolveAsync(new Question(host, type, @class), cancellationToken);
    }
}
