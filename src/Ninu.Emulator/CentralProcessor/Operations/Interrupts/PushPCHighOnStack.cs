namespace Ninu.Emulator.CentralProcessor.Operations.Interrupts
{
    /// <summary>
    /// Pushes the high byte of the PC register onto the stack at 0x100 + S. The stack register is
    /// not decremented.
    /// </summary>
    public class PushPCHighOnStack : CpuOperation
    {
        private PushPCHighOnStack()
        {

        }

        public static PushPCHighOnStack Singleton { get; } = new();

        public override void Execute(NewCpu cpu, IBus bus)
        {
            var pcHigh = (byte)(cpu.CpuState.PC >> 8);
            bus.Write((ushort)(0x100 + cpu.CpuState.S), pcHigh);
        }
    }
}