// ReSharper disable InconsistentNaming

namespace Ninu.Emulator.PpuRegisters
{
    public sealed class ControlRegister
    {
        public byte Data { get; set; }

        private readonly int _baseNameTableAddressMask = 0x0000_0011;
        private readonly int _vramAddressIncrementMask = 1 << 2;
        private readonly int _spritePatternTableAddressFor8x8Mask = 1 << 3;
        private readonly int _backgroundPatternTableAddressMask = 1 << 4;
        private readonly int _spriteSizeMask = 1 << 5;
        private readonly int _ppuMasterSlaveSelect = 1 << 6;
        private readonly int _generateVerticalBlankingIntervalNmi = 1 << 7;

        public ushort BaseNameTableAddress => (Data & _baseNameTableAddressMask) switch
        {
            0 => 0x2000,
            1 => 0x2400,
            2 => 0x2800,
            3 => 0x2c00,
            _ => 0x2000,
        };

        public VramAddressIncrement VramAddressIncrement => (Data & _vramAddressIncrementMask) == 0 ? VramAddressIncrement.Add1GoingAcross : VramAddressIncrement.Add32GoingDown;
        public ushort SpritePatternTableAddressFor8x8 => (Data & _spritePatternTableAddressFor8x8Mask) == 0 ? (ushort)0x0000 : (ushort)0x1000;
        public ushort BackgroundPatternTableAddress => (Data & _backgroundPatternTableAddressMask) == 0 ? (ushort)0x0000 : (ushort)0x1000;
        public SpriteSize SpriteSize => (Data & _spriteSizeMask) == 0 ? SpriteSize.Size8x8 : SpriteSize.Size8x16;
        public bool PpuMasterSlaveSelect => (Data & _ppuMasterSlaveSelect) != 0;
        public bool GenerateVerticalBlankingIntervalNmi => (Data & _generateVerticalBlankingIntervalNmi) != 0;

        public override string ToString() => $"0x{Data:x2}";
    }
}