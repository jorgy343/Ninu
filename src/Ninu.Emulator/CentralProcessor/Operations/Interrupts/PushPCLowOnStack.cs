namespace Ninu.Emulator.CentralProcessor.Operations.Interrupts
{
    /// <summary>
    /// Pushes the low byte of the PC register onto the stack at 0x100 + S - 1. The stack register
    /// is not decremented.
    /// </summary>
    public class PushPCLowOnStack : CpuOperation
    {
        private PushPCLowOnStack()
        {

        }

        public static PushPCLowOnStack Singleton { get; } = new();

        public override void Execute(NewCpu cpu, IBus bus)
        {
            var pcLow = (byte)(cpu.CpuState.PC & 0x00ff);
            bus.Write((ushort)(0x100 + cpu.CpuState.S - 1), pcLow);
        }
    }
}