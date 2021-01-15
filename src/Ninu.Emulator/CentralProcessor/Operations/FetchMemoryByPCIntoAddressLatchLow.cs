namespace Ninu.Emulator.CentralProcessor.Operations
{
    /// <summary>
    /// Fetches a byte of data from memory using PC as the memory address and stores the byte into
    /// the CPU's address latch low.
    /// </summary>
    public class FetchMemoryByPCIntoAddressLatchLow : CpuOperation
    {
        private FetchMemoryByPCIntoAddressLatchLow()
        {

        }

        public static FetchMemoryByPCIntoAddressLatchLow Singleton { get; } = new();

        public override void Execute(Cpu cpu, IBus bus)
        {
            var addressLow = bus.Read(cpu.CpuState.PC);
            cpu.AddressLatchLow = addressLow;
        }
    }
}