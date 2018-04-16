using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Toolchains.CsProj;

namespace Ward.Benchmarks
{
    public class MyConfig : ManualConfig
    {
        public MyConfig()
        {
            Add(Job.Default.With(CsProjCoreToolchain.NetCoreApp20));
            Add(Job.Default.With(CsProjClassicNetToolchain.Net461));
            Add(MemoryDiagnoser.Default);
            Add(HardwareCounter.BranchMispredictions, HardwareCounter.BranchInstructions, HardwareCounter.CacheMisses);
        }
    }
}
