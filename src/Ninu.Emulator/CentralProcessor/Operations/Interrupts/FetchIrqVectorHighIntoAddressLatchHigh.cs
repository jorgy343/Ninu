namespace Ninu.Emulator.CentralProcessor.Operations.Interrupts
{
    public class FetchIrqVectorHighIntoAddressLatchHigh : CpuOperation
    {
        private FetchIrqVectorHighIntoAddressLatchHigh()
        {

        }

        public static FetchIrqVectorHighIntoAddressLatchHigh Singleton { get; } = new();

        public override void Execute(NewCpu cpu, IBus bus)
        {
            cpu.AddressLatchHigh = bus.Read(0xffff);
        }
    }
}