using System;
using System.Collections.Generic;
using System.Text;

namespace Ninu.Emulator
{
    public class Cpu
    {
        public StringBuilder Log { get; } = new StringBuilder();

        private readonly IBus _cpuBus;

        [Save("TotalCycles")]
        private long _totalCycles;

        [Save("RemainingCycles")]
        private int _remainingCycles;

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
                var originalPc = CpuState.PC;

                var opCode = _cpuBus.Read(CpuState.PC++);
                var instruction = Instruction.GetInstruction(opCode);

                //var machineCode = instruction.Size switch
                //{
                //    1 => $"{opCode:X2}      ",
                //    2 => $"{opCode:X2} {_cpuBus.Read(CpuState.PC):X2}   ",
                //    3 => $"{opCode:X2} {_cpuBus.Read(CpuState.PC):X2} {_cpuBus.Read((ushort)(CpuState.PC + 1)):X2}",
                //    _ => throw new InvalidOperationException($"Unexpected instruction size of {instruction.Size} was found."),
                //};

                //Log.Append($"{originalPc:X4}  {machineCode}  {instruction.Name.ToUpperInvariant()}  A:{CpuState.A:X2} X:{CpuState.X:X2} Y:{CpuState.Y:X2} P:{(byte)CpuState.P:X2} SP:{CpuState.S:X2} CYC:{_totalCycles - 1}");
                //Log.AppendLine();

                var cycles = instruction.Execute(_cpuBus, CpuState);

                _remainingCycles = cycles;
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

        public void Reset()
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

        public void NonMaskableInterrupt()
        {
            // Push the PC onto the stack.
            Push((byte)((CpuState.PC >> 8) & 0x00ff)); // High byte first so that when the PC is popped the low byte will come first.
            Push((byte)(CpuState.PC & 0x00ff));

            // Set some flags before we push the status register.
            CpuState.SetFlag(CpuFlags.B, false);
            CpuState.SetFlag(CpuFlags.U, true);
            CpuState.SetFlag(CpuFlags.I, true);

            Push((byte)CpuState.P);

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