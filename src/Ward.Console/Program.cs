using System;
using System.Linq;
using Mono.Options;

namespace Ward.Console
{
    static class Program
    {
        public static Version Version => typeof(Program).Assembly.GetName().Version;
        public static int Verbosity { get; private set; }

        static readonly CommandSet wardCommandSet = new CommandSet("ward") {
            "usage: ward COMMAND [OPTIONS]",
            string.Empty,
            "A console tool for using Ward.",
            string.Empty,
            "Global options:",
            { "v|verbose", "Output verbosity. Specify multiple times to increase verbosity further.", _ => Verbosity++ },
            string.Empty,
            "Available commands:",
            new DigCommand(),
            new TuiCommand()
        };

        static int Main(string[] args) => wardCommandSet.Run(args);
    }
}
