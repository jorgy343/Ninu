namespace Ninu.Emulator.CentralProcessor.Operations
{
    public class IncrementEffectiveAddressLatchLowByXWithWrapping : CpuOperation
    {
        private IncrementEffectiveAddressLatchLowByXWithWrapping()
        {

        }

        public static IncrementEffectiveAddressLatchLowByXWithWrapping Singleton { get; } = new();

        public override void Execute(NewCpu cpu, IBus bus)
        {
            cpu.EffectiveAddressLatchLow += cpu.CpuState.X;
        }
    }
}