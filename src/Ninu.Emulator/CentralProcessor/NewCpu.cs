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
                AddOperation(PushPCHighOnStack.Singleton, false);

                // Store the low byte of the PC onto the stack (0x100 + S - 1) but do not touch S.
                AddOperation(PushPCLowOnStack.Singleton, false);

                // Store the status register onto the stack (0x100 + S - 2) but do not touch S.
                void PushPOnStack()
                {
                    var p = (byte)((byte)CpuState.P | 0x20); // Push the program status flags with B = 0 and U = 1.
                    _bus.Write((ushort)(0x100 + CpuState.S - 2), p);
                }

                AddOperation(Nop.Singleton, false, PushPOnStack);

                // Fetch the low byte of the interrupt vector address and decrement the stack by 3.
                void DecrementStackBy3()
                {
                    CpuState.S -= 3;
                }

                AddOperation(FetchNmiVectorLowIntoAddressLatchLow.Singleton, false, DecrementStackBy3);

                // Fetch the high byte of the interrupt vector address.
                AddOperation(FetchNmiVectorHighIntoAddressLatchHigh.Singleton, false);

                // Set PC to the address latch and fetch the instruction found at PC.
                void SetNmiToFalse()
                {
                    // Set NMI to false to allow an NMI to occur again.
                    Nmi = false;
                }

                AddOperation(SetPCToAddressLatchAndFetchInstruction.Singleton, false, SetNmiToFalse);
            }
        }

        public void ExecuteInstruction(byte opcode)
        {
            switch ((NewOpcode)opcode)
            {
                case Clc_Implied:
                    Addr_Implied(Op_Clc);
                    break;

                case Cld_Implied:
                    Addr_Implied(Op_Cld);
                    break;

                case Cli_Implied:
                    Addr_Implied(Op_Cli);
                    break;

                case Clv_Implied:
                    Addr_Implied(Op_Clv);
                    break;

                case Dex_Implied:
                    Addr_ImpliedDelayedExecution(Op_Dex);
                    break;

                case Dey_Implied:
                    Addr_ImpliedDelayedExecution(Op_Dey);
                    break;

                case Inx_Implied:
                    Addr_ImpliedDelayedExecution(Op_Inx);
                    break;

                case Iny_Implied:
                    Addr_ImpliedDelayedExecution(Op_Iny);
                    break;

                case Jmp_Absolute:
                    AddOperation(FetchMemoryByPCIntoEffectiveAddressLatchLow.Singleton, true);
                    AddOperation(FetchMemoryByPCIntoEffectiveAddressLatchHigh.Singleton, true);
                    AddOperation(FetchInstruction.Singleton, false, Op_Jmp);
                    break;

                case Jmp_Indirect:
                    AddOperation(FetchMemoryByPCIntoAddressLatchLow.Singleton, true);
                    AddOperation(FetchMemoryByPCIntoAddressLatchHigh.Singleton, true);
                    AddOperation(FetchMemoryByAddressLatchIntoEffectiveAddressLatchLow.Singleton, true); // PC increment doesn't matter, but it does happen.
                    AddOperation(FetchMemoryByAddressLatchIntoEffectiveAddressLatchHighWithBug.Singleton, false);
                    AddOperation(FetchInstruction.Singleton, false, Op_Jmp);
                    break;

                case Lda_Absolute:
                    Addr_Absolute(Op_Lda);
                    break;

                case Lda_AbsoluteWithXOffset:
                    throw new NotImplementedException($"The instruction 0x{opcode:x2} is not implemented.");

                case Lda_AbsoluteWithYOffset:
                    throw new NotImplementedException($"The instruction 0x{opcode:x2} is not implemented.");

                case Lda_Immediate:
                    Addr_Immediate(Op_Lda);
                    break;

                case Lda_IndirectZeroPageWithXOffset:
                    throw new NotImplementedException($"The instruction 0x{opcode:x2} is not implemented.");

                case Lda_IndirectZeroPageWithYOffset:
                    throw new NotImplementedException($"The instruction 0x{opcode:x2} is not implemented.");

                case Lda_ZeroPage:
                    Addr_ZeroPage(Op_Lda);
                    break;

                case Lda_ZeroPageWithXOffset:
                    throw new NotImplementedException($"The instruction 0x{opcode:x2} is not implemented.");

                case Ldx_Absolute:
                    Addr_Absolute(Op_Ldx);
                    break;

                case Ldx_AbsoluteWithYOffset:
                    throw new NotImplementedException($"The instruction 0x{opcode:x2} is not implemented.");

                case Ldx_Immediate:
                    Addr_Immediate(Op_Ldx);
                    break;

                case Ldx_ZeroPage:
                    Addr_ZeroPage(Op_Ldx);
                    break;

                case Ldx_ZeroPageWithYOffset:
                    throw new NotImplementedException($"The instruction 0x{opcode:x2} is not implemented.");

                case Ldy_Absolute:
                    Addr_Absolute(Op_Ldy);
                    break;

                case Ldy_AbsoluteWithXOffset:
                    throw new NotImplementedException($"The instruction 0x{opcode:x2} is not implemented.");

                case Ldy_Immediate:
                    Addr_Immediate(Op_Ldy);
                    break;

                case Ldy_ZeroPage:
                    Addr_ZeroPage(Op_Ldy);
                    break;

                case Ldy_ZeroPageWithXOffset:
                    throw new NotImplementedException($"The instruction 0x{opcode:x2} is not implemented.");

                case Nop_Implied:
                    Addr_Implied();
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

                        EffectiveAddressLatchLow = _bus.Read((ushort)(0x100 + CpuState.S + 2));
                    }

                    AddOperation(Nop.Singleton, false, PullPCLowFromStack);

                    // Pull PC high from the stack. Increment S by 3.
                    void PullPCHighFromStack()
                    {
                        EffectiveAddressLatchHigh = _bus.Read((ushort)(0x100 + CpuState.S + 3));
                        CpuState.S += 3;
                    }

                    AddOperation(Nop.Singleton, false, PullPCHighFromStack);

                    // Load PC and fetch the next instruction.
                    void SetPCAndFetchInstruction()
                    {
                        CpuState.PC = (ushort)(EffectiveAddressLatchLow | (EffectiveAddressLatchHigh << 8));

                        var instruction = _bus.Read(CpuState.PC);
                        ExecuteInstruction(instruction);
                    }

                    AddOperation(Nop.Singleton, false, SetPCAndFetchInstruction);

                    break;

                case Sec_Implied:
                    Addr_Implied(Op_Sec);
                    break;

                case Sed_Implied:
                    Addr_Implied(Op_Sed);
                    break;

                case Sei_Implied:
                    Addr_Implied(Op_Sei);
                    break;

                case Sta_Absolute:
                    AddOperation(FetchMemoryByPCIntoEffectiveAddressLatchLow.Singleton, true);
                    AddOperation(FetchMemoryByPCIntoEffectiveAddressLatchHigh.Singleton, true);
                    AddOperation(WriteAToMemoryByEffectiveAddressLatch.Singleton, true);
                    AddOperation(FetchInstruction.Singleton, false);
                    break;

                case Sta_AbsoluteWithXOffset:
                    AddOperation(FetchMemoryByPCIntoEffectiveAddressLatchLow.Singleton, true);
                    AddOperation(FetchMemoryByPCIntoEffectiveAddressLatchHigh.Singleton, true);
                    AddOperation(IncrementEffectiveAddressLatchLowByXWithoutWrapping.Singleton, true);
                    AddOperation(WriteAToMemoryByEffectiveAddressLatch.Singleton, false);
                    AddOperation(FetchInstruction.Singleton, false);
                    break;

                case Sta_AbsoluteWithYOffset:
                    AddOperation(FetchMemoryByPCIntoEffectiveAddressLatchLow.Singleton, true);
                    AddOperation(FetchMemoryByPCIntoEffectiveAddressLatchHigh.Singleton, true);
                    AddOperation(IncrementEffectiveAddressLatchLowByYWithoutWrapping.Singleton, true);
                    AddOperation(WriteAToMemoryByEffectiveAddressLatch.Singleton, false);
                    AddOperation(FetchInstruction.Singleton, false);
                    break;

                case Sta_IndirectZeroPageWithXOffset:
                    AddOperation(FetchZeroPageAddressByPCIntoAddressLatch.Singleton, true);
                    AddOperation(IncrementAddressLatchLowByXWithWrapping.Singleton, true);
                    AddOperation(FetchMemoryByAddressLatchIntoEffectiveAddressLatchLow.Singleton, false);
                    AddOperation(FetchMemoryByAddressLatchIntoEffectiveAddressLatchHighWithBug.Singleton, false);
                    AddOperation(WriteAToMemoryByEffectiveAddressLatch.Singleton, false);
                    AddOperation(FetchInstruction.Singleton, false);
                    break;

                case Sta_IndirectZeroPageWithYOffset:
                    AddOperation(FetchZeroPageAddressByPCIntoAddressLatch.Singleton, true);
                    AddOperation(FetchMemoryByAddressLatchIntoEffectiveAddressLatchLow.Singleton, true);
                    AddOperation(FetchMemoryByAddressLatchIntoEffectiveAddressLatchHighWithBug.Singleton, false);
                    AddOperation(IncrementEffectiveAddressLatchLowByYWithoutWrapping.Singleton, false);
                    AddOperation(WriteAToMemoryByEffectiveAddressLatch.Singleton, false);
                    AddOperation(FetchInstruction.Singleton, false);
                    break;

                case Sta_ZeroPage:
                    AddOperation(FetchZeroPageAddressByPCIntoEffectiveAddressLatch.Singleton, true);
                    AddOperation(WriteAToMemoryByEffectiveAddressLatch.Singleton, true);
                    AddOperation(FetchInstruction.Singleton, false);
                    break;

                case Sta_ZeroPageWithXOffset:
                    AddOperation(FetchZeroPageAddressByPCIntoEffectiveAddressLatch.Singleton, true);
                    AddOperation(IncrementEffectiveAddressLatchLowByXWithWrapping.Singleton, true);
                    AddOperation(WriteAToMemoryByEffectiveAddressLatch.Singleton, false);
                    AddOperation(FetchInstruction.Singleton, false);
                    break;

                case Stx_Absolute:
                    AddOperation(FetchMemoryByPCIntoEffectiveAddressLatchLow.Singleton, true);
                    AddOperation(FetchMemoryByPCIntoEffectiveAddressLatchHigh.Singleton, true);
                    AddOperation(WriteXToMemoryByEffectiveAddressLatch.Singleton, true);
                    AddOperation(FetchInstruction.Singleton, false);
                    break;

                case Stx_ZeroPage:
                    AddOperation(FetchZeroPageAddressByPCIntoEffectiveAddressLatch.Singleton, true);
                    AddOperation(WriteXToMemoryByEffectiveAddressLatch.Singleton, true);
                    AddOperation(FetchInstruction.Singleton, false);
                    break;

                case Stx_ZeroPageWithYOffset:
                    AddOperation(FetchZeroPageAddressByPCIntoEffectiveAddressLatch.Singleton, true);
                    AddOperation(IncrementEffectiveAddressLatchLowByYWithWrapping.Singleton, true);
                    AddOperation(WriteXToMemoryByEffectiveAddressLatch.Singleton, false);
                    AddOperation(FetchInstruction.Singleton, false);
                    break;

                case Sty_Absolute:
                    AddOperation(FetchMemoryByPCIntoEffectiveAddressLatchLow.Singleton, true);
                    AddOperation(FetchMemoryByPCIntoEffectiveAddressLatchHigh.Singleton, true);
                    AddOperation(WriteYToMemoryByEffectiveAddressLatch.Singleton, true);
                    AddOperation(FetchInstruction.Singleton, false);
                    break;

                case Sty_ZeroPage:
                    AddOperation(FetchZeroPageAddressByPCIntoEffectiveAddressLatch.Singleton, true);
                    AddOperation(WriteYToMemoryByEffectiveAddressLatch.Singleton, true);
                    AddOperation(FetchInstruction.Singleton, false);
                    break;

                case Sty_ZeroPageWithXOffset:
                    AddOperation(FetchZeroPageAddressByPCIntoEffectiveAddressLatch.Singleton, true);
                    AddOperation(IncrementEffectiveAddressLatchLowByXWithWrapping.Singleton, true);
                    AddOperation(WriteYToMemoryByEffectiveAddressLatch.Singleton, false);
                    AddOperation(FetchInstruction.Singleton, false);
                    break;

                case Tax_Implied:
                    Addr_Implied(Op_Tax);
                    break;

                case Tay_Implied:
                    Addr_Implied(Op_Tay);
                    break;

                case Tsx_Implied:
                    Addr_Implied(Op_Tsx);
                    break;

                case Txa_Implied:
                    Addr_Implied(Op_Txa);
                    break;

                case Txs_Implied:
                    Addr_Implied(Op_Txs);
                    break;

                case Tya_Implied:
                    Addr_Implied(Op_Tya);
                    break;

                case Adc_Absolute:
                case Adc_AbsoluteWithXOffset:
                case Adc_AbsoluteWithYOffset:
                case Adc_Immediate:
                case Adc_IndirectZeroPageWithXOffset:
                case Adc_IndirectZeroPageWithYOffset:
                case Adc_ZeroPage:
                case Adc_ZeroPageWithXOffset:
                case Ahx_AbsoluteWithYOffset_9F:
                case Ahx_IndirectZeroPageWithYOffset_93:
                case Alr_Immediate_4B:
                case Anc_Immediate_0B:
                case Anc_Immediate_2B:
                case And_Absolute:
                case And_AbsoluteWithXOffset:
                case And_AbsoluteWithYOffset:
                case And_Immediate:
                case And_IndirectZeroPageWithXOffset:
                case And_IndirectZeroPageWithYOffset:
                case And_ZeroPage:
                case And_ZeroPageWithXOffset:
                case Arr_Immediate_6B:
                case Asl_Absolute:
                case Asl_AbsoluteWithXOffset:
                case Asl_Accumulator:
                case Asl_ZeroPage:
                case Asl_ZeroPageWithXOffset:
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
                case Cmp_AbsoluteWithXOffset:
                case Cmp_AbsoluteWithYOffset:
                case Cmp_Immediate:
                case Cmp_IndirectZeroPageWithXOffset:
                case Cmp_IndirectZeroPageWithYOffset:
                case Cmp_ZeroPage:
                case Cmp_ZeroPageWithXOffset:
                case Cpx_Absolute:
                case Cpx_Immediate:
                case Cpx_ZeroPage:
                case Cpy_Absolute:
                case Cpy_Immediate:
                case Cpy_ZeroPage:
                case Dcp_Absolute_CF:
                case Dcp_AbsoluteWithXOffset_DF:
                case Dcp_AbsoluteWithYOffset_DB:
                case Dcp_IndirectZeroPageWithXOffset_C3:
                case Dcp_IndirectZeroPageWithYOffset_D3:
                case Dcp_ZeroPage_C7:
                case Dcp_ZeroPageWithXOffset_D7:
                case Dec_Absolute:
                case Dec_AbsoluteWithXOffset:
                case Dec_ZeroPage:
                case Dec_ZeroPageWithXOffset:
                case Eor_Absolute:
                case Eor_AbsoluteWithXOffset:
                case Eor_AbsoluteWithYOffset:
                case Eor_Immediate:
                case Eor_IndirectZeroPageWithXOffset:
                case Eor_IndirectZeroPageWithYOffset:
                case Eor_ZeroPage:
                case Eor_ZeroPageWithXOffset:
                case Inc_Absolute:
                case Inc_AbsoluteWithXOffset:
                case Inc_ZeroPage:
                case Inc_ZeroPageWithXOffset:
                case Isc_Absolute_EF:
                case Isc_AbsoluteWithXOffset_FF:
                case Isc_AbsoluteWithYOffset_FB:
                case Isc_IndirectZeroPageWithXOffset_E3:
                case Isc_IndirectZeroPageWithYOffset_F3:
                case Isc_ZeroPage_E7:
                case Isc_ZeroPageWithXOffset_F7:
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
                case Las_AbsoluteWithYOffset_BB:
                case Lax_Absolute_AF:
                case Lax_AbsoluteWithYOffset_BF:
                case Lax_Immediate_AB:
                case Lax_IndirectZeroPageWithXOffset_A3:
                case Lax_IndirectZeroPageWithYOffset_B3:
                case Lax_ZeroPage_A7:
                case Lax_ZeroPageWithYOffset_B7:
                case Lsr_Absolute:
                case Lsr_AbsoluteWithXOffset:
                case Lsr_Accumulator:
                case Lsr_ZeroPage:
                case Lsr_ZeroPageWithXOffset:
                case Nop_Absolute_0C:
                case Nop_AbsoluteWithXOffset_1C:
                case Nop_AbsoluteWithXOffset_3C:
                case Nop_AbsoluteWithXOffset_5C:
                case Nop_AbsoluteWithXOffset_7C:
                case Nop_AbsoluteWithXOffset_DC:
                case Nop_AbsoluteWithXOffset_FC:
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
                case Nop_ZeroPageWithXOffset_14:
                case Nop_ZeroPageWithXOffset_34:
                case Nop_ZeroPageWithXOffset_54:
                case Nop_ZeroPageWithXOffset_74:
                case Nop_ZeroPageWithXOffset_D4:
                case Nop_ZeroPageWithXOffset_F4:
                case Ora_Absolute:
                case Ora_AbsoluteWithXOffset:
                case Ora_AbsoluteWithYOffset:
                case Ora_Immediate:
                case Ora_IndirectZeroPageWithXOffset:
                case Ora_IndirectZeroPageWithYOffset:
                case Ora_ZeroPage:
                case Ora_ZeroPageWithXOffset:
                case Pha_Implied:
                case Php_Implied:
                case Pla_Implied:
                case Plp_Implied:
                case Rla_Absolute_2F:
                case Rla_AbsoluteWithXOffset_3F:
                case Rla_AbsoluteWithYOffset_3B:
                case Rla_IndirectZeroPageWithXOffset_23:
                case Rla_IndirectZeroPageWithYOffset_33:
                case Rla_ZeroPage_27:
                case Rla_ZeroPageWithXOffset_37:
                case Rol_Absolute:
                case Rol_AbsoluteWithXOffset:
                case Rol_Accumulator:
                case Rol_ZeroPage:
                case Rol_ZeroPageWithXOffset:
                case Ror_Absolute:
                case Ror_AbsoluteWithXOffset:
                case Ror_Accumulator:
                case Ror_ZeroPage:
                case Ror_ZeroPageWithXOffset:
                case Rra_Absolute_6F:
                case Rra_AbsoluteWithXOffset_7F:
                case Rra_AbsoluteWithYOffset_7B:
                case Rra_IndirectZeroPageWithXOffset_63:
                case Rra_IndirectZeroPageWithYOffset_73:
                case Rra_ZeroPage_67:
                case Rra_ZeroPageWithXOffset_77:
                case Rts_Implied:
                case Sax_Absolute_8F:
                case Sax_IndirectZeroPageWithXOffset_83:
                case Sax_ZeroPage_87:
                case Sax_ZeroPageWithYOffset_97:
                case Sbc_Absolute:
                case Sbc_AbsoluteWithXOffset:
                case Sbc_AbsoluteWithYOffset:
                case Sbc_Immediate:
                case Sbc_Immediate_EB:
                case Sbc_IndirectZeroPageWithXOffset:
                case Sbc_IndirectZeroPageWithYOffset:
                case Sbc_ZeroPage:
                case Sbc_ZeroPageWithXOffset:
                case Shx_AbsoluteWithYOffset_9E:
                case Shy_AbsoluteWithXOffset_9C:
                case Slo_Absolute_0F:
                case Slo_AbsoluteWithXOffset_1F:
                case Slo_AbsoluteWithYOffset_1B:
                case Slo_IndirectZeroPageWithXOffset_03:
                case Slo_IndirectZeroPageWithYOffset_13:
                case Slo_ZeroPage_07:
                case Slo_ZeroPageWithXOffset_17:
                case Sre_Absolute_4F:
                case Sre_AbsoluteWithXOffset_5F:
                case Sre_AbsoluteWithYOffset_5B:
                case Sre_IndirectZeroPageWithXOffset_43:
                case Sre_IndirectZeroPageWithYOffset_53:
                case Sre_ZeroPage_47:
                case Sre_ZeroPageWithXOffset_57:
                case Tas_AbsoluteWithYOffset_9B:
                case Xaa_Immediate_8B:
                default:
                    throw new NotImplementedException($"The instruction 0x{opcode:x2} is not implemented.");
            }
        }

        // Addressing Modes
        private void Addr_Implied()
        {
            AddOperation(Nop.Singleton, true);
            AddOperation(FetchInstruction.Singleton, false);
        }

        private void Addr_Implied(Action action)
        {
            AddOperation(Nop.Singleton, true);
            AddOperation(FetchInstruction.Singleton, false, action);
        }

        /// <summary>
        /// Same as <see cref="Addr_Implied"/> except that the instruction execution happens during the
        /// first clock cycle of the next instruction. This is acomplished by inserting a free
        /// action execution operation in the queue after the instruction fetch operation. The free
        /// action will be the first thing executed on the next clock cycle.
        /// </summary>
        /// <param name="action">The instruction operation to be executed.</param>
        private void Addr_ImpliedDelayedExecution(Action action)
        {
            AddOperation(Nop.Singleton, true);
            AddOperation(FetchInstruction.Singleton, false);
            AddFreeOperation(Nop.Singleton, false, action);
        }

        private void Addr_Immediate(Action action)
        {
            AddOperation(FetchMemoryByPCIntoDataLatch.Singleton, true);
            AddOperation(FetchInstruction.Singleton, true, action);
        }

        private void Addr_Absolute(Action action)
        {
            AddOperation(FetchMemoryByPCIntoEffectiveAddressLatchLow.Singleton, true);
            AddOperation(FetchMemoryByPCIntoEffectiveAddressLatchHigh.Singleton, true);
            AddOperation(FetchMemoryByEffectiveAddressLatchIntoDataLatch.Singleton, true);
            AddOperation(FetchInstruction.Singleton, false, action);
        }

        private void Addr_ZeroPage(Action action)
        {
            AddOperation(FetchZeroPageAddressByPCIntoEffectiveAddressLatch.Singleton, true);
            AddOperation(FetchMemoryByEffectiveAddressLatchIntoDataLatch.Singleton, true);
            AddOperation(FetchInstruction.Singleton, false, action);
        }

        // Instructions
        private void Op_Clc()
        {
            CpuState.SetFlag(CpuFlags.C, false);
        }

        private void Op_Cld()
        {
            CpuState.SetFlag(CpuFlags.D, false);
        }

        private void Op_Cli()
        {
            CpuState.SetFlag(CpuFlags.I, false);
        }

        private void Op_Clv()
        {
            CpuState.SetFlag(CpuFlags.V, false);
        }

        private void Op_Dex()
        {
            CpuState.X--;

            CpuState.SetZeroFlag(CpuState.X);
            CpuState.SetNegativeFlag(CpuState.X);
        }

        private void Op_Dey()
        {
            CpuState.Y--;

            CpuState.SetZeroFlag(CpuState.Y);
            CpuState.SetNegativeFlag(CpuState.Y);
        }

        private void Op_Inx()
        {
            CpuState.X++;

            CpuState.SetZeroFlag(CpuState.X);
            CpuState.SetNegativeFlag(CpuState.X);
        }

        private void Op_Iny()
        {
            CpuState.Y++;

            CpuState.SetZeroFlag(CpuState.Y);
            CpuState.SetNegativeFlag(CpuState.Y);
        }

        private void Op_Jmp()
        {
            CpuState.PC = (ushort)(EffectiveAddressLatchLow | (EffectiveAddressLatchHigh << 8));
        }

        private void Op_Lda()
        {
            CpuState.A = DataLatch;

            CpuState.SetZeroFlag(CpuState.A);
            CpuState.SetNegativeFlag(CpuState.A);
        }

        private void Op_Ldx()
        {
            CpuState.X = DataLatch;

            CpuState.SetZeroFlag(CpuState.X);
            CpuState.SetNegativeFlag(CpuState.X);
        }

        private void Op_Ldy()
        {
            CpuState.Y = DataLatch;

            CpuState.SetZeroFlag(CpuState.Y);
            CpuState.SetNegativeFlag(CpuState.Y);
        }

        private void Op_Sec()
        {
            CpuState.SetFlag(CpuFlags.C, true);
        }

        private void Op_Sed()
        {
            CpuState.SetFlag(CpuFlags.D, true);
        }

        private void Op_Sei()
        {
            CpuState.SetFlag(CpuFlags.I, true);
        }

        private void Op_Tax()
        {
            CpuState.X = CpuState.A;

            CpuState.SetZeroFlag(CpuState.X);
            CpuState.SetNegativeFlag(CpuState.X);
        }

        private void Op_Tay()
        {
            CpuState.Y = CpuState.A;

            CpuState.SetZeroFlag(CpuState.Y);
            CpuState.SetNegativeFlag(CpuState.Y);
        }

        private void Op_Tsx()
        {
            CpuState.X = CpuState.S;

            CpuState.SetZeroFlag(CpuState.X);
            CpuState.SetNegativeFlag(CpuState.X);
        }

        private void Op_Txa()
        {
            CpuState.A = CpuState.X;

            CpuState.SetZeroFlag(CpuState.A);
            CpuState.SetNegativeFlag(CpuState.A);
        }

        private void Op_Txs()
        {
            CpuState.S = CpuState.X;
        }

        private void Op_Tya()
        {
            CpuState.A = CpuState.Y;

            CpuState.SetZeroFlag(CpuState.A);
            CpuState.SetNegativeFlag(CpuState.A);
        }
    }
}