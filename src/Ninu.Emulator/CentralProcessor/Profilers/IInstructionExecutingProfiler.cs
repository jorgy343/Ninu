namespace Ninu.Emulator.CentralProcessor.Profilers
{
    public interface IInstructionExecutingProfiler : IProfiler
    {
        void InstructionExecuting(CpuInstruction instruction, CpuState cpuState);
    }
}