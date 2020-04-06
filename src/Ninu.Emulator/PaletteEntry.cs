namespace Ninu.Emulator
{
    public struct PaletteEntry
    {
        public byte Byte1 { get; }
        public byte Byte2 { get; }
        public byte Byte3 { get; }

        /// <summary>
        /// This byte is always a mirror of the background color.
        /// </summary>
        public byte Byte4 { get; }

        public PaletteEntry(byte byte1, byte byte2, byte byte3, byte byte4)
        {
            Byte1 = byte1;
            Byte2 = byte2;
            Byte3 = byte3;
            Byte4 = byte4;
        }
    }
}