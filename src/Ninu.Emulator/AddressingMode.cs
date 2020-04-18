namespace Ninu.Emulator
{
    public enum AddressingMode
    {
        Implied,
        Accumulator,
        Immediate,
        ZeroPage,
        ZeroPageWithXOffset,
        ZeroPageWithYOffset,
        Absolute,
        AbsoluteWithXOffset,
        AbsoluteWithYOffset,
        Indirect,
        IndirectZeroPageWithXOffset,
        IndirectZeroPageWithYOffset,
        Relative,
        Dummy, // Does nothing. This is used to simplify undocumented instructions.
    }
}