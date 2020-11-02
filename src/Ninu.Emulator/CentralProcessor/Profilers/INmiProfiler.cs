namespace Ninu.Emulator.CentralProcessor.Profilers
{
    public interface INmiProfiler : IProfiler
    {
        void NmiPerformed(CpuState cpuState);
    }
}