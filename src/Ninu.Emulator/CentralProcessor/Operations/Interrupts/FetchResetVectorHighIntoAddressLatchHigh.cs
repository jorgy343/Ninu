namespace Ninu.Emulator.CentralProcessor.Operations.Interrupts
{
    public class FetchResetVectorHighIntoAddressLatchHigh : CpuOperation
    {
        private FetchResetVectorHighIntoAddressLatchHigh()
        {

        }

        public static FetchResetVectorHighIntoAddressLatchHigh Singleton { get; } = new();

        public override void Execute(Cpu cpu, IBus bus)
        {
            cpu.AddressLatchHigh = bus.Read(0xfffd);
        }
    }
}