namespace Ninu.Emulator.CentralProcessor.Operations
{
    /// <summary>
    /// Represents a CPU operation that will take a single cycle.
    /// </summary>
    public abstract class CpuOperation
    {
        public abstract void Execute(NewCpu cpu, IBus bus);
    }
}