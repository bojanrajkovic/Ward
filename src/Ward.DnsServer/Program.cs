using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

using Ward.Dns;

namespace Ward.DnsServer
{
    class Program
    {
        static void ReceiveCallback(IAsyncResult ar) {
            Console.WriteLine("Received data...");

            var state = (ValueTuple<UdpClient, IPEndPoint>)ar.AsyncState;
            var (client, endpoint) = state;

            var received = client.EndReceive(ar, ref endpoint);
            var message = MessageParser.ParseMessage(received);

            foreach (var q in message.Questions) {
                Console.WriteLine($"Q: {q}");
            }

            var header = message.Header;
            var flags = header.Flags;
            var response = new Message(
                new Header(
                    header.Id,
                    header.Opcode,
                    ReturnCode.NoError,
                    new Header.HeaderFlags(
                        flags.Query,
                        false,
                        false,
                        flags.Recurse,
                        true,
                        false,
                        false,
                        flags.CheckingDisabled
                    ),
                    0,
                    1,
                    0,
                    0
                ),
                Array.Empty<Question>(),
                new Record[] {
                    new Ward.Dns.Records.AddressRecord("lol.com", Dns.Type.A, Class.Internet, 600, IPAddress.Parse("5.5.5.5"))
                },
                Array.Empty<Record>(),
                Array.Empty<Record>()
            );

            MessageWriter.SerializeMessageAsync(response, true).ContinueWith(t => {
                Console.WriteLine("Sending response...");
                client.SendAsync(t.Result, t.Result.Length, endpoint);
            });

            client.BeginReceive(new AsyncCallback(ReceiveCallback), (client, endpoint));
        }

        static void Main(string[] args)
        {
            var waiter = new ManualResetEventSlim();
            var endpoint = new IPEndPoint(IPAddress.Any, 5353);
            var client = new UdpClient(endpoint);

            Console.WriteLine("Started");

            client.BeginReceive(new AsyncCallback(ReceiveCallback), (client, endpoint));
            waiter.Wait();
        }
    }
}
