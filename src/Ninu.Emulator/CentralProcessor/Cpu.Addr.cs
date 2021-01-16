namespace Ninu.Emulator.CentralProcessor
{
    // This file containsn all of the addressing methods for the NewCpu class. They are separated
    // out from the main class only to cleanup the main class's file.
    //
    // The "WonkFlags" methods are for a few specific instructions that set the flags register P
    // one cycle before setting the A register with the result of the operation.

    public unsafe partial class Cpu
    {
        private void Addr_Implied(delegate*<Cpu, IBus, void> action, bool delayedExecution = false)
        {
            AddOperation(true);

            if (delayedExecution)
            {
                AddOperation(false, &Operations.FetchInstruction);
                AddFreeOperation(false, action);
            }
            else
            {
                AddOperation(false, action, &Operations.FetchInstruction);
            }
        }

        private void Addr_Immediate(delegate*<Cpu, IBus, void> action, bool delayedExecution = false)
        {
            AddOperation(true, &Operations.ReadMemory.ByPC.IntoData);

            if (delayedExecution)
            {
                AddOperation(true, &Operations.FetchInstruction);
                AddFreeOperation(false, action);
            }
            else
            {
                AddOperation(true, action, &Operations.FetchInstruction);
            }
        }

        private void Addr_ZeroPage(delegate*<Cpu, IBus, void> action, bool delayedExecution = false)
        {
            AddOperation(true, &Operations.FetchZeroPageAddressByPCIntoEffectiveAddressLatch);
            AddOperation(true, &Operations.ReadMemory.ByEffectiveAddress.IntoData);

            if (delayedExecution)
            {
                AddOperation(false, &Operations.FetchInstruction);
                AddFreeOperation(false, action);
            }
            else
            {
                AddOperation(false, action, &Operations.FetchInstruction);
            }
        }

        private void Addr_ZeroPage_WriteBack(delegate*<Cpu, IBus, void> action)
        {
            AddOperation(true, &Operations.FetchZeroPageAddressByPCIntoEffectiveAddressLatch);
            AddOperation(true, &Operations.ReadMemory.ByEffectiveAddress.IntoData);
            AddFreeOperation(false, &Operations.ReadMemory.ByEffectiveAddress.IntoData); // TODO: Is this needed?
            AddOperation(false, &Operations.WriteData.ToMemory.ByEffectiveAddress); // Dummy write of the data we just read.
            AddOperation(false, action, &Operations.WriteData.ToMemory.ByEffectiveAddress);
            AddOperation(false, &Operations.FetchInstruction);
        }

        private void Addr_ZeroPageWithXOffset(delegate*<Cpu, IBus, void> action, bool delayedExecution = false)
        {
            AddOperation(true, &Operations.FetchZeroPageAddressByPCIntoEffectiveAddressLatch);
            AddOperation(true, &Operations.Increment.EffectiveaddressLow.ByX.WithWrapping);
            AddOperation(false, &Operations.ReadMemory.ByEffectiveAddress.IntoData);

            if (delayedExecution)
            {
                AddOperation(false, &Operations.FetchInstruction);
                AddFreeOperation(false, action);
            }
            else
            {
                AddOperation(false, action, &Operations.FetchInstruction);
            }
        }

        private void Addr_ZeroPageWithXOffset_WriteBack(delegate*<Cpu, IBus, void> action)
        {
            AddOperation(true, &Operations.FetchZeroPageAddressByPCIntoEffectiveAddressLatch);
            AddOperation(true, &Operations.Increment.EffectiveaddressLow.ByX.WithWrapping);
            AddOperation(false, &Operations.ReadMemory.ByEffectiveAddress.IntoData);
            AddFreeOperation(false, &Operations.ReadMemory.ByEffectiveAddress.IntoData); // TODO: Do we need this?
            AddOperation(false, &Operations.WriteData.ToMemory.ByEffectiveAddress); // Dummy write of the data we just read.
            AddOperation(false, action, &Operations.WriteData.ToMemory.ByEffectiveAddress);
            AddOperation(false, &Operations.FetchInstruction);
        }

        private void Addr_ZeroPageWithYOffset(delegate*<Cpu, IBus, void> action, bool delayedExecution = false)
        {
            AddOperation(true, &Operations.FetchZeroPageAddressByPCIntoEffectiveAddressLatch);
            AddOperation(true, &Operations.Increment.EffectiveaddressLow.ByY.WithWrapping);
            AddOperation(false, &Operations.ReadMemory.ByEffectiveAddress.IntoData);

            if (delayedExecution)
            {
                AddOperation(false, &Operations.FetchInstruction);
                AddFreeOperation(false, action);
            }
            else
            {
                AddOperation(false, action, &Operations.FetchInstruction);
            }
        }

        private void Addr_Absolute(delegate*<Cpu, IBus, void> action, bool delayedExecution = false)
        {
            AddOperation(true, &Operations.ReadMemory.ByPC.IntoEffectiveAddressLow);
            AddOperation(true, &Operations.ReadMemory.ByPC.IntoEffectiveAddressHigh);
            AddOperation(true, &Operations.ReadMemory.ByEffectiveAddress.IntoData);

            if (delayedExecution)
            {
                AddOperation(false, &Operations.FetchInstruction);
                AddFreeOperation(false, action);
            }
            else
            {
                AddOperation(false, action, &Operations.FetchInstruction);
            }
        }

        private void Addr_Absolute_WriteBack(delegate*<Cpu, IBus, void> action)
        {
            AddOperation(true, &Operations.ReadMemory.ByPC.IntoEffectiveAddressLow);
            AddOperation(true, &Operations.ReadMemory.ByPC.IntoEffectiveAddressHigh);
            AddOperation(true, &Operations.ReadMemory.ByEffectiveAddress.IntoData);
            AddFreeOperation(false, &Operations.ReadMemory.ByEffectiveAddress.IntoData); // TODO: Is this needed?
            AddOperation(false, &Operations.WriteData.ToMemory.ByEffectiveAddress); // Dummy write of the data we just read.
            AddOperation(false, action, &Operations.WriteData.ToMemory.ByEffectiveAddress);
            AddOperation(false, &Operations.FetchInstruction);
        }

        private void Addr_AbsoluteWithXOffset(delegate*<Cpu, IBus, void> action, bool delayedExecution = false)
        {
            AddOperation(true, &Operations.ReadMemory.ByPC.IntoEffectiveAddressLow);
            AddOperation(true, &Operations.ReadMemory.ByPC.IntoEffectiveAddressHigh);
            AddOperation(true, &Operations.FetchForAbsoluteWithXOffsetTry1);
            AddOperation(false, &Operations.FetchForAbsoluteWithXOffsetTry2);

            if (delayedExecution)
            {
                AddOperation(false, &Operations.FetchInstruction);
                AddFreeOperation(false, action);
            }
            else
            {
                AddOperation(false, action, &Operations.FetchInstruction);
            }
        }

        private void Addr_AbsoluteWithXOffset_WriteBack(delegate*<Cpu, IBus, void> action)
        {
            AddOperation(true, &Operations.ReadMemory.ByPC.IntoEffectiveAddressLow);
            AddOperation(true, &Operations.ReadMemory.ByPC.IntoEffectiveAddressHigh);
            AddOperation(true, &Operations.Increment.EffectiveaddressLow.ByX.WithWrapping);
            AddOperation(false, &Operations.Increment.EffectiveAddressHigh.ByX.OnlyWithCarry);
            AddFreeOperation(false, &Operations.ReadMemory.ByEffectiveAddress.IntoData); // TODO: Is this needed?
            AddOperation(false, &Operations.WriteData.ToMemory.ByEffectiveAddress); // Dummy write of the data we just read.
            AddOperation(false, action, &Operations.WriteData.ToMemory.ByEffectiveAddress);
            AddOperation(false, &Operations.FetchInstruction);
        }

        private void Addr_AbsoluteWithYOffset(delegate*<Cpu, IBus, void> action, bool delayedExecution = false)
        {
            AddOperation(true, &Operations.ReadMemory.ByPC.IntoEffectiveAddressLow);
            AddOperation(true, &Operations.ReadMemory.ByPC.IntoEffectiveAddressHigh);
            AddOperation(true, &Operations.FetchForAbsoluteWithYOffsetTry1);
            AddOperation(false, &Operations.FetchForAbsoluteWithYOffsetTry2);

            if (delayedExecution)
            {
                AddOperation(false, &Operations.FetchInstruction);
                AddFreeOperation(false, action);
            }
            else
            {
                AddOperation(false, action, &Operations.FetchInstruction);
            }
        }

        private void Addr_IndirectZeroPageWithXOffset(delegate*<Cpu, IBus, void> action, bool delayedExecution = false)
        {
            AddOperation(true, &Operations.FetchZeroPageAddressByPCIntoAddressLatch);
            AddOperation(true, &Operations.Increment.AddressLow.ByX.WithWrapping);
            AddOperation(false, &Operations.ReadMemory.ByAddress.IntoEffectiveAddressLow);
            AddOperation(false, &Operations.ReadMemory.ByAddress.IntoEffectiveAddressHigh.WithWrapping);
            AddOperation(false, &Operations.ReadMemory.ByEffectiveAddress.IntoData);

            if (delayedExecution)
            {
                AddOperation(false, &Operations.FetchInstruction);
                AddFreeOperation(false, action);
            }
            else
            {
                AddOperation(false, action, &Operations.FetchInstruction);
            }
        }

        private void Addr_IndirectZeroPageWithYOffset(delegate*<Cpu, IBus, void> action, bool delayedExecution = false)
        {
            AddOperation(true, &Operations.FetchZeroPageAddressByPCIntoAddressLatch);
            AddOperation(true, &Operations.ReadMemory.ByAddress.IntoEffectiveAddressLow);
            AddOperation(false, &Operations.ReadMemory.ByAddress.IntoEffectiveAddressHigh.WithWrapping);
            AddOperation(false, &Operations.FetchForAbsoluteWithYOffsetTry1);
            AddOperation(false, &Operations.FetchForAbsoluteWithYOffsetTry2);

            if (delayedExecution)
            {
                AddOperation(false, &Operations.FetchInstruction);
                AddFreeOperation(false, action);
            }
            else
            {
                AddOperation(false, action, &Operations.FetchInstruction);
            }
        }

        private void Addr_Relative(delegate*<Cpu, IBus, void> action)
        {
            AddOperation(true, &Operations.ReadMemory.ByPC.IntoData, action);
            AddOperation(true, &Operations.BranchWithNoPageCrossing);
            AddOperation(false, &SetPCToEffectiveAddressLatch, &Operations.BranchWithPageCrossed);
            AddOperation(false, &SetPCToEffectiveAddressLatch, &Operations.FetchInstruction);
        }
    }
}