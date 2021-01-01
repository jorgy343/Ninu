namespace Ninu.Emulator.CentralProcessor.Operations
{
    /// <summary>
    /// Fetches the byte of data found at PC and stores it into the CPU's temporary data latch.
    /// </summary>
    public class FetchDataByPC : CpuOperation
    {
        public override void Execute(NewCpu cpu, IBus bus)
        {
            var data = bus.Read(cpu.CpuState.PC);

            cpu.DataLatch = data;
        }
    }
}