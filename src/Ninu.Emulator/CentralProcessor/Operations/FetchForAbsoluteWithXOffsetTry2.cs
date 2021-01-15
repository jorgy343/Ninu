namespace Ninu.Emulator.CentralProcessor.Operations
{
    /// <summary>
    /// This operation should only occur if <see cref="FetchForAbsoluteWithXOffsetTry1"/> didn't do
    /// anything because the base address and the base address plus the x register were on
    /// different pages.
    /// </summary>
    public class FetchForAbsoluteWithXOffsetTry2 : CpuOperation
    {
        private FetchForAbsoluteWithXOffsetTry2()
        {

        }

        public static FetchForAbsoluteWithXOffsetTry2 Singleton { get; } = new();

        public override void Execute(Cpu cpu, IBus bus)
        {
            var baseAddress = (ushort)(cpu.EffectiveAddressLatchLow | (cpu.EffectiveAddressLatchHigh << 8));
            var finalAddress = (ushort)((baseAddress + cpu.CpuState.X) & 0xffff); // The & prevents overflow.

            cpu.DataLatch = bus.Read(finalAddress);
        }
    }
}