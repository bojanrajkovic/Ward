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

        public TimeSpan ConnectTimeout { get; }

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

        public Task<IResolveResult> ResolveAsync(string host, Type type, Class @class, CancellationToken cancellationToken = default) =>
            ResolveAsync(new Question(host, type, @class), cancellationToken);
    }
}
