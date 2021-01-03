namespace Ninu.Emulator.CentralProcessor.Operations
{
    public class IncrementEffectiveAddressLatchLowByYWithoutWrapping : CpuOperation
    {
        private IncrementEffectiveAddressLatchLowByYWithoutWrapping()
        {

        }

        public static IncrementEffectiveAddressLatchLowByYWithoutWrapping Singleton { get; } = new();

        public override void Execute(NewCpu cpu, IBus bus)
        {
            var address = (ushort)(cpu.EffectiveAddressLatchLow | (cpu.EffectiveAddressLatchHigh << 8));
            address = (ushort)((address + cpu.CpuState.Y) & 0xffff); // Wrap addresses around 64KiB.

            cpu.EffectiveAddressLatchLow = (byte)(address & 0xff);
            cpu.EffectiveAddressLatchHigh = (byte)(address >> 8);
        }
    }
}