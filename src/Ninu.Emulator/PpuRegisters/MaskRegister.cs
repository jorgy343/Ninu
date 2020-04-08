// ReSharper disable ShiftExpressionRealShiftCountIsZero

namespace Ninu.Emulator.PpuRegisters
{
    public sealed class MaskRegister
    {
        public byte Data { get; set; }

        private readonly int _enableGrayscaleMask = 1 << 0;
        private readonly int _showBackgroundInLeftMost8PixelsOfScreenMask = 1 << 1;
        private readonly int _showSpritesInLeftMost8PixelsOfScreenMask = 1 << 2;
        private readonly int _showBackgroundMask = 1 << 3;
        private readonly int _showSpritesMask = 1 << 4;
        private readonly int _emphasizeRedMask = 1 << 5;
        private readonly int _emphasizeGreenMask = 1 << 6;
        private readonly int _emphasizeBlueMask = 1 << 7;

        public bool EnableGrayscale => (Data & _enableGrayscaleMask) != 0;
        public bool ShowBackgroundInLeftMost8PixelsOfScreen => (Data & _showBackgroundInLeftMost8PixelsOfScreenMask) != 0;
        public bool ShowSpritesInLeftMost8PixelsOfScreen => (Data & _showSpritesInLeftMost8PixelsOfScreenMask) != 0;
        public bool ShowBackground => (Data & _showBackgroundMask) != 0;
        public bool ShowSprites => (Data & _showSpritesMask) != 0;
        public bool EmphasizeRed => (Data & _emphasizeRedMask) != 0;
        public bool EmphasizeGreen => (Data & _emphasizeGreenMask) != 0;
        public bool EmphasizeBlue => (Data & _emphasizeBlueMask) != 0;

        public override string ToString() => $"0x{Data:x2}";
    }
}