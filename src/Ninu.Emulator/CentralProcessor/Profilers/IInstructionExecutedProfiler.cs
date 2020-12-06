using Ninu.Base;

namespace Ninu.Emulator.CentralProcessor.Profilers
{
    public interface IInstructionExecutedProfiler : IProfiler
    {
        void InstructionExecuted(Instruction instruction, CpuState cpuState, int cycles);
    }
}