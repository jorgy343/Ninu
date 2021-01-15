namespace Ninu.Emulator.CentralProcessor.Operations
{
    /// <summary>
    /// Fetches a byte from memory based on PC and stores it into the CPU's address latch low. The
    /// CPU's address latch high is cleared to zero so the address latch as a whole can be used for
    /// a zero page memory access.
    /// </summary>
    public class FetchZeroPageAddressByPCIntoAddressLatch : CpuOperation
    {
        private FetchZeroPageAddressByPCIntoAddressLatch()
        {

        }

        public static FetchZeroPageAddressByPCIntoAddressLatch Singleton { get; } = new();

        public override void Execute(Cpu cpu, IBus bus)
        {
            cpu.AddressLatchLow = bus.Read(cpu.CpuState.PC);
            cpu.AddressLatchHigh = 0x00;
        }
    }
}