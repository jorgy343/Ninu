namespace Ninu.Emulator.CentralProcessor.Operations
{
    public class IncrementAddressLatchLowByXWithWrapping : CpuOperation
    {
        private IncrementAddressLatchLowByXWithWrapping()
        {

        }

        public static IncrementAddressLatchLowByXWithWrapping Singleton { get; } = new();

        public override void Execute(Cpu cpu, IBus bus)
        {
            cpu.AddressLatchLow += cpu.CpuState.X;
        }
    }
}