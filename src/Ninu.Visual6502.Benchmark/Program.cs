using BenchmarkDotNet.Running;

namespace Ninu.Visual6502.Benchmark
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            BenchmarkRunner.Run<CyclesBenchmarks>();

            //var benchmark = new CyclesBenchmarks();

            //benchmark.Setup();
            //benchmark.RunCycles();
        }
    }
}