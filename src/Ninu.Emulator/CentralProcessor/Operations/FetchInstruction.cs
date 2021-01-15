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

        public override void Execute(Cpu cpu, IBus bus)
        {
            if (cpu._nmi && cpu._nmiCycle != cpu._totalCycles - 1)
            {
                cpu.CheckForNmi();
            }
            else
            {
                var instruction = bus.Read(cpu.CpuState.PC);
                cpu.ExecuteInstruction(instruction);
            }
        }
    }
}