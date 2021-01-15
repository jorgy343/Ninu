namespace Ninu.Emulator.CentralProcessor.Operations
{
    public class BranchNoPageCrossing : CpuOperation
    {
        private BranchNoPageCrossing()
        {

        }

        public static BranchNoPageCrossing Singleton { get; } = new();

        public override void Execute(Cpu cpu, IBus bus)
        {
            var baseAddress = (ushort)(cpu.AddressLatchLow | (cpu.AddressLatchHigh << 8));
            var finalAddressNotWrapped = (ushort)((cpu.AddressLatchLow + (sbyte)cpu.DataLatch + (cpu.AddressLatchHigh << 8)) & 0xffff);
            var finalAddressWrapped = (ushort)((((cpu.AddressLatchLow + (sbyte)cpu.DataLatch) & 0xff) | (cpu.AddressLatchHigh << 8)) & 0xffff);

            cpu.EffectiveAddressLatchLow = (byte)(finalAddressWrapped & 0xff);
            cpu.EffectiveAddressLatchHigh = (byte)(finalAddressWrapped >> 8);

            // Check baseAddress and address are on the same page.
            if ((baseAddress & 0xff00) == (finalAddressNotWrapped & 0xff00))
            {
                cpu.Queue.Dequeue();
            }
        }
    }
}