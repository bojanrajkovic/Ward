using System;
using System.Net.Sockets;
using System.Threading.Tasks;

using Ward.Dns;

namespace Ward.DnsClient
{
    public class UdpDnsClient : IDnsClient
    {
        readonly UdpClient client;

        public UdpDnsClient(string serverHost, ushort serverPort)
        {
            client = new UdpClient();
            client.Connect(serverHost, serverPort);
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
                Array.Empty<IRecord>(),
                Array.Empty<IRecord>(),
                Array.Empty<IRecord>()
            );
            var messageData = await MessageWriter.SerializeMessageAsync(message);
            await client.SendAsync(messageData, messageData.Length);

            var recvResult = await client.ReceiveAsync();
            var response = MessageParser.ParseMessage(recvResult.Buffer, 0);

            return new ResolveResult(response.Answers);
        }

        public Task<IResolveResult> ResolveAsync(string host, Dns.Type type, Class @class) =>
            ResolveAsync(new Question(host, type, @class));
    }
}
