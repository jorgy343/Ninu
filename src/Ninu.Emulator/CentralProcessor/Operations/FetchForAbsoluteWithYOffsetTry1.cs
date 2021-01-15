namespace Ninu.Emulator.CentralProcessor.Operations
{
    /// <summary>
    /// This is specifically for the addressing mode <c>absolute with y offset</c>. If the
    /// effective address and the effective address plus the y register is within the same page,
    /// this operation will read the memory at effective address plus the y register and will
    /// dequeue the next operation (which should be <see cref="FetchForAbsoluteWithYOffsetTry2"/>).
    /// Otherwise, this operation does nothing.
    /// </summary>
    public class FetchForAbsoluteWithYOffsetTry1 : CpuOperation
    {
        private FetchForAbsoluteWithYOffsetTry1()
        {

        }

        public static FetchForAbsoluteWithYOffsetTry1 Singleton { get; } = new();

        public override void Execute(Cpu cpu, IBus bus)
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
    }
}