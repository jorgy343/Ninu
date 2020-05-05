using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Patcher6502;

namespace Ninu.Visual6502.Benchmark
{
    [SimpleJob(RuntimeMoniker.NetCoreApp31, baseline: true)]
    [SimpleJob(RuntimeMoniker.CoreRt31)]
    [SimpleJob(RuntimeMoniker.Mono)]
    public class CyclesBenchmarks
    {
#nullable disable
        private Simulator _simulator;
#nullable restore

        //[Params(1_000, 1_000_000)]
        public int CycleCount = 1_000;

        [GlobalSetup]
        public void Setup()
        {
            _simulator = new Simulator();

            var assembler = new PatchAssembler();

            var asm = @"
                .org $0000

                loop: jmp loop

                .org $f000
                rti

                .org $fffa
                nmi .addr $f000

                .org $fffc
                reset .addr $0000

                .org $fffe
                irq .addr $f000
            ".Replace(".org", "* =");

            var data = assembler.Assemble(0, null, asm);

            _simulator.SetMemory(data);

            _simulator.Init();
            _simulator.RunStartProgram();
        }

        [Benchmark]
        public void RunCycles() => _simulator.ExecuteCycles(CycleCount);
    }
}