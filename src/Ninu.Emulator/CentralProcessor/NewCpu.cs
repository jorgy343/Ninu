using Ninu.Emulator.CentralProcessor.Operations;
using Ninu.Emulator.CentralProcessor.Operations.Interrupts;
using System;
using System.Collections.Generic;
using static Ninu.Emulator.CentralProcessor.NewOpcode;

namespace Ninu.Emulator.CentralProcessor
{
    public class NewCpu
    {
        private readonly IBus _bus;

        [Save("TotalCycles")]
        public long _totalCycles;

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
        private readonly Queue<NewCpuOperationQueueState> _queue = new(24);

        [Save]
        public bool _nmi;

        [Save]
        public long _nmiCycle;

        /// <summary>
        /// When set to <c>true</c>, the CPU will enter the NMI routine as soon as the current
        /// instruction is done executing. Note that in the real CPU the NMI line is held low which
        /// causes a flipflop in the CPU to be set which indicates an NMI needs to be triggered.
        /// This property is analogous to the flipflop, not the NMI line which is why we set this
        /// property to <c>true</c> to incidate an NMI.
        /// </summary>
        public bool Nmi
        {
            get => _nmi;
            set
            {
                if (_nmi && value)
                {
                    // Don't set NMI to true if it is already true.
                    return;
                }

                _nmi = value;
                _nmiCycle = value ? _totalCycles : -1;
            }
        }

        public NewCpu(IBus bus)
        {
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
        }

        private void AddOperation(CpuOperation operation, bool incrementPC, Action? action = null)
        {
            _queue.Enqueue(new NewCpuOperationQueueState(operation, action, incrementPC, false));
        }

        private void AddFreeOperation(CpuOperation operation, bool incrementPC, Action? action = null)
        {
            _queue.Enqueue(new NewCpuOperationQueueState(operation, action, incrementPC, true));
        }

        public void Clock()
        {
            // Increment first so that the first cycle is considered 1.
            _totalCycles++;

            while (true)
            {
                // If there is nothing in the queue, we are probably in a jammed state. Do nothing
                // and break out of the infinite while loop.
                if (_queue.Count == 0)
                {
                    return;
                }

                var queueState = _queue.Dequeue();

                // Increment the PC before it is actually used in the operation.
                if (queueState.IncrementPC)
                {
                    CpuState.PC++;
                }

                queueState.Action?.Invoke();

                queueState.Operation.Execute(this, _bus);

                // If the operation is not free, we are done processing for this clock cycle. If
                // the operation is free, we need to execute the next operation in the queue. If
                // there are no more operations in the queue, the next pass in the infinite while
                // loop will break out of the loop. Free operations do not increment the cycle.
                if (!queueState.Free)
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
            AddOperation(Nop.Singleton, false);
            AddOperation(Nop.Singleton, false);
            AddOperation(Nop.Singleton, false);
            AddOperation(Nop.Singleton, false);
            AddOperation(Nop.Singleton, false);
            AddOperation(Nop.Singleton, false);

            // Cycles 7 and 8 load the reset vector into the address latch.
            AddOperation(FetchResetVectorLowIntoAddressLatchLow.Singleton, false);
            AddOperation(FetchResetVectorHighIntoAddressLatchHigh.Singleton, false);

            // Cycle 9 sets PC to the address latch and then loads the instruction found at PC and
            // gets it ready for execution. This is typically the last cycle of an instruction and
            // here it is technically the last cycle of the modified BRK instruction that is being
            // executed.
            AddOperation(SetPCToAddressLatchAndFetchInstruction.Singleton, false);
        }

        public void CheckForNmi()
        {
            if (Nmi)
            {
                // TODO: When/if do we set P.I?

                // First step is a NOP. The real CPU does some data access that gets thrown away.
                AddOperation(Nop.Singleton, false);

                // Store the high byte of the PC onto the stack (0x100 + S) but do not touch S.
                void PushPCHighOnStack()
                {
                    var pcHigh = (byte)(CpuState.PC >> 8);
                    _bus.Write((ushort)(0x100 + CpuState.S), pcHigh);
                }

                AddOperation(Nop.Singleton, false, PushPCHighOnStack);

                // Store the low byte of the PC onto the stack (0x100 + S - 1) but do not touch S.
                void PushPCLowOnStack()
                {
                    var pcLow = (byte)(CpuState.PC & 0x00ff);
                    _bus.Write((ushort)(0x100 + CpuState.S - 1), pcLow);
                }

                AddOperation(Nop.Singleton, false, PushPCLowOnStack);

                // Store the status register onto the stack (0x100 + S - 2) but do not touch S.
                void PushPOnStack()
                {
                    var p = (byte)((byte)CpuState.P | 0x20); // Push the program state with B = 0 and U = 1.
                    _bus.Write((ushort)(0x100 + CpuState.S - 2), p);
                }

                AddOperation(Nop.Singleton, false, PushPOnStack);

                // Fetch the low byte of the interrupt vector address and decrement the stack by 3.
                void FetchLowInterruptVectorAddress()
                {
                    AddressLatchLow = _bus.Read(0xfffa);
                    CpuState.S -= 3;
                }

                AddOperation(Nop.Singleton, false, FetchLowInterruptVectorAddress);

                // Fetch the high byte of the interrupt vector address.
                void FetchHighInterruptVectorAddress()
                {
                    AddressLatchHigh = _bus.Read(0xfffb);
                }

                AddOperation(Nop.Singleton, false, FetchHighInterruptVectorAddress);

                // Set PC to the address latch and fetch the instruction found at PC.
                void SetPCAndFetchInstruction()
                {
                    CpuState.PC = (ushort)(AddressLatchLow | (AddressLatchHigh << 8));

                    var instruction = _bus.Read(CpuState.PC);
                    ExecuteInstruction(instruction);

                    // Set NMI to false to allow an NMI to occur again.
                    Nmi = false;
                }

                AddOperation(Nop.Singleton, false, SetPCAndFetchInstruction);
            }
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
                    AddOperation(FetchMemoryByPCIntoAddressLatchLow.Singleton, true);
                    AddOperation(FetchMemoryByPCIntoAddressLatchHigh.Singleton, true);
                    AddOperation(FetchInstruction.Singleton, false, Jmp);
                    break;

                case Jmp_Indirect:
                    AddOperation(FetchMemoryByPCIntoAddressLatchLow.Singleton, true);
                    AddOperation(FetchMemoryByPCIntoAddressLatchHigh.Singleton, true);
                    AddOperation(FetchMemoryByAddressLatchIntoEffectiveAddressLatchLow.Singleton, true); // PC increment doesn't matter, but it does happen.
                    AddOperation(FetchMemoryByAddressLatchIntoEffectiveAddressLatchHighWithBug.Singleton, false);
                    AddOperation(FetchInstruction.Singleton, false, JmpIndirect);
                    break;

                case Lda_Immediate:
                    AddOperation(FetchMemoryByPCIntoDataLatch.Singleton, true);
                    AddOperation(FetchInstruction.Singleton, true, Lda);
                    break;

                case Ldx_Immediate:
                    AddOperation(FetchMemoryByPCIntoDataLatch.Singleton, true);
                    AddOperation(FetchInstruction.Singleton, true, Ldx);
                    break;

                case Ldy_Immediate:
                    AddOperation(FetchMemoryByPCIntoDataLatch.Singleton, true);
                    AddOperation(FetchInstruction.Singleton, true, Ldy);
                    break;

                case Nop_Implied:
                    Implied();
                    break;

                case Rti_Implied:
                    // TODO: Implement better?

                    // The PC increments are inconsequential but do happen.
                    AddOperation(Nop.Singleton, true);
                    AddOperation(Nop.Singleton, true);

                    // Pull P from stack and store it in the data latch but don't set P yet.
                    void PullPFromStack()
                    {
                        DataLatch = _bus.Read((ushort)(0x100 + CpuState.S + 1));

                        // During interrupts, bits 4 and 5 of the status register may be set in the
                        // stack. Make sure to clear these when pulling P from the stack as these
                        // two bits don't actually exist.
                        DataLatch = (byte)(DataLatch & ~0x30);
                    }

                    AddOperation(Nop.Singleton, false, PullPFromStack);

                    // Pull PC low from the stack. Set P to the data latch.
                    void PullPCLowFromStack()
                    {
                        CpuState.P = (CpuFlags)DataLatch;

                        AddressLatchLow = _bus.Read((ushort)(0x100 + CpuState.S + 2));
                    }

                    AddOperation(Nop.Singleton, false, PullPCLowFromStack);

                    // Pull PC high from the stack. Increment S by 3.
                    void PullPCHighFromStack()
                    {
                        AddressLatchHigh = _bus.Read((ushort)(0x100 + CpuState.S + 3));
                        CpuState.S += 3;
                    }

                    AddOperation(Nop.Singleton, false, PullPCHighFromStack);

                    // Load PC and fetch the next instruction.
                    void SetPCAndFetchInstruction()
                    {
                        CpuState.PC = (ushort)(AddressLatchLow | (AddressLatchHigh << 8));

                        var instruction = _bus.Read(CpuState.PC);
                        ExecuteInstruction(instruction);
                    }

                    AddOperation(Nop.Singleton, false, SetPCAndFetchInstruction);

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
                    AddOperation(FetchMemoryByPCIntoAddressLatchLow.Singleton, true);
                    AddOperation(FetchMemoryByPCIntoAddressLatchHigh.Singleton, true);
                    AddOperation(WriteAToMemoryByAddressLatch.Singleton, true);
                    AddOperation(FetchInstruction.Singleton, false);
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

                case Adc_Absolute:
                case Adc_AbsoluteXIndexed:
                case Adc_AbsoluteYIndexed:
                case Adc_Immediate:
                case Adc_IndirectZeroPageXIndexed:
                case Adc_IndirectZeroPageYIndexed:
                case Adc_ZeroPage:
                case Adc_ZeroPageXIndexed:
                case Ahx_AbsoluteYIndexed_9F:
                case Ahx_IndirectZeroPageYIndexed_93:
                case Alr_Immediate_4B:
                case Anc_Immediate_0B:
                case Anc_Immediate_2B:
                case And_Absolute:
                case And_AbsoluteXIndexed:
                case And_AbsoluteYIndexed:
                case And_Immediate:
                case And_IndirectZeroPageXIndexed:
                case And_IndirectZeroPageYIndexed:
                case And_ZeroPage:
                case And_ZeroPageXIndexed:
                case Arr_Immediate_6B:
                case Asl_Absolute:
                case Asl_AbsoluteXIndexed:
                case Asl_Accumulator:
                case Asl_ZeroPage:
                case Asl_ZeroPageXIndexed:
                case Axs_Immediate_CB:
                case Bcc_Relative:
                case Bcs_Relative:
                case Beq_Relative:
                case Bit_Absolute:
                case Bit_ZeroPage:
                case Bmi_Relative:
                case Bne_Relative:
                case Bpl_Relative:
                case Brk_Implied:
                case Bvc_Relative:
                case Bvs_Relative:
                case Cmp_Absolute:
                case Cmp_AbsoluteXIndexed:
                case Cmp_AbsoluteYIndexed:
                case Cmp_Immediate:
                case Cmp_IndirectZeroPageXIndexed:
                case Cmp_IndirectZeroPageYIndexed:
                case Cmp_ZeroPage:
                case Cmp_ZeroPageXIndexed:
                case Cpx_Absolute:
                case Cpx_Immediate:
                case Cpx_ZeroPage:
                case Cpy_Absolute:
                case Cpy_Immediate:
                case Cpy_ZeroPage:
                case Dcp_Absolute_CF:
                case Dcp_AbsoluteXIndexed_DF:
                case Dcp_AbsoluteYIndexed_DB:
                case Dcp_IndirectZeroPageXIndexed_C3:
                case Dcp_IndirectZeroPageYIndexed_D3:
                case Dcp_ZeroPage_C7:
                case Dcp_ZeroPageXIndexed_D7:
                case Dec_Absolute:
                case Dec_AbsoluteXIndexed:
                case Dec_ZeroPage:
                case Dec_ZeroPageXIndexed:
                case Eor_Absolute:
                case Eor_AbsoluteXIndexed:
                case Eor_AbsoluteYIndexed:
                case Eor_Immediate:
                case Eor_IndirectZeroPageXIndexed:
                case Eor_IndirectZeroPageYIndexed:
                case Eor_ZeroPage:
                case Eor_ZeroPageXIndexed:
                case Inc_Absolute:
                case Inc_AbsoluteXIndexed:
                case Inc_ZeroPage:
                case Inc_ZeroPageXIndexed:
                case Isc_Absolute_EF:
                case Isc_AbsoluteXIndexed_FF:
                case Isc_AbsoluteYIndexed_FB:
                case Isc_IndirectZeroPageXIndexed_E3:
                case Isc_IndirectZeroPageYIndexed_F3:
                case Isc_ZeroPage_E7:
                case Isc_ZeroPageXIndexed_F7:
                case Jsr_Absolute:
                case Kil_Implied_02:
                case Kil_Implied_12:
                case Kil_Implied_22:
                case Kil_Implied_32:
                case Kil_Implied_42:
                case Kil_Implied_52:
                case Kil_Implied_62:
                case Kil_Implied_72:
                case Kil_Implied_92:
                case Kil_Implied_B2:
                case Kil_Implied_D2:
                case Kil_Implied_F2:
                case Las_AbsoluteYIndexed_BB:
                case Lax_Absolute_AF:
                case Lax_AbsoluteYIndexed_BF:
                case Lax_Immediate_AB:
                case Lax_IndirectZeroPageXIndexed_A3:
                case Lax_IndirectZeroPageYIndexed_B3:
                case Lax_ZeroPage_A7:
                case Lax_ZeroPageYIndexed_B7:
                case Lda_Absolute:
                case Lda_AbsoluteXIndexed:
                case Lda_AbsoluteYIndexed:
                case Lda_IndirectZeroPageXIndexed:
                case Lda_IndirectZeroPageYIndexed:
                case Lda_ZeroPage:
                case Lda_ZeroPageXIndexed:
                case Ldx_Absolute:
                case Ldx_AbsoluteYIndexed:
                case Ldx_ZeroPage:
                case Ldx_ZeroPageYIndexed:
                case Ldy_Absolute:
                case Ldy_AbsoluteXIndexed:
                case Ldy_ZeroPage:
                case Ldy_ZeroPageXIndexed:
                case Lsr_Absolute:
                case Lsr_AbsoluteXIndexed:
                case Lsr_Accumulator:
                case Lsr_ZeroPage:
                case Lsr_ZeroPageXIndexed:
                case Nop_Absolute_0C:
                case Nop_AbsoluteXIndexed_1C:
                case Nop_AbsoluteXIndexed_3C:
                case Nop_AbsoluteXIndexed_5C:
                case Nop_AbsoluteXIndexed_7C:
                case Nop_AbsoluteXIndexed_DC:
                case Nop_AbsoluteXIndexed_FC:
                case Nop_Immediate_80:
                case Nop_Immediate_82:
                case Nop_Immediate_89:
                case Nop_Immediate_C2:
                case Nop_Immediate_E2:
                case Nop_Implied_1A:
                case Nop_Implied_3A:
                case Nop_Implied_5A:
                case Nop_Implied_7A:
                case Nop_Implied_DA:
                case Nop_Implied_FA:
                case Nop_ZeroPage_04:
                case Nop_ZeroPage_44:
                case Nop_ZeroPage_64:
                case Nop_ZeroPageXIndexed_14:
                case Nop_ZeroPageXIndexed_34:
                case Nop_ZeroPageXIndexed_54:
                case Nop_ZeroPageXIndexed_74:
                case Nop_ZeroPageXIndexed_D4:
                case Nop_ZeroPageXIndexed_F4:
                case Ora_Absolute:
                case Ora_AbsoluteXIndexed:
                case Ora_AbsoluteYIndexed:
                case Ora_Immediate:
                case Ora_IndirectZeroPageXIndexed:
                case Ora_IndirectZeroPageYIndexed:
                case Ora_ZeroPage:
                case Ora_ZeroPageXIndexed:
                case Pha_Implied:
                case Php_Implied:
                case Pla_Implied:
                case Plp_Implied:
                case Rla_Absolute_2F:
                case Rla_AbsoluteXIndexed_3F:
                case Rla_AbsoluteYIndexed_3B:
                case Rla_IndirectZeroPageXIndexed_23:
                case Rla_IndirectZeroPageYIndexed_33:
                case Rla_ZeroPage_27:
                case Rla_ZeroPageXIndexed_37:
                case Rol_Absolute:
                case Rol_AbsoluteXIndexed:
                case Rol_Accumulator:
                case Rol_ZeroPage:
                case Rol_ZeroPageXIndexed:
                case Ror_Absolute:
                case Ror_AbsoluteXIndexed:
                case Ror_Accumulator:
                case Ror_ZeroPage:
                case Ror_ZeroPageXIndexed:
                case Rra_Absolute_6F:
                case Rra_AbsoluteXIndexed_7F:
                case Rra_AbsoluteYIndexed_7B:
                case Rra_IndirectZeroPageXIndexed_63:
                case Rra_IndirectZeroPageYIndexed_73:
                case Rra_ZeroPage_67:
                case Rra_ZeroPageXIndexed_77:
                case Rts_Implied:
                case Sax_Absolute_8F:
                case Sax_IndirectZeroPageXIndexed_83:
                case Sax_ZeroPage_87:
                case Sax_ZeroPageYIndexed_97:
                case Sbc_Absolute:
                case Sbc_AbsoluteXIndexed:
                case Sbc_AbsoluteYIndexed:
                case Sbc_Immediate:
                case Sbc_Immediate_EB:
                case Sbc_IndirectZeroPageXIndexed:
                case Sbc_IndirectZeroPageYIndexed:
                case Sbc_ZeroPage:
                case Sbc_ZeroPageXIndexed:
                case Shx_AbsoluteYIndexed_9E:
                case Shy_AbsoluteXIndexed_9C:
                case Slo_Absolute_0F:
                case Slo_AbsoluteXIndexed_1F:
                case Slo_AbsoluteYIndexed_1B:
                case Slo_IndirectZeroPageXIndexed_03:
                case Slo_IndirectZeroPageYIndexed_13:
                case Slo_ZeroPage_07:
                case Slo_ZeroPageXIndexed_17:
                case Sre_Absolute_4F:
                case Sre_AbsoluteXIndexed_5F:
                case Sre_AbsoluteYIndexed_5B:
                case Sre_IndirectZeroPageXIndexed_43:
                case Sre_IndirectZeroPageYIndexed_53:
                case Sre_ZeroPage_47:
                case Sre_ZeroPageXIndexed_57:
                case Sta_AbsoluteXIndexed:
                case Sta_AbsoluteYIndexed:
                case Sta_IndirectZeroPageXIndexed:
                case Sta_IndirectZeroPageYIndexed:
                case Sta_ZeroPage:
                case Sta_ZeroPageXIndexed:
                case Stx_Absolute:
                case Stx_ZeroPage:
                case Stx_ZeroPageYIndexed:
                case Sty_Absolute:
                case Sty_ZeroPage:
                case Sty_ZeroPageXIndexed:
                case Tas_AbsoluteYIndexed_9B:
                case Xaa_Immediate_8B:
                default:
                    throw new NotImplementedException($"The instruction 0x{opcode:x2} is not implemented.");
            }
        }

        // Addressing Modes
        private void Implied()
        {
            AddOperation(Nop.Singleton, true);
            AddOperation(FetchInstruction.Singleton, false);
        }

        private void Implied(Action action)
        {
            AddOperation(Nop.Singleton, true);
            AddOperation(FetchInstruction.Singleton, false, action);
        }

        /// <summary>
        /// Same as <see cref="Implied"/> except that the instruction execution happens during the
        /// first clock cycle of the next instruction. This is acomplished by inserting a free
        /// action execution operation in the queue after the instruction fetch operation. The free
        /// action will be the first thing executed on the next clock cycle.
        /// </summary>
        /// <param name="action">The instruction operation to be executed.</param>
        private void ImpliedDelayedExecution(Action action)
        {
            AddOperation(Nop.Singleton, true);
            AddOperation(FetchInstruction.Singleton, false);
            AddFreeOperation(Nop.Singleton, false, action);
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