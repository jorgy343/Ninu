using Ninu.Base;
using Ninu.Emulator.CentralProcessor;
using Ninu.Visual6502;
using Patcher6502;
using System;
using System.IO;
using System.Text;
using Xunit;

namespace Ninu.Emulator.Tests.NewCpu
{
    public class GeneralTests
    {
        [Theory]
        [AsmData("NewCpu/TestFiles/basic.6502.asm")]
        [AsmData("NewCpu/TestFiles/arithmetic-and-comparison.6502.asm")]
        [AsmData("NewCpu/TestFiles/flags.6502.asm")]
        [AsmData("NewCpu/TestFiles/transfers.6502.asm")]
        [AsmData("NewCpu/TestFiles/loads.6502.asm")]
        [AsmData("NewCpu/TestFiles/stores.6502.asm")]
        [AsmData("NewCpu/TestFiles/stack.6502.asm")]
        [AsmData("NewCpu/TestFiles/branches.6502.asm")]
        [AsmData("NewCpu/TestFiles/jmp-indirect.6502.asm")]
        [AsmData("NewCpu/TestFiles/jmp-indirect-buggy.6502.asm")]
        public void TestInstructions(byte[] memory)
        {
            if (memory is null) throw new ArgumentNullException(nameof(memory));

            var simulatorMemory = new TrackedMemory(memory);
            var emulatorMemory = new TrackedMemory(memory);

            var simulator = new Simulator(simulatorMemory);
            simulator.Init();

            var bus = new EmulatorBus(emulatorMemory);
            var cpu = new CentralProcessor.Cpu(bus);

            var simulatorLog = new StringBuilder();
            var emulatorLog = new StringBuilder();

            cpu.Init();

            // Run the init programs.
            for (var i = 0; i < 9; i++)
            {
                cpu.Clock();
                simulator.Clock();
            }

            simulator.HalfClock(); // See notes in the simulation's start program code.

            // We only check the flags register the cycle after sync goes high. This variable
            // tracks the state of sync on the previous cycle.
            var previousSync = false;

            // Run the actual user code.
            for (var i = 0; i < 1000; i++)
            {
                cpu.Clock();
                simulator.Clock();

                WriteDataLine(simulatorLog, i + 1, simulator);
                WriteDataLine(emulatorLog, i + 1, cpu);

                Assert.True(TrackedMemory.AreChangesEqual(simulatorMemory, emulatorMemory));

                Assert.Equal(simulator.ReadPC(), cpu.CpuState.PC);
                Assert.Equal(simulator.ReadA(), cpu.CpuState.A);
                Assert.Equal(simulator.ReadX(), cpu.CpuState.X);
                Assert.Equal(simulator.ReadY(), cpu.CpuState.Y);

                // The S register reads funny during most of the execution of the JSR instruction.
                // Don't check S during execution of this operation.
                if (simulator.ReadBits8("ir") != (byte)Opcode.Jsr_Absolute)
                {
                    Assert.Equal(simulator.ReadS(), cpu.CpuState.S);
                }

                // Because flags are set on very weird cycles for reasons I don't yet understand,
                // we will only check flags once we know for sure they will be set.
                if (previousSync)
                {
                    Assert.Equal(simulator.ReadP(), (int)cpu.CpuState.P);
                }

                previousSync = simulator.ReadBit("sync") == 0 ? false : true;

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

            // Make sure that after the test is ran we have the done marker set. If this isn't set
            // then the test was probably not given enough cycles to complete or it did something
            // wrong.
            if (simulatorMemory[0xff00] != 0xa3)
            {
                throw new Exception("Did not hit done.");
            }
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        [InlineData(5)]
        [InlineData(6)]
        [InlineData(7)]
        [InlineData(8)]
        [InlineData(9)]
        [InlineData(10)]
        [InlineData(11)]
        //[InlineData(12)]
        public void TestNmi(int cycleToSetNmiLow)
        {
            var asm = File.ReadAllText("NewCpu/TestFiles/nmi.6502.asm");

            var assembler = new PatchAssembler();
            var memory = assembler.Assemble(0, null, asm);

            var simulatorMemory = new TrackedMemory(memory);
            var emulatorMemory = new TrackedMemory(memory);

            var simulator = new Simulator(simulatorMemory);
            simulator.Init();

            var bus = new EmulatorBus(emulatorMemory);
            var cpu = new CentralProcessor.Cpu(bus);

            var simulatorLog = new StringBuilder();
            var emulatorLog = new StringBuilder();

            cpu.Init();

            // Run the init programs.
            for (var i = 0; i < 9; i++)
            {
                cpu.Clock();
                simulator.Clock();
            }

            simulator.HalfClock(); // See notes in the simulation's start program code.

            // We only check the flags register the cycle after sync goes high. This variable
            // tracks the state of sync on the previous cycle.
            var previousSync = false;

            // Run the actual user code.
            for (var i = 0; i < 1000; i++)
            {
                if (i + 1 == cycleToSetNmiLow)
                {
                    cpu.Nmi = true;
                    simulator.WriteBit("nmi", false);
                }
                else
                {
                    // For the simulator, we will only pull NMI low for a single cycle. This is
                    // enough to set the NMI flipflop. For the emulator, the NMI flag represents
                    // the flipfop so we don't need to touch that as it will be reset automatically
                    // when the NMI routine finishes.
                    simulator.WriteBit("nmi", true);
                }

                cpu.Clock();
                simulator.Clock();

                WriteDataLine(simulatorLog, i + 1, simulator);
                WriteDataLine(emulatorLog, i + 1, cpu);

                Assert.True(TrackedMemory.AreChangesEqual(simulatorMemory, emulatorMemory));

                Assert.Equal(simulator.ReadPC(), cpu.CpuState.PC);
                Assert.Equal(simulator.ReadA(), cpu.CpuState.A);
                Assert.Equal(simulator.ReadX(), cpu.CpuState.X);
                Assert.Equal(simulator.ReadY(), cpu.CpuState.Y);

                // The S register reads funny during most of the execution of the JSR instruction.
                // Don't check S during execution of this operation.
                if (simulator.ReadBits8("ir") != (byte)Opcode.Jsr_Absolute)
                {
                    Assert.Equal(simulator.ReadS(), cpu.CpuState.S);
                }

                // Because flags are set on very weird cycles for reasons I don't yet understand,
                // we will only check flags once we know for sure they will be set.
                if (previousSync)
                {
                    Assert.Equal(simulator.ReadP(), (int)cpu.CpuState.P);
                }

                previousSync = simulator.ReadBit("sync") == 0 ? false : true;

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

            // Make sure that after the test is ran we have the done marker set. If this isn't set
            // then the test was probably not given enough cycles to complete or it did something
            // wrong.
            if (simulatorMemory[0xff00] != 0xa3)
            {
                throw new Exception("Did not hit done.");
            }
        }

        [Fact]
        public void KlausTest()
        {
            var data = File.ReadAllBytes(@"C:\Users\Jorgy\Downloads\6502_functional_test.bin");

            var simulatorMemory = new TrackedMemory(data);
            var emulatorMemory = new TrackedMemory(data);

            var simulator = new Simulator(simulatorMemory);
            simulator.Init();

            var bus = new EmulatorBus(emulatorMemory);
            var cpu = new CentralProcessor.Cpu(bus);

            //var simulatorLog = new StringBuilder();
            //var emulatorLog = new StringBuilder();

            using var simulatorLog = new StreamWriter(@"C:\Users\Jorgy\Desktop\simulatorLog.txt");
            using var emulatorLog = new StreamWriter(@"C:\Users\Jorgy\Desktop\emulatorLog.txt");

            cpu.Init();

            // Run the init programs.
            for (var i = 0; i < 9; i++)
            {
                cpu.Clock();
                simulator.Clock();
            }

            simulator.HalfClock(); // See notes in the simulation's start program code.

            // We only check the flags register the cycle after sync goes high. This variable
            // tracks the state of sync on the previous cycle.
            var previousSync = false;

            // Run the actual user code.
            for (var i = 0; i < 500_000; i++)
            {
                if (i == 109346)
                {

                }

                cpu.Clock();
                simulator.Clock();

                WriteDataLine(simulatorLog, i + 1, simulator);
                WriteDataLine(emulatorLog, i + 1, cpu);

                Assert.True(TrackedMemory.AreChangesEqual(simulatorMemory, emulatorMemory));

                Assert.Equal(simulator.ReadPC(), cpu.CpuState.PC);
                Assert.Equal(simulator.ReadA(), cpu.CpuState.A);
                Assert.Equal(simulator.ReadX(), cpu.CpuState.X);
                Assert.Equal(simulator.ReadY(), cpu.CpuState.Y);

                // The S register reads funny during most of the execution of the JSR instruction.
                // Don't check S during execution of this operation.
                if (simulator.ReadBits8("ir") != (byte)Opcode.Jsr_Absolute)
                {
                    Assert.Equal(simulator.ReadS(), cpu.CpuState.S);
                }

                // Because flags are set on very weird cycles for reasons I don't yet understand,
                // we will only check flags once we know for sure they will be set.
                if (previousSync)
                {
                    Assert.Equal(simulator.ReadP(), (int)cpu.CpuState.P);
                }

                previousSync = simulator.ReadBit("sync") == 0 ? false : true;

                if (i % 100 == 0)
                {
                    simulatorMemory.CommitChanges();
                    emulatorMemory.CommitChanges();
                }
            }

            //var simulatorLogString = simulatorLog.ToString();
            //var emulatorLogString = emulatorLog.ToString();
        }

        private void WriteDataLine(StringBuilder stringBuilder, int cycle, Simulator simulator)
        {
            stringBuilder.Append($"{cycle:00000} {simulator.ReadAddressBus():x4} {simulator.ReadBits8("db"):x2} {simulator.ReadPC():x4} ");
            stringBuilder.Append($"{simulator.ReadA():x2} {simulator.ReadX():x2} {simulator.ReadY():x2} {simulator.ReadS():x2} ");
            stringBuilder.Append($"{simulator.ReadBits8("ir"):x2}  {simulator.ReadBit("sync")}   {simulator.ReadBit("rw")}   ");
            stringBuilder.Append($"{simulator.ReadBit("nmi")}  {simulator.ReadPString()} ");

            stringBuilder.AppendLine();
        }

        private void WriteDataLine(StreamWriter stream, int cycle, Simulator simulator)
        {
            stream.Write($"{cycle:00000} {simulator.ReadAddressBus():x4} {simulator.ReadBits8("db"):x2} {simulator.ReadPC():x4} ");
            stream.Write($"{simulator.ReadA():x2} {simulator.ReadX():x2} {simulator.ReadY():x2} {simulator.ReadS():x2} ");
            stream.Write($"{simulator.ReadBits8("ir"):x2}  {simulator.ReadBit("sync")}   {simulator.ReadBit("rw")}   ");
            stream.Write($"{simulator.ReadBit("nmi")}  {simulator.ReadPString()} ");

            stream.WriteLine();
        }

        private void WriteDataLine(StringBuilder stringBuilder, int cycle, CentralProcessor.Cpu cpu)
        {
            stringBuilder.Append($"{cycle:00000} ---- -- {cpu.CpuState.PC:x4} ");
            stringBuilder.Append($"{cpu.CpuState.A:x2} {cpu.CpuState.X:x2} {cpu.CpuState.Y:x2} {cpu.CpuState.S:x2} ");
            stringBuilder.Append($"--  -   -   ");
            stringBuilder.Append($"{(cpu.Nmi ? "1" : "0")}  -------- ");

            stringBuilder.AppendLine();
        }

        private void WriteDataLine(StreamWriter stream, int cycle, CentralProcessor.Cpu cpu)
        {
            stream.Write($"{cycle:00000} ---- -- {cpu.CpuState.PC:x4} ");
            stream.Write($"{cpu.CpuState.A:x2} {cpu.CpuState.X:x2} {cpu.CpuState.Y:x2} {cpu.CpuState.S:x2} ");
            stream.Write($"--  -   -   ");
            stream.Write($"{(cpu.Nmi ? "1" : "0")}  -------- ");

            stream.WriteLine();
        }
    }
}