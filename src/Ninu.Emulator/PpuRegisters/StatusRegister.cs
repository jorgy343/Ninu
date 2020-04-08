namespace Ninu.Emulator.PpuRegisters
{
    public sealed class StatusRegister
    {
        public byte Data { get; set; }

        private readonly int _spriteOverflowMask = 1 << 5;
        private readonly int _sprite0HitMask = 1 << 6;
        private readonly int _verticalBlankStartedMask = 1 << 7;

        public bool SpriteOverflow => (Data & _spriteOverflowMask) != 0;
        public bool Sprite0Hit => (Data & _sprite0HitMask) != 0;
        public bool VerticalBlankStarted => (Data & _verticalBlankStartedMask) != 0;

        public override string ToString() => $"0x{Data:x2}";

        public void ClearVerticalBlank() => Data &= (byte)(~_verticalBlankStartedMask);
    }
}