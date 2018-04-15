using System;
using System.Net;
using System.Threading.Tasks;

using Ward.Dns;
using Ward.DnsClient;

using SConsole = System.Console;

namespace Ward.Console
{
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            if (args.Length < 2) {
                SConsole.Error.WriteLine("Must specify a host to look up and a record type to look up.");
                return 1;
            }

            var client = new UdpDnsClient("172.16.128.1", 53);
            var resolve = await client.ResolveAsync(args[0], Enum.Parse<Dns.Type>(args[1]), Class.Internet);

            foreach (var record in resolve.Results)
                SConsole.WriteLine(record);

            return 0;
        }
    }
}
