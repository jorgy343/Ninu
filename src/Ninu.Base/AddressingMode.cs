namespace Ninu.Base
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

        /// <summary>
        /// Does nothing. This is used by the emulator only to simplify the implementation of
        /// undocumented instructions.
        /// </summary>
        Dummy,
    }
}