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
        [AsmData("Cpu/TestFiles/And.6502.asm")]
        public void And_Immediate(byte[] memory, IEnumerable<Checkpoint> checkpoints)
        {
            if (memory == null)
            {
                throw new ArgumentNullException(nameof(memory));
            }

            var testMemory = new byte[memory.Length];
            memory.CopyTo(testMemory, 0);

            void checkpointHit(byte[] memory, byte checkpointNumber)
            {
                var checkpoint = checkpoints.FirstOrDefault(x => x.Number == checkpointNumber);

                Assert.True(checkpoint.AssertExpectations(memory));
            }

            RunSimulation(memory, checkpointHit);
            RunEmulator(testMemory, checkpointHit);

            Assert.True(Enumerable.SequenceEqual(memory, testMemory));
        }

        protected void RunSimulation(byte[] memory, Action<byte[], byte> checkpointHit)
        {
            var simulator = new Simulator(memory);

            simulator.Init();
            simulator.RunStartProgram();

            for (var i = 0; i < 50_000; i++)
            {
                simulator.Clock();

                if (memory[0xfd01] != 0x00) // A checkpoint has been hit.
                {
                    checkpointHit(memory, memory[0xfd01]);

                    memory[0xfd01] = 0; // Reset the checkpoint so we don't hit it again on the next cycle.
                }

                if (memory[0xfd00] == 0xa3) // A value of 0xa3 at memory location 0xfd00 means the test is complete.
                {
                    break;
                }
            }
        }

        protected void RunEmulator(byte[] memory, Action<byte[], byte> checkpointHit)
        {
            var memoryBus = new EmulatorBus(memory);
            var cpu = new Emulator.Cpu(memoryBus);

            cpu.PowerOn();

            for (var i = 0; i < 50_000; i++)
            {
                cpu.Clock();

                if (memory[0xfd01] != 0x00) // A checkpoint has been hit.
                {
                    checkpointHit(memory, memory[0xfd01]);

                    memory[0xfd01] = 0; // Reset the checkpoint so we don't hit it again on the next cycle.
                }

                if (memory[0xfd00] == 0xa3)
                {
                    break;
                }
            }
        }
    }
}