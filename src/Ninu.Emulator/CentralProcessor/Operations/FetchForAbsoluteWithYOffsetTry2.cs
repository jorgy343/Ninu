namespace Ninu.Emulator.CentralProcessor.Operations
{
    /// <summary>
    /// This operation should only occur if <see cref="FetchForAbsoluteWithYOffsetTry1"/> didn't do
    /// anything because the base address and the base address plus the y register were on
    /// different pages.
    /// </summary>
    public class FetchForAbsoluteWithYOffsetTry2 : CpuOperation
    {
        private FetchForAbsoluteWithYOffsetTry2()
        {

        }

        public static FetchForAbsoluteWithYOffsetTry2 Singleton { get; } = new();

        public override void Execute(Cpu cpu, IBus bus)
        {
            var baseAddress = (ushort)(cpu.EffectiveAddressLatchLow | (cpu.EffectiveAddressLatchHigh << 8));
            var finalAddress = (ushort)((baseAddress + cpu.CpuState.Y) & 0xffff); // The & prevents overflow.

            cpu.DataLatch = bus.Read(finalAddress);
        }
    }
}