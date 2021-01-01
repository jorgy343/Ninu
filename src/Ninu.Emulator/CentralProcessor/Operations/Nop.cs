namespace Ninu.Emulator.CentralProcessor.Operations
{
    /// <summary>
    /// Doesn't perform any operation on the CPU. This simply takes up a cycle.
    /// </summary>
    public class Nop : CpuOperation
    {
        private Nop()
        {

        }

        public static Nop Singleton { get; } = new();

        public override void Execute(NewCpu cpu, IBus bus)
        {

        }
    }
}