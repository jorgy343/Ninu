namespace Ninu.Emulator.CentralProcessor.Operations
{
    /// <summary>
    /// Fetches the byte of data found in memory at PC and stores it into the CPU's data latch.
    /// </summary>
    public class FetchMemoryByPCIntoDataLatch : CpuOperation
    {
        private FetchMemoryByPCIntoDataLatch()
        {

        }

        public static FetchMemoryByPCIntoDataLatch Singleton { get; } = new FetchMemoryByPCIntoDataLatch();

        public override void Execute(NewCpu cpu, IBus bus)
        {
            var data = bus.Read(cpu.CpuState.PC);

            cpu.DataLatch = data;
        }
    }
}