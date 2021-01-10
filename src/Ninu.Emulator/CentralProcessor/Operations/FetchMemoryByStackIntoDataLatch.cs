namespace Ninu.Emulator.CentralProcessor.Operations
{
    /// <summary>
    /// Fetches the byte of data found in memory at PC and stores it into the CPU's data latch.
    /// </summary>
    public class FetchMemoryByStackIntoDataLatch : CpuOperation
    {
        private FetchMemoryByStackIntoDataLatch()
        {

        }

        public static FetchMemoryByStackIntoDataLatch Singleton { get; } = new();

        public override void Execute(NewCpu cpu, IBus bus)
        {
            var data = bus.Read((ushort)(cpu.CpuState.S + 0x100));
            cpu.DataLatch = data;
        }
    }
}