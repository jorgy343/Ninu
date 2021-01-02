namespace Ninu.Emulator.CentralProcessor.Operations
{
    /// <summary>
    /// Fetches the low byte of the address found using the address latches and stores it in the
    /// effective address latche low.
    /// </summary>
    public class FetchMemoryByAddressLatchIntoEffectiveAddressLatchLow : CpuOperation
    {
        private FetchMemoryByAddressLatchIntoEffectiveAddressLatchLow()
        {

        }

        public static FetchMemoryByAddressLatchIntoEffectiveAddressLatchLow Singleton { get; } = new();

        public override void Execute(NewCpu cpu, IBus bus)
        {
            var addressLow = cpu.AddressLatchLow;
            var addressHigh = cpu.AddressLatchHigh;

            var effectiveAddressLow = bus.Read((ushort)(addressLow | (addressHigh << 8)));

            cpu.EffectiveAddressLatchLow = effectiveAddressLow;
        }
    }
}