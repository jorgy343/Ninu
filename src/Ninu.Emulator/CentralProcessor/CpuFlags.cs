using System;

namespace Ninu.Emulator.CentralProcessor
{
    [Flags]
    public enum CpuFlags : byte
    {
        C = 1 << 0, // Carry
        Z = 1 << 1, // Zero
        I = 1 << 2, // Disable Interrupts
        D = 1 << 3, // Decimal Mode
        V = 1 << 6, // Overflow
        N = 1 << 7, // Negative
    }

    public static class CpuFlagsExtensions
    {
        public static string ToPrettyString(this CpuFlags cpuFlags)
        {
            return $"{(cpuFlags.HasFlag(CpuFlags.N) ? "N" : "n")}{(cpuFlags.HasFlag(CpuFlags.V) ? "V" : "v")}--{(cpuFlags.HasFlag(CpuFlags.D) ? "D" : "d")}{(cpuFlags.HasFlag(CpuFlags.I) ? "I" : "i")}{(cpuFlags.HasFlag(CpuFlags.Z) ? "Z" : "z")}{(cpuFlags.HasFlag(CpuFlags.C) ? "C" : "c")}";
        }
    }
}