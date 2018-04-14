using System.Net;
using System.Threading.Tasks;

using Aelfweard.Dns;
using Aelfweard.DnsClient;

using SConsole = System.Console;

namespace Aelfweard.Console
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var client = new HttpsDnsClient(IPAddress.Parse("1.1.1.1"), 443, "cloudflare-dns.com", "yioEpqeR4WtDwE9YxNVnCEkTxIjx6EEIwFSQW+lJsbc=");
            var resolve = await client.ResolveAsync("example.com", Dns.Type.A, Class.Internet);

            foreach (var record in resolve.Results)
                SConsole.WriteLine(record);
        }
    }
}
