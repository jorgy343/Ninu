namespace Ninu.Emulator.CentralProcessor.Operations
{
    /// <summary>
    /// Fetches the low byte of the address found using the address latches and stores it in the
    /// effective address latche low.
    /// </summary>
    public class FetchEffectiveAddressLow : CpuOperation
    {
        public override void Execute(NewCpu cpu, IBus bus)
        {
            var addressLow = cpu.AddressLatchLow;
            var addressHigh = cpu.AddressLatchHigh;

            var effectiveAddressLow = bus.Read((ushort)(addressLow | (addressHigh << 8)));

            cpu.EffectiveAddressLatchLow = effectiveAddressLow;
        }
    }
}