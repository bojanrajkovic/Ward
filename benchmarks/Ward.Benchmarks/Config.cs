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

            Add(Job.Default.With(CsProjCoreToolchain.NetCoreApp21));
            Add(isWindows ? Job.Default.With(CsProjClassicNetToolchain.Net461) : Job.Default.With(Runtime.Mono));
            Add(MemoryDiagnoser.Default);
        }
    }
}
