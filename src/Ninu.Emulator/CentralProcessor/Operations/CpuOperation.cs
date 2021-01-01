namespace Ninu.Emulator.CentralProcessor.Operations
{
    /// <summary>
    /// Represents a CPU operation that will take a single cycle.
    /// </summary>
    public abstract class CpuOperation
    {
        public CpuOperation(bool isFree = false)
        {
            IsFree = isFree;
        }

        /// <summary>
        /// When set to <c>true</c>, the CPU will consume this operation along with the next
        /// operation in the queue in the same clock cycle. Typically, this should almost always be
        /// <c>false</c>.
        /// </summary>
        public bool IsFree { get; }

        public abstract void Execute(NewCpu cpu, IBus bus);
    }
}