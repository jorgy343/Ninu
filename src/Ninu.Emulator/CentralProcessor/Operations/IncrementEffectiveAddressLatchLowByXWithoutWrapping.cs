namespace Ninu.Emulator.CentralProcessor.Operations
{
    public class IncrementEffectiveAddressLatchLowByXWithoutWrapping : CpuOperation
    {
        private IncrementEffectiveAddressLatchLowByXWithoutWrapping()
        {

        }

        public static IncrementEffectiveAddressLatchLowByXWithoutWrapping Singleton { get; } = new();

        public override void Execute(Cpu cpu, IBus bus)
        {
            var address = (ushort)(cpu.EffectiveAddressLatchLow | (cpu.EffectiveAddressLatchHigh << 8));
            address = (ushort)((address + cpu.CpuState.X) & 0xffff); // Wrap addresses around 64KiB.

            cpu.EffectiveAddressLatchLow = (byte)(address & 0xff);
            cpu.EffectiveAddressLatchHigh = (byte)(address >> 8);
        }
    }
}