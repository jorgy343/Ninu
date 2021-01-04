namespace Ninu.Emulator.CentralProcessor.Operations
{
    /// <summary>
    /// This is specifically for the addressing mode <c>absolute with x offset</c>. If the
    /// effective address and the effective address plus the x register is within the same page,
    /// this operation will read the memory at effective address plus the x register and will
    /// dequeue the next operation (which should be <see cref="FetchForAbsoluteWithXOffsetTry2"/>).
    /// Otherwise, this operation does nothing.
    /// </summary>
    public class FetchForAbsoluteWithXOffsetTry1 : CpuOperation
    {
        private FetchForAbsoluteWithXOffsetTry1()
        {

        }

        public static FetchForAbsoluteWithXOffsetTry1 Singleton { get; } = new();

        public override void Execute(NewCpu cpu, IBus bus)
        {
            var baseAddress = (ushort)(cpu.EffectiveAddressLatchLow | (cpu.EffectiveAddressLatchHigh << 8));
            var finalAddress = (ushort)((baseAddress + cpu.CpuState.X) & 0xffff); // The & prevents overflow.

            // Check baseAddress and address are on the same page.
            if ((baseAddress & 0xff00) == (finalAddress & 0xff00))
            {
                cpu.DataLatch = bus.Read(finalAddress);

                cpu.Queue.Dequeue();
            }
        }
    }
}