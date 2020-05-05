using BenchmarkDotNet.Running;
using Patcher6502;

namespace Ninu.Visual6502.Benchmark
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            BenchmarkRunner.Run<CyclesBenchmarks>();

            //var simulator = CreateSimulator();

            //for (var i = 0; i < 100_000; i++)
            //{
            //    simulator.HalfStep();
            //    simulator.HalfStep();
            //}
        }

        private static Simulator CreateSimulator()
        {
            var simulator = new Simulator();

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

            simulator.SetMemory(data);

            simulator.Init();
            simulator.RunStartProgram();

            return simulator;
        }
    }
}