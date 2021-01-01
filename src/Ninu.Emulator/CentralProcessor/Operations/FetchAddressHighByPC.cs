namespace Ninu.Emulator.CentralProcessor.Operations
{
    /// <summary>
    /// Fetches the high byte of the address found at PC and stores it in the CPU's high address
    /// latch.
    /// </summary>
    public class FetchAddressHighByPC : CpuOperation
    {
        public override void Execute(NewCpu cpu, IBus bus)
        {
            var addressHigh = bus.Read(cpu.CpuState.PC);

            cpu.AddressLatchHigh = addressHigh;
        }
    }
}