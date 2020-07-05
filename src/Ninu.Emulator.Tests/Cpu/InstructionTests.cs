using Ninu.Visual6502;
using System;
using Xunit;

namespace Ninu.Emulator.Tests.Cpu
{
    public class InstructionTests
    {
        [Theory]
        [AsmData("Cpu/TestFiles/And.6502.asm")]
        public void And_Immediate(byte[] memory)
        {
            if (memory is null)
            {
                throw new ArgumentNullException(nameof(memory));
            }

            var testMemory = new byte[memory.Length];
            memory.CopyTo(testMemory, 0);

            RunSimulation(memory);
            RunEmulator(testMemory);

            Assert.Equal(0xa3, testMemory[0xfd00]);

            for (var i = 0; i < 256; i++)
            {
                Assert.Equal(0x00, testMemory[0xa000 + i]);
                Assert.Equal(i, testMemory[0xa100 + i]);
            }

            Assert.Equal(memory, testMemory);
        }

        protected void RunSimulation(byte[] memory)
        {
            var simulator = new Simulator(memory);

            simulator.Init();
            simulator.RunStartProgram();

            for (var i = 0; i < 50_000; i++)
            {
                simulator.Clock();

                if (memory[0xfd00] == 0xa3)
                {
                    break;
                }
            }
        }

        protected void RunEmulator(byte[] memory)
        {
            var memoryBus = new EmulatorBus(memory);
            var cpu = new Emulator.Cpu(memoryBus);

            cpu.PowerOn();

            for (var i = 0; i < 50_000; i++)
            {
                cpu.Clock();

                if (memory[0xfd00] == 0xa3)
                {
                    break;
                }
            }
        }
    }
}