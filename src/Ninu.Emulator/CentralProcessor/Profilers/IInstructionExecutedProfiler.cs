namespace Ninu.Emulator.CentralProcessor.Profilers
{
    public interface IInstructionExecutedProfiler : IProfiler
    {
        void InstructionExecuted(CpuInstruction instruction, CpuState cpuState, int cycles);
    }
}