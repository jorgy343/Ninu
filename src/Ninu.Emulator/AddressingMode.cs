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
    }
}