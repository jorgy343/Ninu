namespace Ninu.Emulator.CentralProcessor.Operations.Interrupts
{
    public class FetchNmiVectorHighIntoAddressLatchHigh : CpuOperation
    {
        private FetchNmiVectorHighIntoAddressLatchHigh()
        {

        }

        public static FetchNmiVectorHighIntoAddressLatchHigh Singleton { get; } = new();

        public override void Execute(NewCpu cpu, IBus bus)
        {
            cpu.AddressLatchHigh = bus.Read(0xfffb);
        }
    }
}