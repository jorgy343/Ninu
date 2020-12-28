namespace Ninu.Emulator.GraphicsProcessor
{
    /// <summary>
    /// Specifies which pattern table is being used.
    /// </summary>
    public enum PatternTableOffset : ushort
    {
        /// <summary>
        /// Specifies the left pattern table at PPU address 0x0000.
        /// </summary>
        Left = 0x0000,

        /// <summary>
        /// Specifies the right pattern table at PPU address 0x1000.
        /// </summary>
        Right = 0x1000,
    }
}