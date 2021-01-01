namespace Ninu.Emulator.CentralProcessor.Operations
{
    /// <summary>
    /// Fetches the low byte of the address found at PC and stores it in the CPU's low address
    /// latch.
    /// </summary>
    public class FetchAddressLowByPC : CpuOperation
    {
        public override void Execute(NewCpu cpu, IBus bus)
        {
            var addressLow = bus.Read(cpu.CpuState.PC);

            cpu.AddressLatchLow = addressLow;
        }
    }
}