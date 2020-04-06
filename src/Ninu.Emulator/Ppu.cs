using System;

namespace Ninu.Emulator
{
    public class Ppu : ICpuBusComponent
    {
        private readonly Cartridge _cartridge;
        private readonly NameTableRam _nameTableRam = new NameTableRam();
        private readonly PaletteRam _paletteRam = new PaletteRam();

        private int _currentCycle;
        private int _currentScanline;

        public Ppu(Cartridge cartridge)
        {
            _cartridge = cartridge ?? throw new ArgumentNullException(nameof(cartridge));
        }

        public void Reset()
        {

        }

        public void Clock()
        {
            _currentCycle++;

            if (_currentCycle >= 341)
            {
                _currentCycle = 0;
                _currentScanline++;

                if (_currentScanline >= 261)
                {
                    _currentScanline = -1;
                }
            }
        }

        public PatternTile GetPatternTile(int index)
        {
            // TODO: What is the upper bound of the index?
            if (index < 0) throw new ArgumentOutOfRangeException(nameof(index));

            Span<byte> plane1 = stackalloc byte[8];
            Span<byte> plane2 = stackalloc byte[8];

            for (var i = 0; i < 8; i++)
            {
                // Pattern memory starts at 0x0000.
                plane1[i] = PpuRead((ushort)(index * 16 + i));
                plane2[i] = PpuRead((ushort)(index * 16 + 8 + i)); // Add 8 bytes to skip the first plane.
            }

            return new PatternTile(plane1, plane2);
        }

        public bool CpuRead(ushort address, out byte data)
        {
            if (address >= 0x2000 && address <= 0x3fff)
            {
                var translatedAddress = (ushort)(address & 0x0007);

                data = 0;
                return true;
            }

            data = 0;
            return false;
        }

        public bool CpuWrite(ushort address, byte data)
        {
            if (address >= 0x2000 && address <= 0x3fff)
            {
                var translatedAddress = (ushort)(address & 0x0007);

                return true;
            }

            return false;
        }

        public byte PpuRead(ushort address)
        {
            address &= 0x3fff; // Ensure we never read outside of the PPU bus's address range.

            if (_cartridge.PpuRead(address, out var data))
            {
                return data;
            }

            if (_nameTableRam.PpuRead(address, out data))
            {
                return data;
            }

            if (_paletteRam.PpuRead(address, out data))
            {
                return data;
            }

            return 0;
        }

        public void PpuWrite(ushort address, byte data)
        {
            address &= 0x3fff; // Ensure we never write outside of the PPU bus's address range.

            _cartridge.PpuWrite(address, data);
            _nameTableRam.PpuWrite(address, data);
            _paletteRam.PpuWrite(address, data);
        }
    }
}