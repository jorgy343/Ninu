using Ninu.Emulator.PpuRegisters;
using System;

namespace Ninu.Emulator
{
    public class Ppu : ICpuBusComponent
    {
        private readonly Cartridge _cartridge;
        private readonly NameTableRam _nameTableRam;

        public PaletteRam PaletteRam { get; } = new PaletteRam();

        public PpuRegisterState Registers { get; } = new PpuRegisterState();

        public bool CallNmi { get; set; }

        private int _currentCycle;
        private int _currentScanline;

        public Ppu(Cartridge cartridge)
        {
            _cartridge = cartridge ?? throw new ArgumentNullException(nameof(cartridge));

            _nameTableRam = new NameTableRam(cartridge.Image.Mirroring);
        }

        public void Reset()
        {

        }

        public void Clock()
        {
            _currentCycle++;

            if (_currentScanline == -1 && _currentCycle == 1)
            {
                Registers.Status.ClearVerticalBlank();
            }

            if (_currentScanline == 241 && _currentCycle == 1)
            {
                Registers.Status.SetVerticalBlank();

                // TODO: Is this the proper scanline and cycle to call the NMI on?
                if (Registers.Control.GenerateVerticalBlankingIntervalNmi)
                {
                    CallNmi = true;
                }
            }

            if (_currentScanline == 261 && _currentCycle == 1)
            {
                Registers.Status.Data = 0;
            }

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

        public PatternTile GetPatternTile(PatternTableEntry entry, int index)
        {
            // TODO: What is the upper bound of the index?
            if (index < 0) throw new ArgumentOutOfRangeException(nameof(index));

            var entryOffset = entry == PatternTableEntry.Left ? 0x0000 : 0x1000;

            Span<byte> plane1 = stackalloc byte[8];
            Span<byte> plane2 = stackalloc byte[8];

            for (var i = 0; i < 8; i++)
            {
                // Pattern memory starts at 0x0000.
                plane1[i] = PpuRead((ushort)(entryOffset + index * 16 + i));
                plane2[i] = PpuRead((ushort)(entryOffset + index * 16 + 8 + i)); // Add 8 bytes to skip the first plane.
            }

            return new PatternTile(plane1, plane2);
        }

        public bool CpuRead(ushort address, out byte data)
        {
            if (address >= 0x2000 && address <= 0x3fff)
            {
                switch (address & 0x7)
                {
                    case 2:
                        // According to some sources, the bottom 5 bits of this register take on the bottom
                        // 5 bits of the data buffer.
                        data = (byte)((Registers.Status.Data & 0xe0) | (Registers.DataBuffer & 0x1f));

                        // When the status register is read, the vertical blank flag is cleared and the
                        // VRAM address is reset to zero.
                        Registers.Status.ClearVerticalBlank();
                        Registers.VramAddress = 0;

                        return true;

                    case 7:
                        if (Registers.VramAddress >= 0x3f00 && Registers.VramAddress <= 0x3fff)
                        {
                            // Palette memory is read immediately.
                            Registers.DataBuffer = PpuRead(Registers.VramAddress);

                            data = Registers.DataBuffer;
                        }
                        else
                        {
                            // All other memory is deleted by one call.
                            data = Registers.DataBuffer;

                            // Update the buffer with new data only after the current buffer is read.
                            Registers.DataBuffer = PpuRead(Registers.VramAddress);
                        }

                        return true;
                }
            }

            data = 0;
            return false;
        }

        public bool CpuWrite(ushort address, byte data)
        {
            if (address >= 0x2000 && address <= 0x3fff)
            {
                switch (address & 0x7)
                {
                    case 0:
                        Registers.Control.Data = data;
                        break;

                    case 1:
                        Registers.Mask.Data = data;
                        break;

                    case 6:
                        Registers.WriteVramAddressByte(data);
                        break;

                    case 7:
                        PpuWrite(Registers.VramAddress, data);

                        Registers.PostVramReadWrite();
                        break;
                }

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

            if (PaletteRam.PpuRead(address, out data))
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

            PaletteRam.PpuWrite(address, data);
        }
    }
}