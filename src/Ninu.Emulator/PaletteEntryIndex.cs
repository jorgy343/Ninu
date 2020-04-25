namespace Ninu.Emulator
{
    public struct PaletteEntryIndex
    {
        [Save]
        public byte Data;

        public PaletteEntryIndex(byte data)
        {
            Data = (byte)(data & 0x3);
        }

        public PaletteEntryIndex(int lowBit, int highBit)
        {
            Data = (byte)((lowBit & 0x1) | ((highBit & 0x1) << 1));
        }

        public PaletteEntryIndex(bool lowBit, bool highBit)
        {
            Data = (byte)((lowBit ? 0b01 : 0b00) | (highBit ? 0b10 : 0b00));
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