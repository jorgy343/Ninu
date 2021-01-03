namespace Ninu.Emulator.CentralProcessor.Operations
{
    /// <summary>
    /// Fetches a byte from memory based on PC and stores it into the CPU's effective address latch
    /// low. The CPU's effective address latch high is cleared to zero so the effective address
    /// latch as a whole can be used for a zero page memory access.
    /// </summary>
    public class FetchZeroPageAddressByPCIntoEffectiveAddressLatch : CpuOperation
    {
        private FetchZeroPageAddressByPCIntoEffectiveAddressLatch()
        {

        }

        public static FetchZeroPageAddressByPCIntoEffectiveAddressLatch Singleton { get; } = new();

        public override void Execute(NewCpu cpu, IBus bus)
        {
            cpu.EffectiveAddressLatchLow = bus.Read(cpu.CpuState.PC);
            cpu.EffectiveAddressLatchHigh = 0x00;
        }
    }
}