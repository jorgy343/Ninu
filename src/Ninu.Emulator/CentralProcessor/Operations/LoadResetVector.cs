namespace Ninu.Emulator.CentralProcessor.Operations
{
    /// <summary>
    /// Loads the reset vector found at 0xfffc and 0xfffd and puts the result into the PC register.
    /// Normally this would take two operations, but we'll just use one and be sure to insert a Nop
    /// before this operation.
    /// </summary>
    public class LoadResetVector : CpuOperation
    {
        private LoadResetVector()
        {

        }

        public static LoadResetVector Singleton { get; } = new();

        public override void Execute(NewCpu cpu, IBus bus)
        {
            var pcLow = bus.Read(0xfffc);
            var pcHigh = bus.Read(0xfffd);

            cpu.CpuState.PC = (ushort)(pcLow | (pcHigh << 8));
        }
    }
}