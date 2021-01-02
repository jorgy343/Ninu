using System;

namespace Ninu.Emulator.CentralProcessor.Operations
{
    /// <summary>
    /// Executes an action that represents what the instruction does and then fetches the
    /// instruction found at PC and instructs the CPU to decode it into CPU operations.
    /// </summary>
    public class FetchInstructionAndExecute : CpuOperation
    {
        private readonly Action _execute;

        public FetchInstructionAndExecute(Action execute)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        }

        public override void Execute(NewCpu cpu, IBus bus)
        {
            _execute();

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