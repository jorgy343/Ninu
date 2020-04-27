namespace Ninu.Emulator
{
    /// <summary>
    /// Holds state data for the background rendering process.
    /// </summary>
    public class PpuBackgroundState
    {
        [Save]
        public byte NextNameTableTileId { get; set; }

        [Save]
        public byte NextNameTableAttribute { get; set; }

        [Save]
        public byte NextLowPatternByte { get; set; }

        [Save]
        public byte NextHighPatternByte { get; set; }

        [Save]
        public ushort ShiftNameTableAttributeLow { get; set; }

        [Save]
        public ushort ShiftNameTableAttributeHigh { get; set; }

        [Save]
        public ushort ShiftLowPatternByte { get; set; }

        [Save]
        public ushort ShiftHighPatternByte { get; set; }
    }
}