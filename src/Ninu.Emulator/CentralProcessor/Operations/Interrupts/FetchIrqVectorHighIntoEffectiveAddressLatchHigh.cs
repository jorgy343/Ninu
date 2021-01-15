namespace Ninu.Emulator.CentralProcessor.Operations.Interrupts
{
    public class FetchIrqVectorHighIntoEffectiveAddressLatchHigh : CpuOperation
    {
        private FetchIrqVectorHighIntoEffectiveAddressLatchHigh()
        {

        }

        public static FetchIrqVectorHighIntoEffectiveAddressLatchHigh Singleton { get; } = new();

        public override void Execute(Cpu cpu, IBus bus)
        {
            cpu.EffectiveAddressLatchHigh = bus.Read(0xffff);
        }
    }
}