using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

using Ward.Dns;
using Ward.DnsClient;

using SConsole = System.Console;

namespace Ward.Console
{
    class Program
    {
        public static Version Version => typeof(Program).Assembly.GetName().Version;

        static async Task<int> Main(string[] args)
        {
            if (args.Length < 2) {
                SConsole.Error.WriteLine("Must specify a host to look up and a record type to look up.");
                return 1;
            }

            string dnsServer = "172.16.128.1";
            if (args.Length == 3 && args[2][0] == '@')
                dnsServer = args[2].Substring(1);

            var queryTime = DateTime.Now;
            var client = new UdpDnsClient(dnsServer, 53);
            var queryType = Enum.Parse<Dns.Type>(args[1]);
            var timer = Stopwatch.StartNew();
            var resolve = await client.ResolveAsync(args[0], queryType, Class.Internet);
            timer.Stop();

            SConsole.WriteLine($"; <<>> ward-dig {Version} <<>> {args[0]} {args[1]}");
            SConsole.WriteLine(";; Got answer:");
            SConsole.WriteLine($";; ->>HEADER<<- opcode: {resolve.Header.Opcode}, status: {resolve.Header.ReturnCode}, id: {resolve.Header.Id}");
            SConsole.WriteLine($";; flags: {resolve.Header.Flags.ToString()}; QUERY: {resolve.Questions.Count}, ANSWER: {resolve.Answers.Count}, AUTHORITY: {resolve.Authority.Count}, ADDITIONAL: {resolve.Additional.Count}");

            SConsole.WriteLine();
            // TODO: Write OPT pseudosection once we support EDNS0.
            SConsole.WriteLine(";; QUESTION SECTION:");
            foreach (var question in resolve.Questions)
                SConsole.WriteLine($";{question.Name}\t{question.Class}\t{question.Type}");

            if (resolve.Answers.Count > 0) {
                SConsole.WriteLine();
                SConsole.WriteLine(";; ANSWER SECTION:");
                foreach (var record in resolve.Answers)
                    SConsole.WriteLine(record);
            }

            if (resolve.Authority.Count > 0) {
                SConsole.WriteLine();
                SConsole.WriteLine(";; AUTHORITY SECTION:");
                foreach (var record in resolve.Authority)
                    SConsole.WriteLine(record);
            }

            if (resolve.Additional.Count > 0) {
                SConsole.WriteLine();
                SConsole.WriteLine(";; ADDITIONAL SECTION:");
                foreach (var record in resolve.Additional)
                    SConsole.WriteLine(record);
            }

            var stringFormat = $"ddd MMM dd HH:mm:ss \"{TimeZoneName(queryTime)}\" yyyy";

            SConsole.WriteLine();
            SConsole.WriteLine($";; Query time: {timer.ElapsedMilliseconds} msec");
            SConsole.WriteLine($";; SERVER: {dnsServer}#53({dnsServer})");
            SConsole.WriteLine($";; WHEN: {queryTime.ToString(stringFormat)}");
            SConsole.WriteLine($";; MSG SIZE  rcvd: {resolve.MessageSize}");

            return 0;
        }

        // This is a dumb hack.
        public static String TimeZoneName(DateTime dt)
        {
            var tzName = TimeZoneInfo.Local.IsDaylightSavingTime(dt)
                ? TimeZoneInfo.Local.DaylightName
                : TimeZoneInfo.Local.StandardName;

            return new string(tzName.Split(" ").Select(s => s[0]).ToArray());
        }
    }
}
