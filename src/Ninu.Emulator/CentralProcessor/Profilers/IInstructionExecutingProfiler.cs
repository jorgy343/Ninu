using Ninu.Base;

namespace Ninu.Emulator.CentralProcessor.Profilers
{
    public interface IInstructionExecutingProfiler : IProfiler
    {
        void InstructionExecuting(Instruction instruction, CpuState cpuState);
    }
}