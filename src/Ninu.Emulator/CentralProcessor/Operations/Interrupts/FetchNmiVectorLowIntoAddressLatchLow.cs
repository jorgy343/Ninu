namespace Ninu.Emulator.CentralProcessor.Operations.Interrupts
{
    public class FetchNmiVectorLowIntoAddressLatchLow : CpuOperation
    {
        private FetchNmiVectorLowIntoAddressLatchLow()
        {

        }

        public static FetchNmiVectorLowIntoAddressLatchLow Singleton { get; } = new();

        public override void Execute(NewCpu cpu, IBus bus)
        {
            cpu.AddressLatchLow = bus.Read(0xfffa);
        }
    }
}