namespace Ninu.Emulator.CentralProcessor.Operations
{
    public class IncrementEffectiveAddressLatchLowByYWithWrapping : CpuOperation
    {
        private IncrementEffectiveAddressLatchLowByYWithWrapping()
        {

        }

        public static IncrementEffectiveAddressLatchLowByYWithWrapping Singleton { get; } = new();

        public override void Execute(NewCpu cpu, IBus bus)
        {
            cpu.EffectiveAddressLatchLow += cpu.CpuState.Y;
        }
    }
}