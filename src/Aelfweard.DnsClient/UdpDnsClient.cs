using System;
using System.Net.Sockets;
using System.Threading.Tasks;

using Aelfweard.Dns;

namespace Aelfweard.DnsClient
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
            await client.SendAsync(messageData, messageData.Length);

            var recvResult = await client.ReceiveAsync();
            var response = Message.ParseFromBytes(recvResult.Buffer, 0);

            return new ResolveResult(response.Answers);
        }

        public Task<IResolveResult> ResolveAsync(string host, Dns.Type type, Class @class) =>
            ResolveAsync(new Question(host, type, @class));
    }
}
