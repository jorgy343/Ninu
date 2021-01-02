﻿using Ninu.Emulator.CentralProcessor.Operations;
using System;
using System.Collections.Generic;
using static Ninu.Emulator.CentralProcessor.NewOpcode;

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

        // TODO: Saving this won't work out of the box.
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

            while (true)
            {
                // If there is nothing in the queue, we are probably in a jammed state. Do nothing
                // and break out of the infinite while loop.
                if (_operations.Count == 0)
                {
                    return;
                }

                var (operation, incrementPC) = _operations.Dequeue();

                // Increment the PC before it is actually used in the operation.
                if (incrementPC)
                {
                    CpuState.PC++;
                }

                operation.Execute(this, _cpuBus);

                // If the operation is not free, we are done processing for this clock cycle. If
                // the operation is free, we need to execute the next operation in the queue. If
                // there are no more operations in the queue, the next pass in the infinite while
                // loop will break out of the loop. Free operations do not increment the cycle.
                if (!operation.IsFree)
                {
                    break;
                }
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
            switch ((NewOpcode)opcode)
            {
                case Clc_Implied:
                    Implied(Clc);
                    break;

                case Cld_Implied:
                    Implied(Cld);
                    break;

                case Cli_Implied:
                    Implied(Cli);
                    break;

                case Clv_Implied:
                    Implied(Clv);
                    break;

                case Dex_Implied:
                    ImpliedDelayedExecution(Dex);
                    break;

                case Dey_Implied:
                    ImpliedDelayedExecution(Dey);
                    break;

                case Inx_Implied:
                    ImpliedDelayedExecution(Inx);
                    break;

                case Iny_Implied:
                    ImpliedDelayedExecution(Iny);
                    break;

                case Jmp_Absolute:
                    _operations.Enqueue((new FetchAddressLowByPC(), true));
                    _operations.Enqueue((new FetchAddressHighByPC(), true));
                    _operations.Enqueue((new FetchInstructionAndExecute(Jmp), false));
                    break;

                case Jmp_Indirect:
                    _operations.Enqueue((new FetchAddressLowByPC(), true));
                    _operations.Enqueue((new FetchAddressHighByPC(), true));
                    _operations.Enqueue((new FetchEffectiveAddressLow(), true)); // PC increment doesn't matter, but it does happen.
                    _operations.Enqueue((new FetchEffectiveAddressHigh(), false));
                    _operations.Enqueue((new FetchInstructionAndExecute(JmpIndirect), false));
                    break;

                case Lda_Immediate:
                    _operations.Enqueue((FetchMemoryByPCIntoDataLatch.Singleton, true));
                    _operations.Enqueue((new FetchInstructionAndExecute(Lda), true));
                    break;

                case Ldx_Immediate:
                    _operations.Enqueue((FetchMemoryByPCIntoDataLatch.Singleton, true));
                    _operations.Enqueue((new FetchInstructionAndExecute(Ldx), true));
                    break;

                case Ldy_Immediate:
                    _operations.Enqueue((FetchMemoryByPCIntoDataLatch.Singleton, true));
                    _operations.Enqueue((new FetchInstructionAndExecute(Ldy), true));
                    break;

                case Nop_Implied:
                    Implied();
                    break;

                case Sec_Implied:
                    Implied(Sec);
                    break;

                case Sed_Implied:
                    Implied(Sed);
                    break;

                case Sei_Implied:
                    Implied(Sei);
                    break;

                case Sta_Absolute:
                    _operations.Enqueue((new FetchAddressLowByPC(), true));
                    _operations.Enqueue((new FetchAddressHighByPC(), true));
                    _operations.Enqueue((new WriteAToAddressLatch(), true));
                    _operations.Enqueue((new FetchInstruction(), false));
                    break;

                case Tax_Implied:
                    Implied(Tax);
                    break;

                case Tay_Implied:
                    Implied(Tay);
                    break;

                case Tsx_Implied:
                    Implied(Tsx);
                    break;

                case Txa_Implied:
                    Implied(Txa);
                    break;

                case Txs_Implied:
                    Implied(Txs);
                    break;

                case Tya_Implied:
                    Implied(Tya);
                    break;

                default:
                    throw new NotImplementedException($"The instruction 0x{opcode:x2} is not implemented.");
            }
        }

        // Addressing Modes
        private void Implied()
        {
            _operations.Enqueue((Nop.Singleton, true));
            _operations.Enqueue((new FetchInstruction(), false));
        }

        private void Implied(Action action)
        {
            _operations.Enqueue((Nop.Singleton, true));
            _operations.Enqueue((new FetchInstructionAndExecute(action), false));
        }

        /// <summary>
        /// Same as <see cref="Implied"/> except that the instruction execution happens during the
        /// first clock cycle of the next instruction. This is acomplished by inserting a free
        /// action execution operation in the queue after the instruction fetch operation.
        /// </summary>
        /// <param name="action">The instruction operation to be executed.</param>
        private void ImpliedDelayedExecution(Action action)
        {
            _operations.Enqueue((Nop.Singleton, true));
            _operations.Enqueue((new FetchInstruction(), false));
            _operations.Enqueue((new ExecuteForFree(action), false));
        }

        // Instructions
        private void Clc()
        {
            CpuState.SetFlag(CpuFlags.C, false);
        }

        private void Cld()
        {
            CpuState.SetFlag(CpuFlags.D, false);
        }

        private void Cli()
        {
            CpuState.SetFlag(CpuFlags.I, false);
        }

        private void Clv()
        {
            CpuState.SetFlag(CpuFlags.V, false);
        }

        private void Dex()
        {
            CpuState.X--;

            CpuState.SetZeroFlag(CpuState.X);
            CpuState.SetNegativeFlag(CpuState.X);
        }

        private void Dey()
        {
            CpuState.Y--;

            CpuState.SetZeroFlag(CpuState.Y);
            CpuState.SetNegativeFlag(CpuState.Y);
        }

        private void Inx()
        {
            CpuState.X++;

            CpuState.SetZeroFlag(CpuState.X);
            CpuState.SetNegativeFlag(CpuState.X);
        }

        private void Iny()
        {
            CpuState.Y++;

            CpuState.SetZeroFlag(CpuState.Y);
            CpuState.SetNegativeFlag(CpuState.Y);
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

        private void Sec()
        {
            CpuState.SetFlag(CpuFlags.C, true);
        }

        private void Sed()
        {
            CpuState.SetFlag(CpuFlags.D, true);
        }

        private void Sei()
        {
            CpuState.SetFlag(CpuFlags.I, true);
        }

        private void Tax()
        {
            CpuState.X = CpuState.A;

            CpuState.SetZeroFlag(CpuState.X);
            CpuState.SetNegativeFlag(CpuState.X);
        }

        private void Tay()
        {
            CpuState.Y = CpuState.A;

            CpuState.SetZeroFlag(CpuState.Y);
            CpuState.SetNegativeFlag(CpuState.Y);
        }

        private void Tsx()
        {
            CpuState.X = CpuState.S;

            CpuState.SetZeroFlag(CpuState.X);
            CpuState.SetNegativeFlag(CpuState.X);
        }

        private void Txa()
        {
            CpuState.A = CpuState.X;

            CpuState.SetZeroFlag(CpuState.A);
            CpuState.SetNegativeFlag(CpuState.A);
        }

        private void Txs()
        {
            CpuState.S = CpuState.X;
        }

        private void Tya()
        {
            CpuState.A = CpuState.Y;

            CpuState.SetZeroFlag(CpuState.A);
            CpuState.SetNegativeFlag(CpuState.A);
        }
    }
}