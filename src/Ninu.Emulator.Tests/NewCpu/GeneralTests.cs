using Ninu.Base;
using Ninu.Emulator.Tests.Cpu;
using Ninu.Emulator.Tests.TestHeaders;
using Ninu.Visual6502;
using System;
using System.Collections.Generic;
using Xunit;

namespace Ninu.Emulator.Tests.NewCpu
{
    public class GeneralTests
    {
        [Theory]
        [AsmData("NewCpu/TestFiles/basic.6502.asm")]
        [AsmData("NewCpu/TestFiles/jmp-indirect.6502.asm")]
        [AsmData("NewCpu/TestFiles/jmp-indirect-buggy.6502.asm")]
        public void TestInstructions(byte[] memory, IEnumerable<Checkpoint> checkpoints)
        {
            if (memory is null) throw new ArgumentNullException(nameof(memory));
            if (checkpoints is null) throw new ArgumentNullException(nameof(checkpoints));

            var simulatorMemory = new TrackedMemory(memory);
            var emulatorMemory = new TrackedMemory(memory);

            var simulator = new Simulator(simulatorMemory);
            simulator.Init();

            var bus = new EmulatorBus(emulatorMemory);
            var cpu = new CentralProcessor.NewCpu(bus);

            cpu.Init();

            // Run the init programs.
            for (var i = 0; i < 9; i++)
            {
                cpu.Clock();
                simulator.Clock();
            }

            simulator.HalfClock(); // See notes in the simulation's start program code.

            // Run the actual user code.
            for (var i = 0; i < 500; i++)
            {
                cpu.Clock();
                simulator.Clock();

                Assert.True(TrackedMemory.AreChangesEqual(simulatorMemory, emulatorMemory));

                Assert.Equal(simulator.ReadPC(), cpu.CpuState.PC);
                Assert.Equal(simulator.ReadA(), cpu.CpuState.A);
                Assert.Equal(simulator.ReadX(), cpu.CpuState.X);
                Assert.Equal(simulator.ReadY(), cpu.CpuState.Y);
                Assert.Equal(simulator.ReadS(), cpu.CpuState.S);
                Assert.Equal(simulator.ReadP(), (int)cpu.CpuState.P);

                // When the test has completed successfully, it will write 0xa3 to memory location
                // 0xff00.
                if (simulatorMemory[0xff00] == 0xa3)
                {
                    break;
                }
                else if (simulatorMemory[0xff00] == 0xc9) // A value of 0xc9 means we took the wrong code path.
                {
                    throw new Exception("Hit an error.");
                }
            }
        }
    }
}