using System.Runtime.CompilerServices;

namespace Ninu.Emulator
{
    public static class Bits
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static int CreateMask(int position, int size) => ((1 << size) - 1) << position;

        // Get Methods
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static byte GetBits(byte data, int maskPosition, int maskSize) => (byte)((data & CreateMask(maskPosition, maskSize)) >> maskPosition);

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static ushort GetBits(ushort data, int maskPosition, int maskSize) => (ushort)((data & CreateMask(maskPosition, maskSize)) >> maskPosition);

        // Set Methods
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static int SetBits(byte data, int maskPosition, int maskSize, byte newBitValues)
        {
            var mask = CreateMask(maskPosition, maskSize);
            return (data & ~mask) | ((newBitValues << maskPosition) & mask);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static int SetBits(ushort data, int maskPosition, int maskSize, ushort newBitValues)
        {
            var mask = CreateMask(maskPosition, maskSize);
            return (data & ~mask) | ((newBitValues << maskPosition) & mask);
        }

        // Single Bit Methods
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static bool GetBit(byte data, int position) => (data & (1 << position)) != 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static bool GetBit(ushort data, int position) => (data & (1 << position)) != 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static int SetBit(byte data, int position, bool value) => value ? data | (1 << position) : data & ~(1 << position);

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static int SetBit(ushort data, int position, bool value) => value ? data | (1 << position) : data & ~(1 << position);
    }
}