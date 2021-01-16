namespace Ninu.Emulator.CentralProcessor
{
    public static partial class Operations2
    {
        public static void FetchInstruction(Cpu cpu, IBus bus)
        {
            if (cpu._nmi && cpu._nmiCycle != cpu._totalCycles - 1)
            {
                cpu.CheckForNmi();
            }
            else
            {
                var instruction = bus.Read(cpu.CpuState.PC);
                cpu.ExecuteInstruction(instruction);
            }
        }

        public static void BranchWithNoPageCrossing(Cpu cpu, IBus bus)
        {
            var baseAddress = (ushort)(cpu.AddressLatchLow | (cpu.AddressLatchHigh << 8));
            var finalAddressNotWrapped = (ushort)((cpu.AddressLatchLow + (sbyte)cpu.DataLatch + (cpu.AddressLatchHigh << 8)) & 0xffff);
            var finalAddressWrapped = (ushort)((((cpu.AddressLatchLow + (sbyte)cpu.DataLatch) & 0xff) | (cpu.AddressLatchHigh << 8)) & 0xffff);

            cpu.EffectiveAddressLatchLow = (byte)(finalAddressWrapped & 0xff);
            cpu.EffectiveAddressLatchHigh = (byte)(finalAddressWrapped >> 8);

            // Check if baseAddress and address are on the same page. If so, we skip the next cycle
            // so perform a dequeue which should be the BranchWithPageCrossed action.
            if ((baseAddress & 0xff00) == (finalAddressNotWrapped & 0xff00))
            {
                cpu.Queue.Dequeue();
            }
        }

        public static void BranchWithPageCrossed(Cpu cpu, IBus bus)
        {
            var baseAddress = (ushort)(cpu.AddressLatchLow | (cpu.AddressLatchHigh << 8));
            var finalAddress = (ushort)((baseAddress + (sbyte)cpu.DataLatch) & 0xffff);

            cpu.EffectiveAddressLatchLow = (byte)(finalAddress & 0xff);
            cpu.EffectiveAddressLatchHigh = (byte)(finalAddress >> 8);
        }

        /// Fetches a byte from memory based on PC and stores it into the CPU's address latch low.
        /// The CPU's address latch high is cleared to zero so the address latch as a whole can be
        /// used for a zero page memory access.
        public static void FetchZeroPageAddressByPCIntoAddressLatch(Cpu cpu, IBus bus)
        {
            cpu.AddressLatchLow = bus.Read(cpu.CpuState.PC);
            cpu.AddressLatchHigh = 0x00;
        }

        /// Fetches a byte from memory based on PC and stores it into the CPU's effective address
        /// latch low. The CPU's effective address latch high is cleared to zero so the effective
        /// address latch as a whole can be used for a zero page memory access.
        public static void FetchZeroPageAddressByPCIntoEffectiveAddressLatch(Cpu cpu, IBus bus)
        {
            cpu.EffectiveAddressLatchLow = bus.Read(cpu.CpuState.PC);
            cpu.EffectiveAddressLatchHigh = 0x00;
        }

        /// This is specifically for the addressing mode <c>absolute with x offset</c>. If the
        /// effective address and the effective address plus the x register is within the same
        /// page, this operation will read the memory at effective address plus the x register and
        /// will dequeue the next operation (which should be <see
        /// cref="FetchForAbsoluteWithXOffsetTry2(Cpu, IBus)"/>). Otherwise, this operation does
        /// nothing.
        public static void FetchForAbsoluteWithXOffsetTry1(Cpu cpu, IBus bus)
        {
            var baseAddress = (ushort)(cpu.EffectiveAddressLatchLow | (cpu.EffectiveAddressLatchHigh << 8));
            var finalAddress = (ushort)((baseAddress + cpu.CpuState.X) & 0xffff); // The & prevents overflow.

            cpu.DataLatch = bus.Read(finalAddress);

            // If the baseAddress and finalAddress are on the same page, we skip the next cycle by
            // dequeueing the operation.
            if ((baseAddress & 0xff00) == (finalAddress & 0xff00))
            {
                cpu.Queue.Dequeue();
            }
        }

        /// <summary>
        /// This operation should only occur if <see cref="FetchForAbsoluteWithXOffsetTry1(Cpu,
        /// IBus)"/> didn't do anything because the base address and the base address plus the x
        /// register were on different pages.
        /// </summary>
        public static void FetchForAbsoluteWithXOffsetTry2(Cpu cpu, IBus bus)
        {
            var baseAddress = (ushort)(cpu.EffectiveAddressLatchLow | (cpu.EffectiveAddressLatchHigh << 8));
            var finalAddress = (ushort)((baseAddress + cpu.CpuState.X) & 0xffff); // The & prevents overflow.

            cpu.DataLatch = bus.Read(finalAddress);
        }

        /// <summary>
        /// This is specifically for the addressing mode <c>absolute with y offset</c>. If the
        /// effective address and the effective address plus the y register is within the same
        /// page, this operation will read the memory at effective address plus the y register and
        /// will dequeue the next operation (which should be <see
        /// cref="FetchForAbsoluteWithYOffsetTry2(Cpu, IBus)"/>). Otherwise, this operation does
        /// nothing.
        /// </summary>
        public static void FetchForAbsoluteWithYOffsetTry1(Cpu cpu, IBus bus)
        {
            var baseAddress = (ushort)(cpu.EffectiveAddressLatchLow | (cpu.EffectiveAddressLatchHigh << 8));
            var finalAddress = (ushort)((baseAddress + cpu.CpuState.Y) & 0xffff); // The & prevents overflow.

            // Check baseAddress and address are on the same page.
            if ((baseAddress & 0xff00) == (finalAddress & 0xff00))
            {
                cpu.DataLatch = bus.Read(finalAddress);

                cpu.Queue.Dequeue();
            }
        }

        /// <summary>
        /// This operation should only occur if <see cref="FetchForAbsoluteWithYOffsetTry1(Cpu,
        /// IBus)"/> didn't do anything because the base address and the base address plus the y
        /// register were on different pages.
        /// </summary>
        public static void FetchForAbsoluteWithYOffsetTry2(Cpu cpu, IBus bus)
        {
            var baseAddress = (ushort)(cpu.EffectiveAddressLatchLow | (cpu.EffectiveAddressLatchHigh << 8));
            var finalAddress = (ushort)((baseAddress + cpu.CpuState.Y) & 0xffff); // The & prevents overflow.

            cpu.DataLatch = bus.Read(finalAddress);
        }
    }
}