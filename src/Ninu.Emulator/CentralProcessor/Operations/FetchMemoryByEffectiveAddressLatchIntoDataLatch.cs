namespace Ninu.Emulator.CentralProcessor.Operations
{
    public class FetchMemoryByEffectiveAddressLatchIntoDataLatch : CpuOperation
    {
        private FetchMemoryByEffectiveAddressLatchIntoDataLatch()
        {

        }

        public static FetchMemoryByEffectiveAddressLatchIntoDataLatch Singleton { get; } = new();

        public override void Execute(NewCpu cpu, IBus bus)
        {
            var address = (ushort)(cpu.EffectiveAddressLatchLow | (cpu.EffectiveAddressLatchHigh << 8));

            var data = bus.Read(address);
            cpu.DataLatch = data;
        }
    }
}