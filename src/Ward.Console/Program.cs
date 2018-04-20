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

            string dnsServer = "172.16.128.1";
            if (args.Length == 3 && args[2][0] == '@')
                dnsServer = args[2].Substring(1);

            SConsole.WriteLine($"Querying {dnsServer} for {args[1]} records for {args[0]}.");
            var client = new UdpDnsClient(dnsServer, 53);
            var resolve = await client.ResolveAsync(args[0], Enum.Parse<Dns.Type>(args[1]), Class.Internet);

            foreach (var record in resolve.Results)
                SConsole.WriteLine(record);

            return 0;
        }
    }
}
