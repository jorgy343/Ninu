namespace Ninu.Emulator.CentralProcessor.Operations
{
    public class BranchPageCrossed : CpuOperation
    {
        private BranchPageCrossed()
        {

        }

        public static BranchPageCrossed Singleton { get; } = new();

        public override void Execute(NewCpu cpu, IBus bus)
        {
            var baseAddress = (ushort)(cpu.AddressLatchLow | (cpu.AddressLatchHigh << 8));
            var finalAddress = (ushort)((baseAddress + (sbyte)cpu.DataLatch) & 0xffff);

            cpu.EffectiveAddressLatchLow = (byte)(finalAddress & 0xff);
            cpu.EffectiveAddressLatchHigh = (byte)(finalAddress >> 8);
        }
    }
}