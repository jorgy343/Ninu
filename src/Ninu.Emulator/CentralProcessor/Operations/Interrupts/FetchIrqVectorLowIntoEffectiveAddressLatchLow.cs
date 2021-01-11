namespace Ninu.Emulator.CentralProcessor.Operations.Interrupts
{
    public class FetchIrqVectorLowIntoEffectiveAddressLatchLow : CpuOperation
    {
        private FetchIrqVectorLowIntoEffectiveAddressLatchLow()
        {

        }

        public static FetchIrqVectorLowIntoEffectiveAddressLatchLow Singleton { get; } = new();

        public override void Execute(NewCpu cpu, IBus bus)
        {
            cpu.EffectiveAddressLatchLow = bus.Read(0xfffe);
        }
    }
}