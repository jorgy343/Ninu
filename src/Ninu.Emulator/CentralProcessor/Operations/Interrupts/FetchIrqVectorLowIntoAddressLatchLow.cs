namespace Ninu.Emulator.CentralProcessor.Operations.Interrupts
{
    public class FetchIrqVectorLowIntoAddressLatchLow : CpuOperation
    {
        private FetchIrqVectorLowIntoAddressLatchLow()
        {

        }

        public static FetchIrqVectorLowIntoAddressLatchLow Singleton { get; } = new();

        public override void Execute(NewCpu cpu, IBus bus)
        {
            cpu.AddressLatchLow = bus.Read(0xfffe);
        }
    }
}