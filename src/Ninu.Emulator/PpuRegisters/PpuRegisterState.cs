namespace Ninu.Emulator.PpuRegisters
{
    public sealed class PpuRegisterState
    {
        public ControlRegister Control { get; } = new ControlRegister();
        public MaskRegister Mask { get; } = new MaskRegister();
        public StatusRegister Status { get; } = new StatusRegister();
    }
}