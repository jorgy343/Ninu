namespace Ninu.Emulator.CentralProcessor.Profilers
{
    public interface IJumpProfiler : IProfiler
    {
        void JumpEncountered(in JumpResult jumpResult);
    }
}