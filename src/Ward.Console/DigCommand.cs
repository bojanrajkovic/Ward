using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using Mono.Options;
using Ward.Dns;
using Ward.Dns.Records;
using Ward.DnsClient;

using SConsole = System.Console;

namespace Ward.Console
{
    class DigCommand : Command
    {
        Dns.Type queryType = Dns.Type.A;
        string serverHost;
        bool useTcp, useTls, useHttps, allowMultiQuestion, showHelp;
        ushort? port;
        string spkiHash;

        public DigCommand() : base("dig", "DNS lookup tool")
        {
            Options = new OptionSet {
                "usage: ward dig [OPTIONS] [NAMES]",
                string.Empty,
                "DNS lookup tool, like dig(1) on Unix systems.",
                { "help|?|h", "Show this message and exit.", v => showHelp = (v != null) },
                { "type|t=", "Record type to query. Defaults to A.", (Dns.Type type) => queryType = type },
                { "server|s=", "DNS server to query. Defaults to local preferred server.", hostName => serverHost = hostName },
                { "tcp", "Use DNS over TCP", v => useTcp = (v != null) },
                { "tls", "Use TLS with DNS over TCP", v => useTls = (v != null) },
                { "https", "Use DNS over HTTPS", v => useHttps = (v != null) },
                { "port|p=", "Connect on the specified {port}.", (ushort port) => this.port = port },
                { "spki-hash", "Expected SPKI hash when using TLS or DoH.", spkiHash => this.spkiHash = spkiHash },
                { "allow-multi-question", "Allow multiple questions in a single message.", v => allowMultiQuestion = (v != null) },
            };
        }

        public override int Invoke(IEnumerable<string> arguments)
        {
            try {
                var names = Options.Parse(arguments);

                if (showHelp) {
                    Options.WriteOptionDescriptions(CommandSet.Out);
                    return 0;
                }

                if (!names.Any()) {
                    CommandSet.Error.WriteLine("ward: Missing names to look up—you must pass at least one name.");
                    CommandSet.Error.WriteLine("ward: Run `ward help dig` for details.");
                    return 1;
                }

                if (names.Count > 1 && !allowMultiQuestion) {
                    CommandSet.Error.WriteLine("ward: Most DNS servers do not respond well to multiple questions in one message.");
                    CommandSet.Error.WriteLine("ward: If you're sure you know what you're doing, pass --allow-multi-question.");
                    return 1;
                }

                if (useTcp && useHttps) {
                    CommandSet.Error.WriteLine("ward: --tcp and --https are mutually incompatible options, please only specify one.");
                    return 1;
                }

                return RunAsync(names).ConfigureAwait(false).GetAwaiter().GetResult();
            } catch (Exception e) {
                CommandSet.Error.WriteLine("ward: {0}", Program.Verbosity >= 1 ? e.ToString() : e.Message);
                return 1;
            }
        }

        public async Task<int> RunAsync(IEnumerable<string> names)
        {
            var queryTime = DateTime.Now;

            string dnsServer = serverHost ?? Utils.GetDnsAddresses().First().ToString();
            IDnsClient client;

            if (useTcp)
                client = new TcpDnsClient(
                    dnsServer,
                    port ?? (port = (ushort)(useTls ? 853 : 53)).Value,
                    useTls,
                    tlsHost: dnsServer,
                    expectedSpkiPin: spkiHash
                );
            else if (useHttps)
                client = new HttpsDnsClient(
                    dnsServer,
                    port ?? (port = 443).Value,
                    dnsServer,
                    expectedSpkiPin: spkiHash
                );
            else
                client = new UdpDnsClient(dnsServer, port ?? (port = 53).Value);

            var timer = Stopwatch.StartNew();
            var questions = names.Select(n => new Question(n, queryType, Class.Internet));
            var resolve = await client.ResolveAsync(questions);
            timer.Stop();

            SConsole.WriteLine($"; <<>> ward dig {Program.Version.ToString(3)} <<>> {string.Join(", ", names)} {queryType}");
            SConsole.WriteLine(";; Got answer:");
            SConsole.WriteLine($";; ->>HEADER<<- opcode: {resolve.Header.Opcode}, status: {resolve.Header.ReturnCode}, id: {resolve.Header.Id}");
            SConsole.WriteLine($";; flags: {resolve.Header.Flags.ToString()}; QUERY: {resolve.Questions.Count}, ANSWER: {resolve.Answers.Count}, AUTHORITY: {resolve.Authority.Count}, ADDITIONAL: {resolve.Additional.Count}");

            SConsole.WriteLine();

            var optRecord = resolve.Additional.OfType<OptRecord>().SingleOrDefault();
            if (optRecord != null) {
                SConsole.WriteLine(";; OPT PSEUDOSECTION:");
                SConsole.WriteLine($"; EDNS: version: {optRecord.Edns0Version}, flags:{(optRecord.DnsSecOk ? "do" : "")}; udp: {optRecord.UdpPayloadSize}");
            }

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
                foreach (var record in resolve.Additional) {
                    if (record is OptRecord _)
                        continue;
                    SConsole.WriteLine(record);
                }
            }

            var stringFormat = $"ddd MMM dd HH:mm:ss \"{TimeZoneName(queryTime)}\" yyyy";
            var proto = useHttps ? "DoH" : (useTcp ? (useTls ? "TLS" : "TCP") : "UDP");

            SConsole.WriteLine();
            SConsole.WriteLine($";; Query time: {timer.ElapsedMilliseconds} msec");
            SConsole.WriteLine($";; SERVER: {dnsServer}#{port} - proto: {proto}");
            SConsole.WriteLine($";; WHEN: {queryTime.ToString(stringFormat)}");
            SConsole.WriteLine($";; MSG SIZE  rcvd: {resolve.MessageSize}");

            return 0;
        }

        // This is a dumb hack.
        static string TimeZoneName(DateTime dt)
        {
            var tzName = TimeZoneInfo.Local.IsDaylightSavingTime(dt)
                ? TimeZoneInfo.Local.DaylightName
                : TimeZoneInfo.Local.StandardName;

            return new string(tzName.Split(" ").Select(s => s[0]).ToArray());
        }
    }
}
