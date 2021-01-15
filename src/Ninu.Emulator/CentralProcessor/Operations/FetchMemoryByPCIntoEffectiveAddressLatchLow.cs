namespace Ninu.Emulator.CentralProcessor.Operations
{
    /// <summary>
    /// Fetches a byte of data from memory using PC as the memory address and stores the byte into
    /// the CPU's address latch low.
    /// </summary>
    public class FetchMemoryByPCIntoEffectiveAddressLatchLow : CpuOperation
    {
        private FetchMemoryByPCIntoEffectiveAddressLatchLow()
        {

        }

        public static FetchMemoryByPCIntoEffectiveAddressLatchLow Singleton { get; } = new();

        public override void Execute(Cpu cpu, IBus bus)
        {
            var addressLow = bus.Read(cpu.CpuState.PC);
            cpu.EffectiveAddressLatchLow = addressLow;
        }
    }
}