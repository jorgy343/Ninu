using System;
// ReSharper disable ShiftExpressionRealShiftCountIsZero

namespace Ninu.Emulator
{
    [Flags]
    public enum CpuFlags : byte
    {
        C = 1 << 0, // Carry
        Z = 1 << 1, // Zero
        I = 1 << 2, // Disable Interrupts
        D = 1 << 3, // Decimal Mode
        B = 1 << 4, // Break
        U = 1 << 5, // Unused
        V = 1 << 6, // Overflow
        N = 1 << 7, // Negative
    }
}