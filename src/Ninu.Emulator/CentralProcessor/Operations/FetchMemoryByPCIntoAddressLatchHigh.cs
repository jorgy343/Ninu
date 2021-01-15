namespace Ninu.Emulator.CentralProcessor.Operations
{
    /// <summary>
    /// Fetches a byte of data from memory using PC as the memory address and stores the byte into
    /// the CPU's address latch high.
    /// </summary>
    public class FetchMemoryByPCIntoAddressLatchHigh : CpuOperation
    {
        private FetchMemoryByPCIntoAddressLatchHigh()
        {

        }

        public static FetchMemoryByPCIntoAddressLatchHigh Singleton { get; } = new();

        public override void Execute(Cpu cpu, IBus bus)
        {
            var addressHigh = bus.Read(cpu.CpuState.PC);
            cpu.AddressLatchHigh = addressHigh;
        }
    }
}