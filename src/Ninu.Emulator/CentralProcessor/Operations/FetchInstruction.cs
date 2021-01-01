namespace Ninu.Emulator.CentralProcessor.Operations
{
    /// <summary>
    /// Fetches the instruction found at PC and instructs the CPU to decode it into CPU operations.
    /// </summary>
    public class FetchInstruction : CpuOperation
    {
        public override void Execute(NewCpu cpu, IBus bus)
        {
            var instruction = bus.Read(cpu.CpuState.PC);

            cpu.ExecuteInstruction(instruction);
        }
    }
}