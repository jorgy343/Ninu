namespace Ninu.Emulator
{
    /// <summary>
    /// Represents an index into a palette. This is a wrapper that is guaranteed to represent a number between 0 and 3
    /// inclusive.
    /// </summary>
    public readonly struct PaletteEntryIndex
    {
        /// <summary>
        /// The underlying data which represents the index into the palette. This is guaranteed to be a number between
        /// 0 and 3 inclusive.
        /// </summary>
        [Save]
        public byte Data { get; }

        /// <summary>
        /// Creates a new instance of this object. The parameter is expected to be a number between 0 and 3 inclusive;
        /// however, the parameter is AND with 0x3 to enforce this guarantee.
        /// </summary>
        /// <param name="data">A number between 0 and 3 inclusive.</param>
        public PaletteEntryIndex(byte data)
        {
            Data = (byte)(data & 0x3);
        }

        /// <summary>
        /// Creates a new instance of this object. Both parameters are expected to be either 0 or 1; however, both
        /// parameters are AND with 0x1 to ensure that the end result is guaranteed to be a number between 0 and 3
        /// inclusive.
        /// </summary>
        /// <param name="lowBit">The low bit of the index.</param>
        /// <param name="highBit">The high bit of the index. This should be either 0 or 1. The bit should not be shifted to the left.</param>
        public PaletteEntryIndex(int lowBit, int highBit)
        {
            Data = (byte)((lowBit & 0x1) | ((highBit & 0x1) << 1));
        }

        public static PaletteEntryIndex operator ~(PaletteEntryIndex value) => new PaletteEntryIndex((byte)~value.Data);

        public static PaletteEntryIndex operator &(PaletteEntryIndex left, PaletteEntryIndex right) => new PaletteEntryIndex((byte)(left.Data & right.Data));
        public static PaletteEntryIndex operator |(PaletteEntryIndex left, PaletteEntryIndex right) => new PaletteEntryIndex((byte)(left.Data | right.Data));
        public static PaletteEntryIndex operator ^(PaletteEntryIndex left, PaletteEntryIndex right) => new PaletteEntryIndex((byte)(left.Data ^ right.Data));

        public static PaletteEntryIndex operator &(byte left, PaletteEntryIndex right) => new PaletteEntryIndex((byte)(left & right.Data));
        public static PaletteEntryIndex operator |(byte left, PaletteEntryIndex right) => new PaletteEntryIndex((byte)(left | right.Data));
        public static PaletteEntryIndex operator ^(byte left, PaletteEntryIndex right) => new PaletteEntryIndex((byte)(left ^ right.Data));

        public static PaletteEntryIndex operator &(PaletteEntryIndex left, byte right) => new PaletteEntryIndex((byte)(left.Data & right));
        public static PaletteEntryIndex operator |(PaletteEntryIndex left, byte right) => new PaletteEntryIndex((byte)(left.Data | right));
        public static PaletteEntryIndex operator ^(PaletteEntryIndex left, byte right) => new PaletteEntryIndex((byte)(left.Data ^ right));

        public static PaletteEntryIndex operator &(int left, PaletteEntryIndex right) => new PaletteEntryIndex((byte)(left & right.Data));
        public static PaletteEntryIndex operator |(int left, PaletteEntryIndex right) => new PaletteEntryIndex((byte)(left | right.Data));
        public static PaletteEntryIndex operator ^(int left, PaletteEntryIndex right) => new PaletteEntryIndex((byte)(left ^ right.Data));

        public static PaletteEntryIndex operator &(PaletteEntryIndex left, int right) => new PaletteEntryIndex((byte)(left.Data & right));
        public static PaletteEntryIndex operator |(PaletteEntryIndex left, int right) => new PaletteEntryIndex((byte)(left.Data | right));
        public static PaletteEntryIndex operator ^(PaletteEntryIndex left, int right) => new PaletteEntryIndex((byte)(left.Data ^ right));

        public static implicit operator byte(PaletteEntryIndex register) => register.Data;
        public static implicit operator int(PaletteEntryIndex register) => register.Data;

        public static explicit operator PaletteEntryIndex(byte data) => new PaletteEntryIndex(data);
        public static explicit operator PaletteEntryIndex(int data) => new PaletteEntryIndex((byte)data);
    }
}