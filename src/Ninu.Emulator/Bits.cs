using System.Runtime.CompilerServices;

namespace Ninu.Emulator
{
    public static class Bits
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static int CreateMask(int size, int position) => ((1 << size) - 1) << position;

        // Get Methods
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static byte GetBits(byte data, int maskSize, int maskPosition) => (byte)((data & CreateMask(maskSize, maskPosition)) >> maskPosition);

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static ushort GetBits(ushort data, int maskSize, int maskPosition) => (ushort)((data & CreateMask(maskSize, maskPosition)) >> maskPosition);

        // Set Methods
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static int SetBits(byte data, byte newBitValues, int maskSize, int maskPosition)
        {
            var mask = CreateMask(maskSize, maskPosition);
            return (data & ~mask) | ((newBitValues << maskPosition) & mask);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static int SetBits(ushort data, ushort newBitValues, int maskSize, int maskPosition)
        {
            var mask = CreateMask(maskSize, maskPosition);
            return (data & ~mask) | ((newBitValues << maskPosition) & mask);
        }

        // Single Bit Methods
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static bool GetBit(byte data, int position) => (data & (1 << position)) != 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static bool GetBit(ushort data, int position) => (data & (1 << position)) != 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static int SetBit(byte data, bool value, int position) => value ? data | (1 << position) : data & ~(1 << position);

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static int SetBit(ushort data, bool value, int position) => value ? data | (1 << position) : data & ~(1 << position);
    }
}