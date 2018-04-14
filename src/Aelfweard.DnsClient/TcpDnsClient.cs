using System;
using System.Buffers;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Runtime.ExceptionServices;
using System.Security;
using System.Security.Authentication;
using System.Threading.Tasks;

using Aelfweard.Dns;
using Type = Aelfweard.Dns.Type;

namespace Aelfweard.DnsClient
{
    public class TcpDnsClient : IDnsClient
    {
        static readonly ArrayPool<byte> BufferPool = ArrayPool<byte>.Shared;

        readonly IPAddress address;
        readonly ushort port;
        readonly string tlsHost;
        readonly string expectedSpkiPin;
        readonly bool tls;
        readonly TcpClient tcpClient;

        public TimeSpan ConnectTimeout { get; }

        public TcpDnsClient(IPAddress address, ushort port, bool tls, string tlsHost = "", string expectedSpkiPin = "", int connectTimeout = 5000)
        {
            this.address = address;
            this.port = port;
            this.tls = tls;
            this.tlsHost = tlsHost;
            this.expectedSpkiPin = expectedSpkiPin;

            tcpClient = new TcpClient();
            ConnectTimeout = TimeSpan.FromMilliseconds(connectTimeout);
        }

        async Task<Stream> ConnectAsync()
        {
            var connectTask = tcpClient.ConnectAsync(address, port);
            var timeout = Task.Delay(ConnectTimeout);

            var first = await Task.WhenAny(connectTask, timeout);

            if (first == timeout)
                throw new TimeoutException($"Timeout connecting to {address}:{port}");

            if (first.IsFaulted)
                ExceptionDispatchInfo.Capture (first.Exception.InnerException).Throw ();

            if (!tcpClient.Connected)
                throw new Exception($"Failed to connect to {address}:{port}");

            var stream = tcpClient.GetStream();

            if (!tls)
                return stream;

            var sslStream = new SslStream(stream, false);
            await sslStream.AuthenticateAsClientAsync(tlsHost, null, SslProtocols.Tls12, true);

            if (string.IsNullOrWhiteSpace(expectedSpkiPin))
                return sslStream;

            var remoteCert = sslStream.RemoteCertificate;
            var spkiPinHash = remoteCert.GetSpkiPinHash();

            if (spkiPinHash != expectedSpkiPin)
                throw new SecurityException($"SPKI hash {spkiPinHash} did not match expected SPKI hash {expectedSpkiPin}");

            return sslStream;
        }

        public async Task<IResolveResult> ResolveAsync(Question question)
        {
            var stream = await ConnectAsync();

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

            var messageData = await message.SerializeAsync();
            var messageLengthOctet = BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)messageData.Length));

            await stream.WriteAsync(messageLengthOctet, 0, messageLengthOctet.Length);
            await stream.WriteAsync(messageData, 0, messageData.Length);

            var responseLengthBuf = BufferPool.Rent(2);
            await stream.ReadAsync(responseLengthBuf, 0, 2);
            var responseLength = IPAddress.NetworkToHostOrder(BitConverter.ToInt16(responseLengthBuf, 0));
            var responseBuf = BufferPool.Rent(responseLength);
            await stream.ReadAsync(responseBuf, 0, responseLength);

            var result = new ResolveResult(Message.ParseFromBytes(responseBuf, 0).Answers);

            BufferPool.Return(responseLengthBuf);
            BufferPool.Return(responseBuf);

            return result;
        }

        public Task<IResolveResult> ResolveAsync(string host, Type type, Class @class) =>
            ResolveAsync(new Question(host, type, @class));
    }
}
