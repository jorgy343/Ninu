using System;
using System.Collections.Generic;

namespace Ninu.Emulator
{
    public class Cpu
    {
        private readonly IBus _cpuBus;

        [Save("TotalCycles")]
        private long _totalCycles;

        [Save("RemainingCycles")]
        private int _remainingCycles;

        [Save]
        public bool Nmi{ get; set; }

        [SaveChildren]
        public CpuState CpuState { get; } = new CpuState();

        public Cpu(IBus cpuBus)
        {
            _cpuBus = cpuBus ?? throw new ArgumentNullException(nameof(cpuBus));
        }

        public void Clock()
        {
            if (_remainingCycles == 0) // Read the next instruction when the previous is done executing.
            {
                // Check for an NMI signal before processing the next instruction. When an NMI is triggered, it will
                // always occur after the currently executing instruction.
                if (Nmi)
                {
                    Nmi = false;

                    PerformNmi();
                }
                else
                {
                    var opCode = _cpuBus.Read(CpuState.PC++);
                    var instruction = Instruction.GetInstruction(opCode);

                    var cycles = instruction.Execute(_cpuBus, CpuState);

                    _remainingCycles = cycles;
                }
            }

            _totalCycles++;
            _remainingCycles--;
        }

        public string DecodeInstruction(ushort address)
        {
            var opCode = _cpuBus.Read(address);
            var instruction = Instruction.GetInstruction(opCode);

            return instruction.Name;
        }

        public IEnumerable<string> DecodeInstructions(ushort address, int count)
        {
            for (var i = 0; i < count; i++)
            {
                var opCode = _cpuBus.Read(address);
                var instruction = Instruction.GetInstruction(opCode);

                yield return instruction.Name;

                address += (ushort)instruction.Size;
            }
        }

        public void PowerOn()
        {
            CpuState.A = 0;
            CpuState.X = 0;
            CpuState.Y = 0;

            CpuState.S = 0xfd;
            CpuState.P = 0x00 | CpuFlags.U | CpuFlags.I;

            var pcLow = (ushort)_cpuBus.Read(0xfffc);
            var pcHigh = (ushort)_cpuBus.Read(0xfffd);

            CpuState.PC = (ushort)(pcLow | (pcHigh << 8));

            _remainingCycles = 8;
        }

        public void Reset()
        {
            CpuState.S -= 3;
            CpuState.P |= CpuFlags.I;

            var pcLow = (ushort)_cpuBus.Read(0xfffc);
            var pcHigh = (ushort)_cpuBus.Read(0xfffd);

            CpuState.PC = (ushort)(pcLow | (pcHigh << 8));

            _remainingCycles = 6; // TODO: Pretty sure this is supposed to be 6.
        }

        public void Interrupt()
        {
            if (!CpuState.GetFlag(CpuFlags.I))
            {
                // Push the PC onto the stack.
                Push((byte)((CpuState.PC >> 8) & 0x00ff)); // High byte first so that when the PC is popped the low byte will come first.
                Push((byte)(CpuState.PC & 0x00ff));

                // Set some flags before we push the status register.
                CpuState.SetFlag(CpuFlags.B, false);
                CpuState.SetFlag(CpuFlags.U, true);
                CpuState.SetFlag(CpuFlags.I, true);

                Push((byte)CpuState.P);

                // Read the interrupt address at 0xfffe.
                var pcLow = (ushort)_cpuBus.Read(0xfffe);
                var pcHigh = (ushort)_cpuBus.Read(0xffff);

                CpuState.PC = (ushort)(pcLow | (pcHigh << 8));

                _remainingCycles = 7;
            }
        }

        private void PerformNmi()
        {
            // Push the PC onto the stack.
            Push((byte)((CpuState.PC >> 8) & 0x00ff)); // High byte first so that when the PC is popped the low byte will come first.
            Push((byte)(CpuState.PC & 0x00ff));

            Push((byte)CpuState.P);

            // Set some flags before we push the status register.
            CpuState.SetFlag(CpuFlags.B, false);
            CpuState.SetFlag(CpuFlags.U, true);
            CpuState.SetFlag(CpuFlags.I, true);

            // Read the interrupt address at 0xfffa.
            var pcLow = (ushort)_cpuBus.Read(0xfffa);
            var pcHigh = (ushort)_cpuBus.Read(0xfffb);

            CpuState.PC = (ushort)(pcLow | (pcHigh << 8));

            _remainingCycles = 8;
        }

        public void Push(byte data)
        {
            _cpuBus.Write((ushort)(0x0100 + CpuState.S), data);
            CpuState.S--;
        }

        public byte Pop()
        {
            CpuState.S++;
            return _cpuBus.Read((ushort)(0x0100 + CpuState.S));
        }
    }
}