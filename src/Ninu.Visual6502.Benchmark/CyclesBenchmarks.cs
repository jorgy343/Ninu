﻿using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Ninu.Base;
using Patcher6502;

namespace Ninu.Visual6502.Benchmark
{
    [SimpleJob(RuntimeMoniker.NetCoreApp50, baseline: true)]
    [SimpleJob(RuntimeMoniker.CoreRt50)]
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
            var assembler = new PatchAssembler();

            var asm = @"
                .org $0000

                loop:
                inx
                stx $a00
                jmp loop

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

            _simulator = new Simulator(new ArrayMemory(data));

            _simulator.Init();
            _simulator.RunStartProgram();
        }

        [Benchmark]
        public void RunCycles() => _simulator.ExecuteCycles(CycleCount);
    }
}