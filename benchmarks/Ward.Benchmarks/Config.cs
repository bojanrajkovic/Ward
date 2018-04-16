using System;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Toolchains.CsProj;

namespace Ward.Benchmarks
{
    public class Config : ManualConfig
    {
        public Config()
        {
            Add(Job.Default.With(CsProjCoreToolchain.NetCoreApp20));
            Add(Job.Default.With(CsProjClassicNetToolchain.Net461));
            Add(MemoryDiagnoser.Default);

            // If we're not in Appveyor, add hardware counters.
            if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("APPVEYOR")))
                Add(HardwareCounter.BranchMispredictions, HardwareCounter.BranchInstructions, HardwareCounter.CacheMisses);
        }
    }
}
