using Ninu.Emulator.CentralProcessor;
using Ninu.Emulator.Tests.TestHeaders;
using Ninu.Visual6502;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Ninu.Emulator.Tests.Cpu
{
    public class InstructionTests
    {
        [Theory]
        [AsmData("Cpu/TestFiles/and.6502.asm")]
        public void And_Immediate(byte[] memory, IEnumerable<Checkpoint> checkpoints)
        {
            if (memory is null)
            {
                throw new ArgumentNullException(nameof(memory));
            }

            if (checkpoints is null)
            {
                throw new ArgumentNullException(nameof(checkpoints));
            }

            var testMemory = new byte[memory.Length];
            memory.CopyTo(testMemory, 0);

            void checkpointHit(byte checkpointNumber, byte[] memory, CpuFlags flags, byte a, byte x, byte y)
            {
                var checkpoint = checkpoints.FirstOrDefault(x => x.Identifier == checkpointNumber);

                Assert.NotNull(checkpoint);
                Assert.True(checkpoint!.AssertExpectations(memory, flags, a, x, y));
            }

            RunSimulation(memory, checkpointHit);
            RunEmulator(testMemory, checkpointHit);

            Assert.True(Enumerable.SequenceEqual(memory, testMemory));
        }

        [Theory]
        [AsmData("Cpu/TestFiles/and.6502.asm")]
        public void TestInstructions(byte[] memory, IEnumerable<Checkpoint> checkpoints)
        {
            if (memory is null)
            {
                throw new ArgumentNullException(nameof(memory));
            }

            if (checkpoints is null)
            {
                throw new ArgumentNullException(nameof(checkpoints));
            }

            var testMemory = new byte[memory.Length];
            memory.CopyTo(testMemory, 0);

            var simulator = new Simulator(memory);

            simulator.Init();
            simulator.RunStartProgram();

            var memoryBus = new EmulatorBus(memory);
            var cpu = new CentralProcessor.Cpu(memoryBus);

            cpu.PowerOn();

            for (var i = 0; i < 10_000; i++)
            {
                if (cpu.RemainingCycles == 0)
                {
                    // Do the comparisons.
                    Assert.Equal(memory, testMemory);
                }
                else
                {
                    cpu.Clock();
                    simulator.Clock();
                }

                if (testMemory[0xff00] == 0xa3) // A value of 0xa3 at memory location 0xff00 means the test is complete.
                {
                    break;
                }
            }
        }

        protected void RunSimulation(byte[] memory, Action<byte, byte[], CpuFlags, byte, byte, byte> checkpointHit)
        {
            var simulator = new Simulator(memory);

            simulator.Init();
            simulator.RunStartProgram();

            for (var i = 0; i < 10_000; i++)
            {
                simulator.Clock();

                if (memory[0xff01] != 0x00) // A checkpoint has been hit.
                {
                    checkpointHit(
                        memory[0xff01],
                        memory,
                        (CpuFlags)memory[0x0100 + simulator.ReadS() + 2],
                        memory[0x0100 + simulator.ReadS() + 1],
                        (byte)simulator.ReadX(),
                        (byte)simulator.ReadY());

                    memory[0xff01] = 0; // Reset the checkpoint so we don't hit it again on the next cycle.
                }

                if (memory[0xff00] == 0xa3) // A value of 0xa3 at memory location 0xff00 means the test is complete.
                {
                    break;
                }
            }
        }

        protected void RunEmulator(byte[] memory, Action<byte, byte[], CpuFlags, byte, byte, byte> checkpointHit)
        {
            var memoryBus = new EmulatorBus(memory);
            var cpu = new CentralProcessor.Cpu(memoryBus);

            cpu.PowerOn();

            for (var i = 0; i < 10_000; i++)
            {
                cpu.Clock();

                if (memory[0xff01] != 0x00) // A checkpoint has been hit.
                {
                    checkpointHit(
                        memory[0xff01],
                        memory,
                        (CpuFlags)memory[0x0100 + cpu.CpuState.S + 2],
                        memory[0x0100 + cpu.CpuState.S + 1],
                        cpu.CpuState.X,
                        cpu.CpuState.Y);

                    memory[0xff01] = 0; // Reset the checkpoint so we don't hit it again on the next cycle.
                }

                if (memory[0xff00] == 0xa3) // A value of 0xa3 at memory location 0xff00 means the test is complete.
                {
                    break;
                }
            }
        }
    }
}