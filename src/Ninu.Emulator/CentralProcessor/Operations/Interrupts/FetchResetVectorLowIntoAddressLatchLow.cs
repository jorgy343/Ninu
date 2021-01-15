namespace Ninu.Emulator.CentralProcessor.Operations.Interrupts
{
    public class FetchResetVectorLowIntoAddressLatchLow : CpuOperation
    {
        private FetchResetVectorLowIntoAddressLatchLow()
        {

        }

        public static FetchResetVectorLowIntoAddressLatchLow Singleton { get; } = new();

        public override void Execute(Cpu cpu, IBus bus)
        {
            cpu.AddressLatchLow = bus.Read(0xfffc);
        }
    }
}