using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Mono.Options;
using Terminal.Gui;
using Ward.DnsClient;

namespace Ward.Console
{
    class TuiCommand : Command
    {
        IDnsClient dnsClient;

        public TuiCommand() : base("tui", "Curses-based terminal DNS UI.")
        {
            dnsClient = new UdpDnsClient("1.1.1.1", 53);
        }

        public override int Invoke(IEnumerable<string> arguments)
        {
            Application.Init();

            var top = Application.Top;
            var win = new Window("Ward") {
                X = 0,
                Y = 1,
                Width = Dim.Fill(),
                Height = Dim.Fill()
            };
            top.Add(win);

            var menu = new MenuBar(new MenuBarItem[] {
                new MenuBarItem ("_File", new MenuItem[] {
                    new MenuItem ("_Quit", "", () => { top.Running = false; })
                })
            });
            top.Add(menu);

            var name = new Label("Name To Query") { X = 3, Y = 2 };
            var nameEntry = new TextField(string.Empty) { X = name.X + name.Text.Length + 1, Y = 2 };
            var queryLabel = new Label("Query Type") { X = 3, Y = 4 };
            var dnsTypeRadio = new RadioGroup(new [] {
                "A",
                "CAA",
                "CNAME",
                "MX",
                "NS",
                "PTR",
                "SOA",
                "TXT"
            }, 0) { X = queryLabel.X + queryLabel.Text.Length + 1, Y = 4 };

            TextView outputView = null;
            var quitButton = new Button("Quit") {
                X = 3,
                Y = 4 + dnsTypeRadio.RadioLabels.Length + 1,
                Clicked = () => { top.Running = false; }
            };
            var runQueryButton = new Button("Run Query") {
                X = Pos.Right(quitButton) + 4,
                Y = 4 + dnsTypeRadio.RadioLabels.Length + 1,
                Clicked = async () => {
                    try {
                        var timer = Stopwatch.StartNew();
                        var result = await dnsClient.ResolveAsync(
                            nameEntry.Text.ToString(),
                            Enum.Parse<Dns.Type>(dnsTypeRadio.RadioLabels[dnsTypeRadio.Selected]),
                            Dns.Class.Internet
                        );
                        timer.Stop();
                        var resultText = string.Join(Environment.NewLine, result.Answers)
                            + $"{Environment.NewLine}{Environment.NewLine}"
                            + $"Time: {timer.Elapsed.TotalMilliseconds}ms";
                        outputView.Text = resultText;
                        Application.Current.SetNeedsDisplay();
                    } catch (Exception e) {
                        MessageBox.ErrorQuery(50, 5, "Error", e.Message, "OK");
                    }
                }
            };

            outputView = new TextView() {
                X = 3,
                Y = 6 + dnsTypeRadio.RadioLabels.Length + 1,
                Width = Dim.Fill(),
                Height = Dim.Fill(),
                Text = "",
                CanFocus = false,
            };

            win.Add(
                name,
                nameEntry,
                queryLabel,
                dnsTypeRadio,
                quitButton,
                runQueryButton,
                outputView
            );

            Application.Run();

            return 0;
        }
    }
}
