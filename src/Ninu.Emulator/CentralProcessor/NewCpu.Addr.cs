using Ninu.Emulator.CentralProcessor.Operations;
using System;

namespace Ninu.Emulator.CentralProcessor
{
    // This file containsn all of the addressing methods for the NewCpu class. They are separated
    // out from the main class only to cleanup the main class's file.
    //
    // The "WonkFlags" methods are for a few specific instructions that set the flags register P
    // one cycle before setting the A register with the result of the operation.

    public partial class NewCpu
    {
        private void Addr_Implied(Action? action, bool delayedExecution = false)
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

        private void Addr_Immediate(Action action, bool delayedExecution = false)
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

        private void Addr_ZeroPage(Action action, bool delayedExecution = false)
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

        private void Addr_ZeroPageWithXOffset(Action action, bool delayedExecution = false)
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

        private void Addr_ZeroPageWithYOffset(Action action, bool delayedExecution = false)
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

        private void Addr_Absolute(Action action, bool delayedExecution = false)
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

        private void Addr_AbsoluteWithXOffset(Action action, bool delayedExecution = false)
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

        private void Addr_AbsoluteWithYOffset(Action action, bool delayedExecution = false)
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

        private void Addr_IndirectZeroPageWithXOffset(Action action, bool delayedExecution = false)
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

        private void Addr_IndirectZeroPageWithYOffset(Action action, bool delayedExecution = false)
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

        private void Addr_Relative(Action action)
        {
            AddOperation(FetchMemoryByPCIntoDataLatch.Singleton, true, postAction: action);
            AddOperation(BranchNoPageCrossing.Singleton, true);
            AddOperation(BranchPageCrossed.Singleton, false, SetPCToEffectiveAddressLatch);
            AddOperation(FetchInstruction.Singleton, false, SetPCToEffectiveAddressLatch);
        }
    }
}