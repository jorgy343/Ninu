namespace Ninu.Emulator.CentralProcessor.Operations
{
    /// <summary>
    /// Fetches a byte of data from memory using PC as the memory address and stores the byte into
    /// the CPU's address latch high.
    /// </summary>
    public class FetchMemoryByPCIntoEffectiveAddressLatchHigh : CpuOperation
    {
        private FetchMemoryByPCIntoEffectiveAddressLatchHigh()
        {

        }

        public static FetchMemoryByPCIntoEffectiveAddressLatchHigh Singleton { get; } = new();

        public override void Execute(NewCpu cpu, IBus bus)
        {
            var addressHigh = bus.Read(cpu.CpuState.PC);
            cpu.EffectiveAddressLatchHigh = addressHigh;
        }
    }
}