namespace Ninu.Emulator.CentralProcessor.Operations
{
    public class IncrementEffectiveAddressLatchHighByXOnlyWithCarry : CpuOperation
    {
        private IncrementEffectiveAddressLatchHighByXOnlyWithCarry()
        {

        }

        public static IncrementEffectiveAddressLatchHighByXOnlyWithCarry Singleton { get; } = new();

        public override void Execute(Cpu cpu, IBus bus)
        {
            var result = cpu.EffectiveAddressLatchLow + cpu.CpuState.X;
            var increment = result >> 8; // This will shift the carry bit to the LSB.

            cpu.EffectiveAddressLatchHigh += (byte)increment;
        }
    }
}