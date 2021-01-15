namespace Ninu.Emulator.CentralProcessor.Operations
{
    /// <summary>
    /// Writes the X register to memory at the address defined by the effective address latches
    /// <see cref="Cpu.EffectiveAddressLatchLow"/> and <see
    /// cref="Cpu.EffectiveAddressLatchHigh"/>.
    /// </summary>
    public class WriteXToMemoryByEffectiveAddressLatch : CpuOperation
    {
        private WriteXToMemoryByEffectiveAddressLatch()
        {

        }

        public static WriteXToMemoryByEffectiveAddressLatch Singleton { get; } = new();

        public override void Execute(Cpu cpu, IBus bus)
        {
            var address = (ushort)(cpu.EffectiveAddressLatchLow | (cpu.EffectiveAddressLatchHigh << 8));
            bus.Write(address, cpu.CpuState.X);
        }
    }
}