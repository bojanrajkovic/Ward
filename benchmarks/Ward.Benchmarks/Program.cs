using System;

using BenchmarkDotNet.Running;

namespace Ward.Benchmarks
{
    class Program
    {
        static void Main(string[] args)
        {
            BenchmarkSwitcher.FromTypes(new [] {
                typeof(MessageParseBenchmark)
            }).Run(args);
        }
    }
}
