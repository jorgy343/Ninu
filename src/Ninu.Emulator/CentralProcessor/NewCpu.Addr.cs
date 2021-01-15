using Ninu.Emulator.CentralProcessor.Operations;

namespace Ninu.Emulator.CentralProcessor
{
    // This file containsn all of the addressing methods for the NewCpu class. They are separated
    // out from the main class only to cleanup the main class's file.
    //
    // The "WonkFlags" methods are for a few specific instructions that set the flags register P
    // one cycle before setting the A register with the result of the operation.

    public unsafe partial class NewCpu
    {
        private void Addr_Implied(delegate*<NewCpu, IBus, void> action, bool delayedExecution = false)
        {
            AddOperation(Nop.Singleton, true);

            if (delayedExecution)
            {
                AddOperation(FetchInstruction.Singleton, false);
                AddFreeOperation(Nop.Singleton, false, action);
            }
            else
            {
                AddOperation(FetchInstruction.Singleton, false, action);
            }
        }

        private void Addr_Immediate(delegate*<NewCpu, IBus, void> action, bool delayedExecution = false)
        {
            AddOperation(FetchMemoryByPCIntoDataLatch.Singleton, true);

            if (delayedExecution)
            {
                AddOperation(FetchInstruction.Singleton, true);
                AddFreeOperation(Nop.Singleton, false, action);
            }
            else
            {
                AddOperation(FetchInstruction.Singleton, true, action);
            }
        }

        private void Addr_ZeroPage(delegate*<NewCpu, IBus, void> action, bool delayedExecution = false)
        {
            AddOperation(FetchZeroPageAddressByPCIntoEffectiveAddressLatch.Singleton, true);
            AddOperation(FetchMemoryByEffectiveAddressLatchIntoDataLatch.Singleton, true);

            if (delayedExecution)
            {
                AddOperation(FetchInstruction.Singleton, false);
                AddFreeOperation(Nop.Singleton, false, action);
            }
            else
            {
                AddOperation(FetchInstruction.Singleton, false, action);
            }
        }

        private void Addr_ZeroPage_WriteBack(delegate*<NewCpu, IBus, void> action)
        {
            AddOperation(FetchZeroPageAddressByPCIntoEffectiveAddressLatch.Singleton, true);
            AddOperation(FetchMemoryByEffectiveAddressLatchIntoDataLatch.Singleton, true);
            AddFreeOperation(FetchMemoryByEffectiveAddressLatchIntoDataLatch.Singleton, false);
            AddOperation(WriteDataLatchToMemoryByEffectiveAddressLatch.Singleton, false); // Dummy write of the data we just read.
            AddOperation(WriteDataLatchToMemoryByEffectiveAddressLatch.Singleton, false, action);
            AddOperation(FetchInstruction.Singleton, false);
        }

        private void Addr_ZeroPageWithXOffset(delegate*<NewCpu, IBus, void> action, bool delayedExecution = false)
        {
            AddOperation(FetchZeroPageAddressByPCIntoEffectiveAddressLatch.Singleton, true);
            AddOperation(IncrementEffectiveAddressLatchLowByXWithWrapping.Singleton, true);
            AddOperation(FetchMemoryByEffectiveAddressLatchIntoDataLatch.Singleton, false);

            if (delayedExecution)
            {
                AddOperation(FetchInstruction.Singleton, false);
                AddFreeOperation(Nop.Singleton, false, action);
            }
            else
            {
                AddOperation(FetchInstruction.Singleton, false, action);
            }
        }

        private void Addr_ZeroPageWithXOffset_WriteBack(delegate*<NewCpu, IBus, void> action)
        {
            AddOperation(FetchZeroPageAddressByPCIntoEffectiveAddressLatch.Singleton, true);
            AddOperation(IncrementEffectiveAddressLatchLowByXWithWrapping.Singleton, true);
            AddOperation(FetchMemoryByEffectiveAddressLatchIntoDataLatch.Singleton, false);
            AddFreeOperation(FetchMemoryByEffectiveAddressLatchIntoDataLatch.Singleton, false);
            AddOperation(WriteDataLatchToMemoryByEffectiveAddressLatch.Singleton, false); // Dummy write of the data we just read.
            AddOperation(WriteDataLatchToMemoryByEffectiveAddressLatch.Singleton, false, action);
            AddOperation(FetchInstruction.Singleton, false);
        }

        private void Addr_ZeroPageWithYOffset(delegate*<NewCpu, IBus, void> action, bool delayedExecution = false)
        {
            AddOperation(FetchZeroPageAddressByPCIntoEffectiveAddressLatch.Singleton, true);
            AddOperation(IncrementEffectiveAddressLatchLowByYWithWrapping.Singleton, true);
            AddOperation(FetchMemoryByEffectiveAddressLatchIntoDataLatch.Singleton, false);

            if (delayedExecution)
            {
                AddOperation(FetchInstruction.Singleton, false);
                AddFreeOperation(Nop.Singleton, false, action);
            }
            else
            {
                AddOperation(FetchInstruction.Singleton, false, action);
            }
        }

        private void Addr_Absolute(delegate*<NewCpu, IBus, void> action, bool delayedExecution = false)
        {
            AddOperation(FetchMemoryByPCIntoEffectiveAddressLatchLow.Singleton, true);
            AddOperation(FetchMemoryByPCIntoEffectiveAddressLatchHigh.Singleton, true);
            AddOperation(FetchMemoryByEffectiveAddressLatchIntoDataLatch.Singleton, true);

            if (delayedExecution)
            {
                AddOperation(FetchInstruction.Singleton, false);
                AddFreeOperation(Nop.Singleton, false, action);
            }
            else
            {
                AddOperation(FetchInstruction.Singleton, false, action);
            }
        }

        private void Addr_Absolute_WriteBack(delegate*<NewCpu, IBus, void> action)
        {
            AddOperation(FetchMemoryByPCIntoEffectiveAddressLatchLow.Singleton, true);
            AddOperation(FetchMemoryByPCIntoEffectiveAddressLatchHigh.Singleton, true);
            AddOperation(FetchMemoryByEffectiveAddressLatchIntoDataLatch.Singleton, true);
            AddFreeOperation(FetchMemoryByEffectiveAddressLatchIntoDataLatch.Singleton, false);
            AddOperation(WriteDataLatchToMemoryByEffectiveAddressLatch.Singleton, false); // Dummy write of the data we just read.
            AddOperation(WriteDataLatchToMemoryByEffectiveAddressLatch.Singleton, false, action);
            AddOperation(FetchInstruction.Singleton, false);
        }

        private void Addr_AbsoluteWithXOffset(delegate*<NewCpu, IBus, void> action, bool delayedExecution = false)
        {
            AddOperation(FetchMemoryByPCIntoEffectiveAddressLatchLow.Singleton, true);
            AddOperation(FetchMemoryByPCIntoEffectiveAddressLatchHigh.Singleton, true);
            AddOperation(FetchForAbsoluteWithXOffsetTry1.Singleton, true);
            AddOperation(FetchForAbsoluteWithXOffsetTry2.Singleton, false);

            if (delayedExecution)
            {
                AddOperation(FetchInstruction.Singleton, false);
                AddFreeOperation(Nop.Singleton, false, action);
            }
            else
            {
                AddOperation(FetchInstruction.Singleton, false, action);
            }
        }

        private void Addr_AbsoluteWithXOffset_WriteBack(delegate*<NewCpu, IBus, void> action)
        {
            AddOperation(FetchMemoryByPCIntoEffectiveAddressLatchLow.Singleton, true);
            AddOperation(FetchMemoryByPCIntoEffectiveAddressLatchHigh.Singleton, true);
            AddOperation(IncrementEffectiveAddressLatchLowByXWithWrapping.Singleton, true);
            AddOperation(IncrementEffectiveAddressLatchHighByXOnlyWithCarry.Singleton, false);
            AddFreeOperation(FetchMemoryByEffectiveAddressLatchIntoDataLatch.Singleton, false);
            AddOperation(WriteDataLatchToMemoryByEffectiveAddressLatch.Singleton, false); // Dummy write of the data we just read.
            AddOperation(WriteDataLatchToMemoryByEffectiveAddressLatch.Singleton, false, action);
            AddOperation(FetchInstruction.Singleton, false);
        }

        private void Addr_AbsoluteWithYOffset(delegate*<NewCpu, IBus, void> action, bool delayedExecution = false)
        {
            AddOperation(FetchMemoryByPCIntoEffectiveAddressLatchLow.Singleton, true);
            AddOperation(FetchMemoryByPCIntoEffectiveAddressLatchHigh.Singleton, true);
            AddOperation(FetchForAbsoluteWithYOffsetTry1.Singleton, true);
            AddOperation(FetchForAbsoluteWithYOffsetTry2.Singleton, false);

            if (delayedExecution)
            {
                AddOperation(FetchInstruction.Singleton, false);
                AddFreeOperation(Nop.Singleton, false, action);
            }
            else
            {
                AddOperation(FetchInstruction.Singleton, false, action);
            }
        }

        private void Addr_IndirectZeroPageWithXOffset(delegate*<NewCpu, IBus, void> action, bool delayedExecution = false)
        {
            AddOperation(FetchZeroPageAddressByPCIntoAddressLatch.Singleton, true);
            AddOperation(IncrementAddressLatchLowByXWithWrapping.Singleton, true);
            AddOperation(FetchMemoryByAddressLatchIntoEffectiveAddressLatchLow.Singleton, false);
            AddOperation(FetchMemoryByAddressLatchIntoEffectiveAddressLatchHighWithWrapping.Singleton, false);
            AddOperation(FetchMemoryByEffectiveAddressLatchIntoDataLatch.Singleton, false);

            if (delayedExecution)
            {
                AddOperation(FetchInstruction.Singleton, false);
                AddFreeOperation(Nop.Singleton, false, action);
            }
            else
            {
                AddOperation(FetchInstruction.Singleton, false, action);
            }
        }

        private void Addr_IndirectZeroPageWithYOffset(delegate*<NewCpu, IBus, void> action, bool delayedExecution = false)
        {
            AddOperation(FetchZeroPageAddressByPCIntoAddressLatch.Singleton, true);
            AddOperation(FetchMemoryByAddressLatchIntoEffectiveAddressLatchLow.Singleton, true);
            AddOperation(FetchMemoryByAddressLatchIntoEffectiveAddressLatchHighWithWrapping.Singleton, false);
            AddOperation(FetchForAbsoluteWithYOffsetTry1.Singleton, false);
            AddOperation(FetchForAbsoluteWithYOffsetTry2.Singleton, false);

            if (delayedExecution)
            {
                AddOperation(FetchInstruction.Singleton, false);
                AddFreeOperation(Nop.Singleton, false, action);
            }
            else
            {
                AddOperation(FetchInstruction.Singleton, false, action);
            }
        }

        private void Addr_Relative(delegate*<NewCpu, IBus, void> action)
        {
            AddOperation(FetchMemoryByPCIntoDataLatch.Singleton, true, postAction: action);
            AddOperation(BranchNoPageCrossing.Singleton, true);
            AddOperation(BranchPageCrossed.Singleton, false, &SetPCToEffectiveAddressLatch);
            AddOperation(FetchInstruction.Singleton, false, &SetPCToEffectiveAddressLatch);
        }
    }
}