namespace Ninu.Emulator.CentralProcessor
{
    public enum JumpType
    {
        Conditional,
        Unconditional,
        Break,
        Interrupt,
        Nmi,
        ReturnFromSubroutine,
        ReturnFromInterrupt,
    }
}