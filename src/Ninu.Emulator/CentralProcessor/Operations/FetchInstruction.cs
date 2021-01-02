namespace Ninu.Emulator.CentralProcessor.Operations
{
    /// <summary>
    /// Fetches the instruction found at PC and instructs the CPU to decode it into CPU operations.
    /// </summary>
    public class FetchInstruction : CpuOperation
    {
        private FetchInstruction()
        {

        }

        public static FetchInstruction Singleton { get; } = new();

        public override void Execute(NewCpu cpu, IBus bus)
        {
            var instruction = bus.Read(cpu.CpuState.PC);

            cpu.ExecuteInstruction(instruction);
        }
    }
}