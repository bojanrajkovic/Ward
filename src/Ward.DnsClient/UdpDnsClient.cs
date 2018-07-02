using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

using Ward.Dns;

namespace Ward.DnsClient
{
    /// <summary>
    /// A standard UDP DNS client.
    /// </summary>
    /// <seealso cref="IDnsClient" />
    public class UdpDnsClient : IDnsClient
    {
        readonly UdpClient client;
        readonly SemaphoreSlim semaphore = new SemaphoreSlim(1);

        /// <summary>
        /// Initializes a new instance of the <see cref="UdpDnsClient"/> class.
        /// </summary>
        /// <param name="serverHost">The server host.</param>
        /// <param name="serverPort">The server port.</param>
        public UdpDnsClient(string serverHost, ushort serverPort)
        {
            client = new UdpClient();
            client.Connect(serverHost, serverPort);
        }

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

            var messageData = await MessageWriter.SerializeMessageAsync(
                message,
                writeOpt: true,
                udpPayloadSize: 4096
            );
            await client.SendAsync(messageData, messageData.Length);

            cancellationToken.ThrowIfCancellationRequested();

            UdpReceiveResult recvResult;
            try {
                await semaphore.WaitAsync();
                await client.SendAsync(messageData, messageData.Length);
                cancellationToken.ThrowIfCancellationRequested();
                recvResult = await client.ReceiveAsync();
                cancellationToken.ThrowIfCancellationRequested();
            } finally {
                semaphore.Release();
            }

            var respBytes = recvResult.Buffer;
            var response = MessageParser.ParseMessage(respBytes, 0);

            return new ResolveResult(response, recvResult.Buffer.Length);
        }

        /// <inheritdoc />
        public Task<IResolveResult> ResolveAsync(string host, Dns.Type type, Class @class, CancellationToken cancellationToken = default) =>
            ResolveAsync(new Question(host, type, @class), cancellationToken);
    }
}
