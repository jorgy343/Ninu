using Ninu.Emulator.CentralProcessor.Operations;
using System;
using System.Collections.Generic;

namespace Ninu.Emulator.CentralProcessor
{
    public class NewCpu
    {
        private readonly IBus _cpuBus;

        [Save("TotalCycles")]
        private long _totalCycles;

        [SaveChildren]
        public CpuState CpuState { get; } = new();

        [Save]
        public byte DataLatch { get; set; }

        [Save]
        public byte AddressLatchLow { get; set; }

        [Save]
        public byte AddressLatchHigh { get; set; }

        [Save]
        public byte EffectiveAddressLatchLow { get; set; }

        [Save]
        public byte EffectiveAddressLatchHigh { get; set; }

        // TODO: This is going to be an allocation mess.
        [Save("Operations")]
        private readonly Queue<(CpuOperation Operation, bool IncrementPC)> _operations = new(16);

        public NewCpu(IBus cpuBus)
        {
            _cpuBus = cpuBus ?? throw new ArgumentNullException(nameof(cpuBus));
        }

        public void Clock()
        {
            // Increment first so that the first cycle is considered 1.
            _totalCycles++;

            // If there is nothing in the queue, we are probably in a jammed state. Do nothing.
            if (_operations.Count > 0)
            {
                var (operation, incrementPC) = _operations.Dequeue();

                // Increment the PC before it is actually used in the operation.
                if (incrementPC)
                {
                    CpuState.PC++;
                }

                operation.Execute(this, _cpuBus);
            }
        }

        /// <summary>
        /// Setups up the CPU execution stack to perform the initialization sequence which takes 9
        /// cycles according to Visual 6502.
        /// </summary>
        public void Init()
        {
            // During the init routine, the stack is eventually set to 0xfd. The flags are also set
            // to a specific value.
            CpuState.S = 0xfd;
            CpuState.P = 0x00 | CpuFlags.I | CpuFlags.Z; // Setting the Z flag because visual 6502 seems to do it.

            // Visual 6502 appears to set the other registers to these specific values. In the real
            // world, these would probably be random due to noise. For testing purposes, let's just
            // match what Visual 6502 does.
            CpuState.A = 0xaa;
            CpuState.X = 0x00;
            CpuState.Y = 0x00;

            // The first seven cycles do stuff in the actual CPU. We don't care about those
            // details, we just need to take up some cycles.
            _operations.Enqueue((Nop.Singleton, false));
            _operations.Enqueue((Nop.Singleton, false));
            _operations.Enqueue((Nop.Singleton, false));
            _operations.Enqueue((Nop.Singleton, false));
            _operations.Enqueue((Nop.Singleton, false));
            _operations.Enqueue((Nop.Singleton, false));
            _operations.Enqueue((Nop.Singleton, false)); // This should technically load the low byte of the reset vector.

            // Cycles 7 and 8 (or just 8 because we are lazy) loads the reset vector into PC.
            _operations.Enqueue((new LoadResetVector(), false));

            // Cycle 9 loads the instruction found at PC and gets it ready for execution.
            _operations.Enqueue((new FetchInstruction(), false));
        }

        public void ExecuteInstruction(byte opcode)
        {
            switch (opcode)
            {
                // JMP absolute
                case 0x4c:
                    _operations.Enqueue((new FetchAddressLowByPC(), true));
                    _operations.Enqueue((new FetchAddressHighByPC(), true));
                    _operations.Enqueue((new FetchInstructionAndExecute(Jmp), false));
                    break;

                // JMP indirect
                case 0x6c:
                    _operations.Enqueue((new FetchAddressLowByPC(), true));
                    _operations.Enqueue((new FetchAddressHighByPC(), true));
                    _operations.Enqueue((new FetchEffectiveAddressLow(), true)); // PC increment doesn't matter, but it does happen.
                    _operations.Enqueue((new FetchEffectiveAddressHigh(), false));
                    _operations.Enqueue((new FetchInstructionAndExecute(JmpIndirect), false));
                    break;

                // STA absolute
                case 0x8d:
                    _operations.Enqueue((new FetchAddressLowByPC(), true));
                    _operations.Enqueue((new FetchAddressHighByPC(), true));
                    _operations.Enqueue((new WriteAToAddressLatch(), true));
                    _operations.Enqueue((new FetchInstruction(), false));
                    break;

                // LDY immediate
                case 0xa0:
                    _operations.Enqueue((new FetchDataByPC(), true));
                    _operations.Enqueue((new FetchInstructionAndExecute(Ldy), true));
                    break;

                // LDX immediate
                case 0xa2:
                    _operations.Enqueue((new FetchDataByPC(), true));
                    _operations.Enqueue((new FetchInstructionAndExecute(Ldx), true));
                    break;

                // LDA immediate
                case 0xa9:
                    _operations.Enqueue((new FetchDataByPC(), true));
                    _operations.Enqueue((new FetchInstructionAndExecute(Lda), true));
                    break;

                default:
                    throw new NotImplementedException($"The instruction 0x{opcode:x2} is not implemented.");
            }
        }

        private void Jmp()
        {
            CpuState.PC = (ushort)(AddressLatchLow | (AddressLatchHigh << 8));
        }

        private void JmpIndirect()
        {
            CpuState.PC = (ushort)(EffectiveAddressLatchLow | (EffectiveAddressLatchHigh << 8));
        }

        private void Lda()
        {
            CpuState.A = DataLatch;

            CpuState.SetZeroFlag(CpuState.A);
            CpuState.SetNegativeFlag(CpuState.A);
        }

        private void Ldx()
        {
            CpuState.X = DataLatch;

            CpuState.SetZeroFlag(CpuState.X);
            CpuState.SetNegativeFlag(CpuState.X);
        }

        private void Ldy()
        {
            CpuState.Y = DataLatch;

            CpuState.SetZeroFlag(CpuState.Y);
            CpuState.SetNegativeFlag(CpuState.Y);
        }
    }
}