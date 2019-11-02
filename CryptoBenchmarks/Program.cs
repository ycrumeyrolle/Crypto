using System.Reflection;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;

namespace Sha256Benchmarks
{
    class Program
    {
        static void Main(string[] args)
        {
#if DEBUG
            BenchmarkSwitcher.FromAssembly(typeof(Program).GetTypeInfo().Assembly).Run(args, new DebugInProcessConfig());
#else
            BenchmarkSwitcher.FromAssembly(typeof(Program).GetTypeInfo().Assembly).Run(args);
#endif
        }
    }
}
