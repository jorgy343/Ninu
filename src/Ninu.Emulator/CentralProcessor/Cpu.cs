using Ninu.Emulator.CentralProcessor.Operations;
using Ninu.Emulator.CentralProcessor.Operations.Interrupts;
using System;
using System.Collections.Generic;
using static Ninu.Emulator.CentralProcessor.Opcode;

namespace Ninu.Emulator.CentralProcessor
{
    public unsafe partial class Cpu
    {
        private readonly IBus _bus;

        [Save("TotalCycles")]
        public long _totalCycles;

        [SaveChildren]
        public CpuState CpuState { get; } = new();

        [Save]
        internal byte DataLatch { get; set; }

        [Save]
        internal byte AddressLatchLow { get; set; }

        [Save]
        internal byte AddressLatchHigh { get; set; }

        [Save]
        internal byte EffectiveAddressLatchLow { get; set; }

        [Save]
        internal byte EffectiveAddressLatchHigh { get; set; }

        // TODO: Saving this won't work out of the box.
        [Save("Operations")]
        internal Queue<CpuOperationQueueState> Queue = new(24);

        [Save]
        internal bool _nmi;

        [Save]
        internal long _nmiCycle;

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

        public Cpu(IBus bus)
        {
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
        }

        private void AddOperation(CpuOperation operation, bool incrementPC, delegate*<Cpu, IBus, void> preAction = null, delegate*<Cpu, IBus, void> postAction = null)
        {
            Queue.Enqueue(new CpuOperationQueueState(operation, preAction, postAction, incrementPC, false));
        }

        private void AddFreeOperation(CpuOperation operation, bool incrementPC, delegate*<Cpu, IBus, void> preAction = null, delegate*<Cpu, IBus, void> postAction = null)
        {
            Queue.Enqueue(new CpuOperationQueueState(operation, preAction, postAction, incrementPC, true));
        }

        public void Clock()
        {
            // Increment first so that the first cycle is considered 1.
            _totalCycles++;

            while (true)
            {
                // If there is nothing in the queue, we are probably in a jammed state. Do nothing
                // and break out of the infinite while loop.
                if (Queue.Count == 0)
                {
                    return;
                }

                var queueState = Queue.Dequeue();

                // Increment the PC before it is actually used in the operation.
                if (queueState.IncrementPC)
                {
                    CpuState.PC++;
                }

                if (queueState.PreAction != null)
                {
                    queueState.PreAction(this, _bus);
                }

                queueState.Operation.Execute(this, _bus);

                if (queueState.PostAction != null)
                {
                    queueState.PostAction(this, _bus);
                }

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
                static void PushPOnStack(Cpu cpu, IBus bus)
                {
                    var p = (byte)((byte)cpu.CpuState.P | 0x20); // Push the program status flags with B = 0 and U = 1.
                    bus.Write((ushort)(0x100 + cpu.CpuState.S - 2), p);
                }

                AddOperation(Nop.Singleton, false, &PushPOnStack);

                // Fetch the low byte of the interrupt vector address and decrement the stack by 3.
                static void DecrementStackBy3(Cpu cpu, IBus bus)
                {
                    cpu.CpuState.S -= 3;
                }

                AddOperation(FetchNmiVectorLowIntoAddressLatchLow.Singleton, false, &DecrementStackBy3);

                // Fetch the high byte of the interrupt vector address.
                AddOperation(FetchNmiVectorHighIntoAddressLatchHigh.Singleton, false);

                // Set PC to the address latch and fetch the instruction found at PC.
                static void SetNmiToFalse(Cpu cpu, IBus bus)
                {
                    // Set NMI to false to allow an NMI to occur again.
                    cpu.Nmi = false;
                }

                AddOperation(SetPCToAddressLatchAndFetchInstruction.Singleton, false, &SetNmiToFalse);
            }
        }

        // TODO:
        // Instructions not yet tested:
        // All conditional jumps
        // AND
        // EOR
        // ORA

        // TODO: Cleanup and explain conditional jumps.

        public void ExecuteInstruction(byte opcode)
        {
            switch ((Opcode)opcode)
            {
                case Adc_Absolute:
                    Addr_Absolute(&Op_Adc, delayedExecution: true);
                    break;

                case Adc_AbsoluteWithXOffset:
                    Addr_AbsoluteWithXOffset(&Op_Adc, delayedExecution: true);
                    break;

                case Adc_AbsoluteWithYOffset:
                    Addr_AbsoluteWithYOffset(&Op_Adc, delayedExecution: true);
                    break;

                case Adc_Immediate:
                    Addr_Immediate(&Op_Adc, delayedExecution: true);
                    break;

                case Adc_IndirectZeroPageWithXOffset:
                    Addr_IndirectZeroPageWithXOffset(&Op_Adc, delayedExecution: true);
                    break;

                case Adc_IndirectZeroPageWithYOffset:
                    Addr_IndirectZeroPageWithYOffset(&Op_Adc, delayedExecution: true);
                    break;

                case Adc_ZeroPage:
                    Addr_ZeroPage(&Op_Adc, delayedExecution: true);
                    break;

                case Adc_ZeroPageWithXOffset:
                    Addr_ZeroPageWithXOffset(&Op_Adc, delayedExecution: true);
                    break;

                case And_Absolute:
                    Addr_Absolute(&Op_And, delayedExecution: true);
                    break;

                case And_AbsoluteWithXOffset:
                    Addr_AbsoluteWithXOffset(&Op_And, delayedExecution: true);
                    break;

                case And_AbsoluteWithYOffset:
                    Addr_AbsoluteWithYOffset(&Op_And, delayedExecution: true);
                    break;

                case And_Immediate:
                    Addr_Immediate(&Op_And, delayedExecution: true);
                    break;

                case And_IndirectZeroPageWithXOffset:
                    Addr_IndirectZeroPageWithXOffset(&Op_And, delayedExecution: true);
                    break;

                case And_IndirectZeroPageWithYOffset:
                    Addr_IndirectZeroPageWithYOffset(&Op_And, delayedExecution: true);
                    break;

                case And_ZeroPage:
                    Addr_ZeroPage(&Op_And, delayedExecution: true);
                    break;

                case And_ZeroPageWithXOffset:
                    Addr_ZeroPageWithXOffset(&Op_And, delayedExecution: true);
                    break;

                case Asl_Absolute:
                    Addr_Absolute_WriteBack(&Op_Asl_DataLatch);
                    break;

                case Asl_AbsoluteWithXOffset:
                    Addr_AbsoluteWithXOffset_WriteBack(&Op_Asl_DataLatch);
                    break;

                case Asl_Accumulator:
                    Addr_Implied(&Op_Asl_Accumulator, delayedExecution: true);
                    break;

                case Asl_ZeroPage:
                    Addr_ZeroPage_WriteBack(&Op_Asl_DataLatch);
                    break;

                case Asl_ZeroPageWithXOffset:
                    Addr_ZeroPageWithXOffset_WriteBack(&Op_Asl_DataLatch);
                    break;

                case Bcc_Relative:
                    Addr_Relative(&Op_Bcc);
                    break;

                case Bcs_Relative:
                    Addr_Relative(&Op_Bcs);
                    break;

                case Beq_Relative:
                    Addr_Relative(&Op_Beq);
                    break;

                case Bit_Absolute:
                    Addr_Absolute(&Op_Bit);
                    break;

                case Bit_ZeroPage:
                    Addr_ZeroPage(&Op_Bit);
                    break;

                case Bmi_Relative:
                    Addr_Relative(&Op_Bmi);
                    break;

                case Bne_Relative:
                    Addr_Relative(&Op_Bne);
                    break;

                case Bpl_Relative:
                    Addr_Relative(&Op_Bpl);
                    break;

                case Brk_Implied:
                    AddOperation(FetchMemoryByPCIntoDataLatch.Singleton, true); // Dummy read.
                    AddOperation(Nop.Singleton, true, &WritePCHighToStack);
                    AddOperation(Nop.Singleton, false, &WritePCLowToStackMinus1);
                    AddOperation(Nop.Singleton, false, &WritePToStackMinus2, &SetInterruptFlag); // Set I after pushing P to the stack.
                    AddOperation(FetchIrqVectorLowIntoEffectiveAddressLatchLow.Singleton, false, &DecrementSByThree);
                    AddOperation(FetchIrqVectorHighIntoEffectiveAddressLatchHigh.Singleton, false);
                    AddOperation(FetchInstruction.Singleton, false, &Op_Jmp);
                    break;

                case Bvc_Relative:
                    Addr_Relative(&Op_Bvc);
                    break;

                case Bvs_Relative:
                    Addr_Relative(&Op_Bvs);
                    break;

                case Clc_Implied:
                    Addr_Implied(&Op_Clc);
                    break;

                case Cld_Implied:
                    Addr_Implied(&Op_Cld);
                    break;

                case Cli_Implied:
                    Addr_Implied(&Op_Cli);
                    break;

                case Clv_Implied:
                    Addr_Implied(&Op_Clv);
                    break;

                case Cmp_Absolute:
                    Addr_Absolute(&Op_Cmp, delayedExecution: true);
                    break;

                case Cmp_AbsoluteWithXOffset:
                    Addr_AbsoluteWithXOffset(&Op_Cmp, delayedExecution: true);
                    break;

                case Cmp_AbsoluteWithYOffset:
                    Addr_AbsoluteWithYOffset(&Op_Cmp, delayedExecution: true);
                    break;

                case Cmp_Immediate:
                    Addr_Immediate(&Op_Cmp, delayedExecution: true);
                    break;

                case Cmp_IndirectZeroPageWithXOffset:
                    Addr_IndirectZeroPageWithXOffset(&Op_Cmp, delayedExecution: true);
                    break;

                case Cmp_IndirectZeroPageWithYOffset:
                    Addr_IndirectZeroPageWithYOffset(&Op_Cmp, delayedExecution: true);
                    break;

                case Cmp_ZeroPage:
                    Addr_ZeroPage(&Op_Cmp, delayedExecution: true);
                    break;

                case Cmp_ZeroPageWithXOffset:
                    Addr_ZeroPageWithXOffset(&Op_Cmp, delayedExecution: true);
                    break;

                case Cpx_Absolute:
                    Addr_Absolute(&Op_Cpx, delayedExecution: true);
                    break;

                case Cpx_Immediate:
                    Addr_Immediate(&Op_Cpx, delayedExecution: true);
                    break;

                case Cpx_ZeroPage:
                    Addr_ZeroPage(&Op_Cpx, delayedExecution: true);
                    break;

                case Cpy_Absolute:
                    Addr_Absolute(&Op_Cpy, delayedExecution: true);
                    break;

                case Cpy_Immediate:
                    Addr_Immediate(&Op_Cpy, delayedExecution: true);
                    break;

                case Cpy_ZeroPage:
                    Addr_ZeroPage(&Op_Cpy, delayedExecution: true);
                    break;

                case Dec_Absolute:
                    Addr_Absolute_WriteBack(&Op_Dec);
                    break;

                case Dec_AbsoluteWithXOffset:
                    Addr_AbsoluteWithXOffset_WriteBack(&Op_Dec);
                    break;

                case Dec_ZeroPage:
                    Addr_ZeroPage_WriteBack(&Op_Dec);
                    break;

                case Dec_ZeroPageWithXOffset:
                    Addr_ZeroPageWithXOffset_WriteBack(&Op_Dec);
                    break;

                case Dex_Implied:
                    Addr_Implied(&Op_Dex, delayedExecution: true);
                    break;

                case Dey_Implied:
                    Addr_Implied(&Op_Dey, delayedExecution: true);
                    break;

                case Eor_Absolute:
                    Addr_Absolute(&Op_Eor, delayedExecution: true);
                    break;

                case Eor_AbsoluteWithXOffset:
                    Addr_AbsoluteWithXOffset(&Op_Eor, delayedExecution: true);
                    break;

                case Eor_AbsoluteWithYOffset:
                    Addr_AbsoluteWithYOffset(&Op_Eor, delayedExecution: true);
                    break;

                case Eor_Immediate:
                    Addr_Immediate(&Op_Eor, delayedExecution: true);
                    break;

                case Eor_IndirectZeroPageWithXOffset:
                    Addr_IndirectZeroPageWithXOffset(&Op_Eor, delayedExecution: true);
                    break;

                case Eor_IndirectZeroPageWithYOffset:
                    Addr_IndirectZeroPageWithYOffset(&Op_Eor, delayedExecution: true);
                    break;

                case Eor_ZeroPage:
                    Addr_ZeroPage(&Op_Eor, delayedExecution: true);
                    break;

                case Eor_ZeroPageWithXOffset:
                    Addr_ZeroPageWithXOffset(&Op_Eor, delayedExecution: true);
                    break;

                case Inc_Absolute:
                    Addr_Absolute_WriteBack(&Op_Inc);
                    break;

                case Inc_AbsoluteWithXOffset:
                    Addr_AbsoluteWithXOffset_WriteBack(&Op_Inc);
                    break;

                case Inc_ZeroPage:
                    Addr_ZeroPage_WriteBack(&Op_Inc);
                    break;

                case Inc_ZeroPageWithXOffset:
                    Addr_ZeroPageWithXOffset_WriteBack(&Op_Inc);
                    break;

                case Inx_Implied:
                    Addr_Implied(&Op_Inx, delayedExecution: true);
                    break;

                case Iny_Implied:
                    Addr_Implied(&Op_Iny, delayedExecution: true);
                    break;

                case Jmp_Absolute:
                    AddOperation(FetchMemoryByPCIntoEffectiveAddressLatchLow.Singleton, true);
                    AddOperation(FetchMemoryByPCIntoEffectiveAddressLatchHigh.Singleton, true);
                    AddOperation(FetchInstruction.Singleton, false, &Op_Jmp);
                    break;

                case Jmp_Indirect:
                    AddOperation(FetchMemoryByPCIntoAddressLatchLow.Singleton, true);
                    AddOperation(FetchMemoryByPCIntoAddressLatchHigh.Singleton, true);
                    AddOperation(FetchMemoryByAddressLatchIntoEffectiveAddressLatchLow.Singleton, true); // PC increment doesn't matter, but it does happen.
                    AddOperation(FetchMemoryByAddressLatchIntoEffectiveAddressLatchHighWithWrapping.Singleton, false);
                    AddOperation(FetchInstruction.Singleton, false, &Op_Jmp);
                    break;

                case Jsr_Absolute:
                    AddOperation(FetchMemoryByPCIntoEffectiveAddressLatchLow.Singleton, true);
                    AddOperation(FetchMemoryByStackIntoDataLatch.Singleton, true); // Discarded read.
                    AddOperation(Nop.Singleton, false, &WritePCHighToStack, &DecrementS);
                    AddOperation(Nop.Singleton, false, &WritePCLowToStack, &DecrementS);
                    AddOperation(FetchMemoryByPCIntoEffectiveAddressLatchHigh.Singleton, false);
                    AddOperation(FetchInstruction.Singleton, true, &Op_Jmp);
                    break;

                case Lda_Absolute:
                    Addr_Absolute(&Op_Lda);
                    break;

                case Lda_AbsoluteWithXOffset:
                    Addr_AbsoluteWithXOffset(&Op_Lda);
                    break;

                case Lda_AbsoluteWithYOffset:
                    Addr_AbsoluteWithYOffset(&Op_Lda);
                    break;

                case Lda_Immediate:
                    Addr_Immediate(&Op_Lda);
                    break;

                case Lda_IndirectZeroPageWithXOffset:
                    Addr_IndirectZeroPageWithXOffset(&Op_Lda);
                    break;

                case Lda_IndirectZeroPageWithYOffset:
                    Addr_IndirectZeroPageWithYOffset(&Op_Lda);
                    break;

                case Lda_ZeroPage:
                    Addr_ZeroPage(&Op_Lda);
                    break;

                case Lda_ZeroPageWithXOffset:
                    Addr_ZeroPageWithXOffset(&Op_Lda);
                    break;

                case Ldx_Absolute:
                    Addr_Absolute(&Op_Ldx);
                    break;

                case Ldx_AbsoluteWithYOffset:
                    Addr_AbsoluteWithYOffset(&Op_Ldx);
                    break;

                case Ldx_Immediate:
                    Addr_Immediate(&Op_Ldx);
                    break;

                case Ldx_ZeroPage:
                    Addr_ZeroPage(&Op_Ldx);
                    break;

                case Ldx_ZeroPageWithYOffset:
                    Addr_ZeroPageWithYOffset(&Op_Ldx);
                    break;

                case Ldy_Absolute:
                    Addr_Absolute(&Op_Ldy);
                    break;

                case Ldy_AbsoluteWithXOffset:
                    Addr_AbsoluteWithXOffset(&Op_Ldy);
                    break;

                case Ldy_Immediate:
                    Addr_Immediate(&Op_Ldy);
                    break;

                case Ldy_ZeroPage:
                    Addr_ZeroPage(&Op_Ldy);
                    break;

                case Ldy_ZeroPageWithXOffset:
                    Addr_ZeroPageWithXOffset(&Op_Ldy);
                    break;

                case Lsr_Absolute:
                    Addr_Absolute_WriteBack(&Op_Lsr_DataLatch);
                    break;

                case Lsr_AbsoluteWithXOffset:
                    Addr_AbsoluteWithXOffset_WriteBack(&Op_Lsr_DataLatch);
                    break;

                case Lsr_Accumulator:
                    Addr_Implied(&Op_Lsr_Accumulator, delayedExecution: true);
                    break;

                case Lsr_ZeroPage:
                    Addr_ZeroPage_WriteBack(&Op_Lsr_DataLatch);
                    break;

                case Lsr_ZeroPageWithXOffset:
                    Addr_ZeroPageWithXOffset_WriteBack(&Op_Lsr_DataLatch);
                    break;

                case Nop_Implied:
                    Addr_Implied(null);
                    break;

                case Ora_Absolute:
                    Addr_Absolute(&Op_Ora, delayedExecution: true);
                    break;

                case Ora_AbsoluteWithXOffset:
                    Addr_AbsoluteWithXOffset(&Op_Ora, delayedExecution: true);
                    break;

                case Ora_AbsoluteWithYOffset:
                    Addr_AbsoluteWithYOffset(&Op_Ora, delayedExecution: true);
                    break;

                case Ora_Immediate:
                    Addr_Immediate(&Op_Ora, delayedExecution: true);
                    break;

                case Ora_IndirectZeroPageWithXOffset:
                    Addr_IndirectZeroPageWithXOffset(&Op_Ora, delayedExecution: true);
                    break;

                case Ora_IndirectZeroPageWithYOffset:
                    Addr_IndirectZeroPageWithYOffset(&Op_Ora, delayedExecution: true);
                    break;

                case Ora_ZeroPage:
                    Addr_ZeroPage(&Op_Ora, delayedExecution: true);
                    break;

                case Ora_ZeroPageWithXOffset:
                    Addr_ZeroPageWithXOffset(&Op_Ora, delayedExecution: true);
                    break;

                case Pha_Implied:
                    AddOperation(FetchMemoryByPCIntoDataLatch.Singleton, true);
                    AddOperation(Nop.Singleton, false, &WriteAToStack);
                    AddOperation(FetchInstruction.Singleton, false, &DecrementS);
                    break;

                case Php_Implied:
                    AddOperation(FetchMemoryByPCIntoDataLatch.Singleton, true);
                    AddOperation(Nop.Singleton, false, &WritePToStack);
                    AddOperation(FetchInstruction.Singleton, false, &DecrementS);
                    break;

                case Pla_Implied:
                    AddOperation(FetchMemoryByPCIntoDataLatch.Singleton, true);
                    AddOperation(FetchMemoryByStackIntoDataLatch.Singleton, false);
                    AddOperation(FetchMemoryByStackIntoDataLatch.Singleton, false, &IncrementS);
                    AddOperation(FetchInstruction.Singleton, false, &TransferDataLatchToA);
                    break;

                case Plp_Implied:
                    AddOperation(FetchMemoryByPCIntoDataLatch.Singleton, true);
                    AddOperation(FetchMemoryByStackIntoDataLatch.Singleton, false);
                    AddOperation(FetchMemoryByStackIntoDataLatch.Singleton, false, &IncrementS);
                    AddOperation(FetchInstruction.Singleton, false, &TransferDataLatchToP);
                    break;

                case Rol_Absolute:
                    Addr_Absolute_WriteBack(&Op_Rol_DataLatch);
                    break;

                case Rol_AbsoluteWithXOffset:
                    Addr_AbsoluteWithXOffset_WriteBack(&Op_Rol_DataLatch);
                    break;

                case Rol_Accumulator:
                    Addr_Implied(&Op_Rol_Accumulator, delayedExecution: true);
                    break;

                case Rol_ZeroPage:
                    Addr_ZeroPage_WriteBack(&Op_Rol_DataLatch);
                    break;

                case Rol_ZeroPageWithXOffset:
                    Addr_ZeroPageWithXOffset_WriteBack(&Op_Rol_DataLatch);
                    break;

                case Ror_Absolute:
                    Addr_Absolute_WriteBack(&Op_Ror_DataLatch);
                    break;

                case Ror_AbsoluteWithXOffset:
                    Addr_AbsoluteWithXOffset_WriteBack(&Op_Ror_DataLatch);
                    break;

                case Ror_Accumulator:
                    Addr_Implied(&Op_Ror_Accumulator, delayedExecution: true);
                    break;

                case Ror_ZeroPage:
                    Addr_ZeroPage_WriteBack(&Op_Ror_DataLatch);
                    break;

                case Ror_ZeroPageWithXOffset:
                    Addr_ZeroPageWithXOffset_WriteBack(&Op_Ror_DataLatch);
                    break;

                case Rti_Implied:
                    // TODO: Implement better?

                    // The PC increments are inconsequential but do happen.
                    AddOperation(Nop.Singleton, true);
                    AddOperation(Nop.Singleton, true);

                    // Pull P from stack and store it in the data latch but don't set P yet.
                    static void PullPFromStack(Cpu cpu, IBus bus)
                    {
                        cpu.DataLatch = bus.Read((ushort)(0x100 + cpu.CpuState.S + 1));

                        // During interrupts, bits 4 and 5 of the status register may be set in the
                        // stack. Make sure to clear these when pulling P from the stack as these
                        // two bits don't actually exist.
                        cpu.DataLatch = (byte)(cpu.DataLatch & ~0x30);
                    }

                    AddOperation(Nop.Singleton, false, &PullPFromStack);

                    // Pull PC low from the stack. Set P to the data latch.
                    static void PullPCLowFromStack(Cpu cpu, IBus bus)
                    {
                        cpu.CpuState.P = (CpuFlags)cpu.DataLatch;

                        cpu.EffectiveAddressLatchLow = bus.Read((ushort)(0x100 + cpu.CpuState.S + 2));
                    }

                    AddOperation(Nop.Singleton, false, &PullPCLowFromStack);

                    // Pull PC high from the stack. Increment S by 3.
                    static void PullPCHighFromStack(Cpu cpu, IBus bus)
                    {
                        cpu.EffectiveAddressLatchHigh = bus.Read((ushort)(0x100 + cpu.CpuState.S + 3));
                        cpu.CpuState.S += 3;
                    }

                    AddOperation(Nop.Singleton, false, &PullPCHighFromStack);

                    // Load PC and fetch the next instruction.
                    static void SetPCAndFetchInstruction(Cpu cpu, IBus bus)
                    {
                        cpu.CpuState.PC = (ushort)(cpu.EffectiveAddressLatchLow | (cpu.EffectiveAddressLatchHigh << 8));

                        var instruction = bus.Read(cpu.CpuState.PC);
                        cpu.ExecuteInstruction(instruction);
                    }

                    AddOperation(Nop.Singleton, false, &SetPCAndFetchInstruction);

                    break;

                case Rts_Implied:
                    AddOperation(FetchMemoryByPCIntoDataLatch.Singleton, true); // Dummy read by PC.
                    AddOperation(FetchMemoryByStackIntoDataLatch.Singleton, true); // Dummy read by stack.
                    AddOperation(Nop.Singleton, false, &ReadSPlus1IntoEffectiveAddressLatchLow);
                    AddOperation(Nop.Singleton, false, &ReadSPlus2IntoEffectiveAddressLatchHigh, &IncrementSByTwo);
                    AddOperation(FetchMemoryByPCIntoDataLatch.Singleton, false, &Op_Jmp); // Dummy read by PC after jump.
                    AddOperation(FetchInstruction.Singleton, true);
                    break;

                case Sbc_Absolute:
                    Addr_Absolute(&Op_Sbc, delayedExecution: true);
                    break;

                case Sbc_AbsoluteWithXOffset:
                    Addr_AbsoluteWithXOffset(&Op_Sbc, delayedExecution: true);
                    break;

                case Sbc_AbsoluteWithYOffset:
                    Addr_AbsoluteWithYOffset(&Op_Sbc, delayedExecution: true);
                    break;

                case Sbc_Immediate:
                    Addr_Immediate(&Op_Sbc, delayedExecution: true);
                    break;

                case Sbc_IndirectZeroPageWithXOffset:
                    Addr_IndirectZeroPageWithXOffset(&Op_Sbc, delayedExecution: true);
                    break;

                case Sbc_IndirectZeroPageWithYOffset:
                    Addr_IndirectZeroPageWithYOffset(&Op_Sbc, delayedExecution: true);
                    break;

                case Sbc_ZeroPage:
                    Addr_ZeroPage(&Op_Sbc, delayedExecution: true);
                    break;

                case Sbc_ZeroPageWithXOffset:
                    Addr_ZeroPageWithXOffset(&Op_Sbc, delayedExecution: true);
                    break;

                case Sec_Implied:
                    Addr_Implied(&Op_Sec);
                    break;

                case Sed_Implied:
                    Addr_Implied(&Op_Sed);
                    break;

                case Sei_Implied:
                    Addr_Implied(&Op_Sei);
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
                    AddOperation(FetchMemoryByAddressLatchIntoEffectiveAddressLatchHighWithWrapping.Singleton, false);
                    AddOperation(WriteAToMemoryByEffectiveAddressLatch.Singleton, false);
                    AddOperation(FetchInstruction.Singleton, false);
                    break;

                case Sta_IndirectZeroPageWithYOffset:
                    AddOperation(FetchZeroPageAddressByPCIntoAddressLatch.Singleton, true);
                    AddOperation(FetchMemoryByAddressLatchIntoEffectiveAddressLatchLow.Singleton, true);
                    AddOperation(FetchMemoryByAddressLatchIntoEffectiveAddressLatchHighWithWrapping.Singleton, false);
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
                    Addr_Implied(&Op_Tax);
                    break;

                case Tay_Implied:
                    Addr_Implied(&Op_Tay);
                    break;

                case Tsx_Implied:
                    Addr_Implied(&Op_Tsx);
                    break;

                case Txa_Implied:
                    Addr_Implied(&Op_Txa);
                    break;

                case Txs_Implied:
                    Addr_Implied(&Op_Txs);
                    break;

                case Tya_Implied:
                    Addr_Implied(&Op_Tya);
                    break;

                case Ahx_AbsoluteWithYOffset_9F:
                case Ahx_IndirectZeroPageWithYOffset_93:
                case Alr_Immediate_4B:
                case Anc_Immediate_0B:
                case Anc_Immediate_2B:
                case Arr_Immediate_6B:
                case Axs_Immediate_CB:
                case Dcp_Absolute_CF:
                case Dcp_AbsoluteWithXOffset_DF:
                case Dcp_AbsoluteWithYOffset_DB:
                case Dcp_IndirectZeroPageWithXOffset_C3:
                case Dcp_IndirectZeroPageWithYOffset_D3:
                case Dcp_ZeroPage_C7:
                case Dcp_ZeroPageWithXOffset_D7:
                case Isc_Absolute_EF:
                case Isc_AbsoluteWithXOffset_FF:
                case Isc_AbsoluteWithYOffset_FB:
                case Isc_IndirectZeroPageWithXOffset_E3:
                case Isc_IndirectZeroPageWithYOffset_F3:
                case Isc_ZeroPage_E7:
                case Isc_ZeroPageWithXOffset_F7:
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
                case Rla_Absolute_2F:
                case Rla_AbsoluteWithXOffset_3F:
                case Rla_AbsoluteWithYOffset_3B:
                case Rla_IndirectZeroPageWithXOffset_23:
                case Rla_IndirectZeroPageWithYOffset_33:
                case Rla_ZeroPage_27:
                case Rla_ZeroPageWithXOffset_37:
                case Rra_Absolute_6F:
                case Rra_AbsoluteWithXOffset_7F:
                case Rra_AbsoluteWithYOffset_7B:
                case Rra_IndirectZeroPageWithXOffset_63:
                case Rra_IndirectZeroPageWithYOffset_73:
                case Rra_ZeroPage_67:
                case Rra_ZeroPageWithXOffset_77:
                case Sax_Absolute_8F:
                case Sax_IndirectZeroPageWithXOffset_83:
                case Sax_ZeroPage_87:
                case Sax_ZeroPageWithYOffset_97:
                case Sbc_Immediate_EB:
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

        // Instructions
        private static void Op_Adc(Cpu cpu, IBus bus)
        {
            var resultTemp = cpu.CpuState.A + cpu.DataLatch + (cpu.CpuState.GetFlag(CpuFlags.C) ? 1 : 0);
            var resultByte = (byte)(resultTemp & 0xff);

            cpu.CpuState.SetFlag(CpuFlags.C, resultTemp > 0xff);
            cpu.CpuState.SetZeroFlag(resultByte);
            cpu.CpuState.SetFlag(CpuFlags.V, ((cpu.CpuState.A ^ cpu.DataLatch) & 0x80) == 0 && ((cpu.CpuState.A ^ resultTemp) & 0x80) != 0);
            cpu.CpuState.SetNegativeFlag(resultByte);

            cpu.CpuState.A = resultByte;
        }

        private static void Op_And(Cpu cpu, IBus bus)
        {
            cpu.CpuState.A = (byte)(cpu.CpuState.A & cpu.DataLatch);

            cpu.CpuState.SetZeroFlag(cpu.CpuState.A);
            cpu.CpuState.SetNegativeFlag(cpu.CpuState.A);
        }

        private static void Op_Asl_DataLatch(Cpu cpu, IBus bus)
        {
            cpu.CpuState.SetFlag(CpuFlags.C, (cpu.DataLatch & 0x80) != 0); // Carry flag is set to the bit that is being shifted out.

            cpu.DataLatch = (byte)(cpu.DataLatch << 1);

            cpu.CpuState.SetZeroFlag(cpu.DataLatch);
            cpu.CpuState.SetNegativeFlag(cpu.DataLatch);
        }

        private static void Op_Asl_Accumulator(Cpu cpu, IBus bus)
        {
            cpu.DataLatch = cpu.CpuState.A;
            Op_Asl_DataLatch(cpu, bus); // Performs the operation directly to the data latch.
            cpu.CpuState.A = cpu.DataLatch;
        }

        private static void SetPCToEffectiveAddressLatch(Cpu cpu, IBus bus)
        {
            cpu.CpuState.PC = (ushort)(cpu.EffectiveAddressLatchLow | (cpu.EffectiveAddressLatchHigh << 8));
        }

        /// <summary>
        /// All of the conditional jump instructions call this method. When a conditional jump is
        /// decoded onto the queue, four operations are queued (see <see cref="Addr_Relative"/>).
        /// If the condition fails and the jump is not being taken, this method will dequeue the
        /// next three operations and queue a new operation that simply increments <c>PC</c> and
        /// fetches the next instruction. If the jump is being taken, then this method sets the
        /// address latch to <c>PC + 1</c>. This way, to find the destination of the jump, just add
        /// the offset from the instruction's operand and add it to the address latch.
        /// </summary>
        /// <param name="takingJump">A boolean that says whether the jump is being taken. Pass <c>true</c> if the branch condition succeeds and the jump is being taken. Otherwise, pass <c>false</c>.</param>
        private static void PerformConditionalJumpOperation(Cpu cpu, bool takingJump)
        {
            if (!takingJump)
            {
                cpu.Queue.Dequeue();
                cpu.Queue.Dequeue();
                cpu.Queue.Dequeue();

                cpu.AddOperation(FetchInstruction.Singleton, true);
            }

            // Save PC + 1 into address latch because it will get clobbered.
            cpu.AddressLatchLow = (byte)((cpu.CpuState.PC + 1) & 0xff);
            cpu.AddressLatchHigh = (byte)((cpu.CpuState.PC + 1) >> 8);
        }

        private static void Op_Bcc(Cpu cpu, IBus bus)
        {
            PerformConditionalJumpOperation(cpu, !cpu.CpuState.GetFlag(CpuFlags.C));
        }

        private static void Op_Bcs(Cpu cpu, IBus bus)
        {
            PerformConditionalJumpOperation(cpu, cpu.CpuState.GetFlag(CpuFlags.C));
        }

        private static void Op_Beq(Cpu cpu, IBus bus)
        {
            PerformConditionalJumpOperation(cpu, cpu.CpuState.GetFlag(CpuFlags.Z));
        }

        private static void Op_Bit(Cpu cpu, IBus bus)
        {
            cpu.CpuState.SetZeroFlag((byte)(cpu.DataLatch & cpu.CpuState.A));
            cpu.CpuState.SetFlag(CpuFlags.V, (cpu.DataLatch & 0x40) != 0); // Set overflow flag to bit 6 of data.
            cpu.CpuState.SetNegativeFlag(cpu.DataLatch);
        }

        private static void Op_Bmi(Cpu cpu, IBus bus)
        {
            PerformConditionalJumpOperation(cpu, cpu.CpuState.GetFlag(CpuFlags.N));
        }

        private static void Op_Bne(Cpu cpu, IBus bus)
        {
            PerformConditionalJumpOperation(cpu, !cpu.CpuState.GetFlag(CpuFlags.Z));
        }

        private static void Op_Bpl(Cpu cpu, IBus bus)
        {
            PerformConditionalJumpOperation(cpu, !cpu.CpuState.GetFlag(CpuFlags.N));
        }

        private static void Op_Bvc(Cpu cpu, IBus bus)
        {
            PerformConditionalJumpOperation(cpu, !cpu.CpuState.GetFlag(CpuFlags.V));
        }

        private static void Op_Bvs(Cpu cpu, IBus bus)
        {
            PerformConditionalJumpOperation(cpu, cpu.CpuState.GetFlag(CpuFlags.V));
        }

        private static void Op_Clc(Cpu cpu, IBus bus)
        {
            cpu.CpuState.SetFlag(CpuFlags.C, false);
        }

        private static void Op_Cld(Cpu cpu, IBus bus)
        {
            cpu.CpuState.SetFlag(CpuFlags.D, false);
        }

        private static void Op_Cli(Cpu cpu, IBus bus)
        {
            cpu.CpuState.SetFlag(CpuFlags.I, false);
        }

        private static void Op_Clv(Cpu cpu, IBus bus)
        {
            cpu.CpuState.SetFlag(CpuFlags.V, false);
        }

        private static void Op_Cmp(Cpu cpu, IBus bus)
        {
            var result = (ushort)(cpu.CpuState.A - cpu.DataLatch);

            cpu.CpuState.SetFlag(CpuFlags.C, cpu.CpuState.A >= cpu.DataLatch);
            cpu.CpuState.SetZeroFlag(result);
            cpu.CpuState.SetNegativeFlag(result);
        }

        private static void Op_Cpx(Cpu cpu, IBus bus)
        {
            var result = (ushort)(cpu.CpuState.X - cpu.DataLatch);

            cpu.CpuState.SetFlag(CpuFlags.C, cpu.CpuState.X >= cpu.DataLatch);
            cpu.CpuState.SetZeroFlag(result);
            cpu.CpuState.SetNegativeFlag(result);
        }

        private static void Op_Cpy(Cpu cpu, IBus bus)
        {
            var result = (ushort)(cpu.CpuState.Y - cpu.DataLatch);

            cpu.CpuState.SetFlag(CpuFlags.C, cpu.CpuState.Y >= cpu.DataLatch);
            cpu.CpuState.SetZeroFlag(result);
            cpu.CpuState.SetNegativeFlag(result);
        }

        private static void Op_Dec(Cpu cpu, IBus bus)
        {
            cpu.DataLatch--;

            cpu.CpuState.SetZeroFlag(cpu.DataLatch);
            cpu.CpuState.SetNegativeFlag(cpu.DataLatch);
        }

        private static void Op_Dex(Cpu cpu, IBus bus)
        {
            cpu.CpuState.X--;

            cpu.CpuState.SetZeroFlag(cpu.CpuState.X);
            cpu.CpuState.SetNegativeFlag(cpu.CpuState.X);
        }

        private static void Op_Dey(Cpu cpu, IBus bus)
        {
            cpu.CpuState.Y--;

            cpu.CpuState.SetZeroFlag(cpu.CpuState.Y);
            cpu.CpuState.SetNegativeFlag(cpu.CpuState.Y);
        }

        private static void Op_Eor(Cpu cpu, IBus bus)
        {
            cpu.CpuState.A = (byte)(cpu.CpuState.A ^ cpu.DataLatch);

            cpu.CpuState.SetZeroFlag(cpu.CpuState.A);
            cpu.CpuState.SetNegativeFlag(cpu.CpuState.A);
        }

        private static void Op_Inc(Cpu cpu, IBus bus)
        {
            cpu.DataLatch++;

            cpu.CpuState.SetZeroFlag(cpu.DataLatch);
            cpu.CpuState.SetNegativeFlag(cpu.DataLatch);
        }

        private static void Op_Inx(Cpu cpu, IBus bus)
        {
            cpu.CpuState.X++;

            cpu.CpuState.SetZeroFlag(cpu.CpuState.X);
            cpu.CpuState.SetNegativeFlag(cpu.CpuState.X);
        }

        private static void Op_Iny(Cpu cpu, IBus bus)
        {
            cpu.CpuState.Y++;

            cpu.CpuState.SetZeroFlag(cpu.CpuState.Y);
            cpu.CpuState.SetNegativeFlag(cpu.CpuState.Y);
        }

        private static void Op_Jmp(Cpu cpu, IBus bus)
        {
            cpu.CpuState.PC = (ushort)(cpu.EffectiveAddressLatchLow | (cpu.EffectiveAddressLatchHigh << 8));
        }

        private static void Op_Lda(Cpu cpu, IBus bus)
        {
            cpu.CpuState.A = cpu.DataLatch;

            cpu.CpuState.SetZeroFlag(cpu.CpuState.A);
            cpu.CpuState.SetNegativeFlag(cpu.CpuState.A);
        }

        private static void Op_Ldx(Cpu cpu, IBus bus)
        {
            cpu.CpuState.X = cpu.DataLatch;

            cpu.CpuState.SetZeroFlag(cpu.CpuState.X);
            cpu.CpuState.SetNegativeFlag(cpu.CpuState.X);
        }

        private static void Op_Ldy(Cpu cpu, IBus bus)
        {
            cpu.CpuState.Y = cpu.DataLatch;

            cpu.CpuState.SetZeroFlag(cpu.CpuState.Y);
            cpu.CpuState.SetNegativeFlag(cpu.CpuState.Y);
        }

        private static void Op_Lsr_DataLatch(Cpu cpu, IBus bus)
        {
            cpu.CpuState.SetFlag(CpuFlags.C, (cpu.DataLatch & 0x01) != 0); // Carry flag is set to the bit that is being shifted out.

            cpu.DataLatch = (byte)(cpu.DataLatch >> 1);

            cpu.CpuState.SetZeroFlag(cpu.DataLatch);
            cpu.CpuState.SetNegativeFlag(cpu.DataLatch);
        }

        private static void Op_Lsr_Accumulator(Cpu cpu, IBus bus)
        {
            cpu.DataLatch = cpu.CpuState.A;
            Op_Lsr_DataLatch(cpu, bus); // Performs the operation directly to the data latch.
            cpu.CpuState.A = cpu.DataLatch;
        }

        private static void Op_Ora(Cpu cpu, IBus bus)
        {
            cpu.CpuState.A = (byte)(cpu.CpuState.A | cpu.DataLatch);

            cpu.CpuState.SetZeroFlag(cpu.CpuState.A);
            cpu.CpuState.SetNegativeFlag(cpu.CpuState.A);
        }

        private static void Op_Rol_DataLatch(Cpu cpu, IBus bus)
        {
            var newCarry = (cpu.DataLatch & 0x80) != 0;

            cpu.DataLatch = (byte)(cpu.DataLatch << 1);

            if (cpu.CpuState.GetFlag(CpuFlags.C))
            {
                cpu.DataLatch |= 0x01;
            }

            cpu.CpuState.SetFlag(CpuFlags.C, newCarry);
            cpu.CpuState.SetZeroFlag(cpu.DataLatch);
            cpu.CpuState.SetNegativeFlag(cpu.DataLatch);
        }

        private static void Op_Rol_Accumulator(Cpu cpu, IBus bus)
        {
            cpu.DataLatch = cpu.CpuState.A;
            Op_Rol_DataLatch(cpu, bus); // Performs the operation directly to the data latch.
            cpu.CpuState.A = cpu.DataLatch;
        }

        private static void Op_Ror_DataLatch(Cpu cpu, IBus bus)
        {
            var newCarry = (cpu.DataLatch & 0x01) != 0;

            cpu.DataLatch = (byte)(cpu.DataLatch >> 1);

            if (cpu.CpuState.GetFlag(CpuFlags.C))
            {
                cpu.DataLatch |= 0x80;
            }

            cpu.CpuState.SetFlag(CpuFlags.C, newCarry);
            cpu.CpuState.SetZeroFlag(cpu.DataLatch);
            cpu.CpuState.SetNegativeFlag(cpu.DataLatch);
        }

        private static void Op_Ror_Accumulator(Cpu cpu, IBus bus)
        {
            cpu.DataLatch = cpu.CpuState.A;
            Op_Ror_DataLatch(cpu, bus); // Performs the operation directly to the data latch.
            cpu.CpuState.A = cpu.DataLatch;
        }

        private static void IncrementS(Cpu cpu, IBus bus)
        {
            cpu.CpuState.S++;
        }

        private static void IncrementSByTwo(Cpu cpu, IBus bus)
        {
            cpu.CpuState.S += 2;
        }

        private static void DecrementS(Cpu cpu, IBus bus)
        {
            cpu.CpuState.S--;
        }

        private static void DecrementSByThree(Cpu cpu, IBus bus)
        {
            cpu.CpuState.S -= 3;
        }

        private static void WriteAToStack(Cpu cpu, IBus bus)
        {
            bus.Write((ushort)(cpu.CpuState.S + 0x100), cpu.CpuState.A);
        }

        private static void WritePToStack(Cpu cpu, IBus bus)
        {
            var data = (byte)((byte)cpu.CpuState.P | 0x30); // Push the program state with B = 1 and U = 1.
            bus.Write((ushort)(cpu.CpuState.S + 0x100), data);
        }

        private static void WritePToStackMinus2(Cpu cpu, IBus bus)
        {
            var data = (byte)((byte)cpu.CpuState.P | 0x30); // Push the program state with B = 1 and U = 1.
            bus.Write((ushort)(((cpu.CpuState.S - 2) & 0xff) + 0x100), data);
        }

        private static void WritePCHighToStack(Cpu cpu, IBus bus)
        {
            var data = (byte)(cpu.CpuState.PC >> 8);
            bus.Write((ushort)(cpu.CpuState.S + 0x100), data);
        }

        private static void WritePCLowToStack(Cpu cpu, IBus bus)
        {
            var data = (byte)(cpu.CpuState.PC & 0xff);
            bus.Write((ushort)(cpu.CpuState.S + 0x100), data);
        }

        private static void WritePCLowToStackMinus1(Cpu cpu, IBus bus)
        {
            var data = (byte)(cpu.CpuState.PC & 0xff);
            bus.Write((ushort)(((cpu.CpuState.S - 1) & 0xff) + 0x100), data);
        }

        private static void ReadSPlus1IntoEffectiveAddressLatchLow(Cpu cpu, IBus bus)
        {
            cpu.EffectiveAddressLatchLow = bus.Read((ushort)(((cpu.CpuState.S + 1) & 0xff) + 0x100));
        }

        private static void ReadSPlus2IntoEffectiveAddressLatchHigh(Cpu cpu, IBus bus)
        {
            cpu.EffectiveAddressLatchHigh = bus.Read((ushort)(((cpu.CpuState.S + 2) & 0xff) + 0x100));
        }

        private static void SetInterruptFlag(Cpu cpu, IBus bus)
        {
            cpu.CpuState.SetFlag(CpuFlags.I, true);
        }

        private static void TransferDataLatchToA(Cpu cpu, IBus bus)
        {
            cpu.CpuState.A = cpu.DataLatch;

            cpu.CpuState.SetZeroFlag(cpu.CpuState.A);
            cpu.CpuState.SetNegativeFlag(cpu.CpuState.A);
        }

        private static void TransferDataLatchToP(Cpu cpu, IBus bus)
        {
            // During PHP, bits 4 and 5 of the status register are set in the stack. Make sure to
            // clear these when pulling P from the stack as these two bits don't actually exist.
            cpu.CpuState.P = (CpuFlags)(cpu.DataLatch & ~0x30);
        }

        private static void Op_Sbc(Cpu cpu, IBus bus)
        {
            var result = cpu.CpuState.A - cpu.DataLatch - (cpu.CpuState.GetFlag(CpuFlags.C) ? 0 : 1);
            var resultByte = (byte)(result & 0xff);

            cpu.CpuState.SetFlag(CpuFlags.C, (ushort)result < 0x100);
            cpu.CpuState.SetZeroFlag(resultByte);
            cpu.CpuState.SetFlag(CpuFlags.V, ((cpu.CpuState.A ^ cpu.DataLatch) & 0x80) != 0 && ((cpu.CpuState.A ^ result) & 0x80) != 0);
            cpu.CpuState.SetNegativeFlag(resultByte);

            cpu.CpuState.A = resultByte;
        }

        private static void Op_Sec(Cpu cpu, IBus bus)
        {
            cpu.CpuState.SetFlag(CpuFlags.C, true);
        }

        private static void Op_Sed(Cpu cpu, IBus bus)
        {
            cpu.CpuState.SetFlag(CpuFlags.D, true);
        }

        private static void Op_Sei(Cpu cpu, IBus bus)
        {
            cpu.CpuState.SetFlag(CpuFlags.I, true);
        }

        private static void Op_Tax(Cpu cpu, IBus bus)
        {
            cpu.CpuState.X = cpu.CpuState.A;

            cpu.CpuState.SetZeroFlag(cpu.CpuState.X);
            cpu.CpuState.SetNegativeFlag(cpu.CpuState.X);
        }

        private static void Op_Tay(Cpu cpu, IBus bus)
        {
            cpu.CpuState.Y = cpu.CpuState.A;

            cpu.CpuState.SetZeroFlag(cpu.CpuState.Y);
            cpu.CpuState.SetNegativeFlag(cpu.CpuState.Y);
        }

        private static void Op_Tsx(Cpu cpu, IBus bus)
        {
            cpu.CpuState.X = cpu.CpuState.S;

            cpu.CpuState.SetZeroFlag(cpu.CpuState.X);
            cpu.CpuState.SetNegativeFlag(cpu.CpuState.X);
        }

        private static void Op_Txa(Cpu cpu, IBus bus)
        {
            cpu.CpuState.A = cpu.CpuState.X;

            cpu.CpuState.SetZeroFlag(cpu.CpuState.A);
            cpu.CpuState.SetNegativeFlag(cpu.CpuState.A);
        }

        private static void Op_Txs(Cpu cpu, IBus bus)
        {
            cpu.CpuState.S = cpu.CpuState.X;
        }

        private static void Op_Tya(Cpu cpu, IBus bus)
        {
            cpu.CpuState.A = cpu.CpuState.Y;

            cpu.CpuState.SetZeroFlag(cpu.CpuState.A);
            cpu.CpuState.SetNegativeFlag(cpu.CpuState.A);
        }
    }
}