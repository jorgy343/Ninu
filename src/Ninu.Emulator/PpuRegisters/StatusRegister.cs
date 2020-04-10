namespace Ninu.Emulator.PpuRegisters
{
    public sealed class StatusRegister
    {
        public byte Data { get; set; }

        private const int SpriteOverflowMask = 1 << 5;
        private const int Sprite0HitMask = 1 << 6;
        private const int VerticalBlankStartedMask = 1 << 7;

        public bool SpriteOverflow
        {
            get => (Data & SpriteOverflowMask) != 0;
            set => Data = (byte)(value ? Data | SpriteOverflowMask : Data & ~SpriteOverflowMask);
        }

        public bool Sprite0Hit
        {
            get => (Data & Sprite0HitMask) != 0;
            set => Data = (byte)(value ? Data | Sprite0HitMask : Data & ~Sprite0HitMask);
        }

        public bool VerticalBlankStarted
        {
            get => (Data & VerticalBlankStartedMask) != 0;
            set => Data = (byte)(value ? Data | VerticalBlankStartedMask : Data & ~VerticalBlankStartedMask);
        }

        public override string ToString() => $"0x{Data:x2}";
    }
}