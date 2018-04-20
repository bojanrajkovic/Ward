using System;
using System.Runtime.InteropServices;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Toolchains.CsProj;

namespace Ward.Benchmarks
{
    public class Config : ManualConfig
    {
        public Config()
        {
            var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

            Add(Job.Default.With(CsProjCoreToolchain.NetCoreApp20));

            if (isWindows)
                Add(Job.Default.With(CsProjClassicNetToolchain.Net461));
            else
                Add(Job.Default.With(Runtime.Mono));

            Add(MemoryDiagnoser.Default);

            // If we're not in Appveyor, add hardware counters.
            if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("APPVEYOR")) && isWindows)
                Add(HardwareCounter.BranchMispredictions, HardwareCounter.BranchInstructions, HardwareCounter.CacheMisses);
        }
    }
}
