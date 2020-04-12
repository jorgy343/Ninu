using System;

namespace Ninu.Emulator
{
    public delegate void FrameCompleteHandler(object source, EventArgs e);

    public class Ppu : ICpuBusComponent
    {
        private readonly Cartridge _cartridge;
        private readonly NameTableRam _nameTableRam;

        public PaletteRam PaletteRam { get; } = new PaletteRam();

        public PpuRegisters Registers { get; } = new PpuRegisters();

        public bool CallNmi { get; set; }

        private int _currentCycle;
        private int _currentScanline;

        public event FrameCompleteHandler? FrameComplete;

        private byte _nextNameTableTileId;
        private byte _nextNameTableAttribute;
        private byte _nextLowPatternByte;
        private byte _nextHighPatternByte;

        private ushort _shiftNameTableAttributeLow;
        private ushort _shiftNameTableAttributeHigh;
        private ushort _shiftLowPatternByte;
        private ushort _shiftHighPatternByte;

        public byte[] CurrentImageBuffer { get; private set; } = new byte[256 * 240];
        public byte[] PreviousImageBuffer { get; private set; } = new byte[256 * 240];

        public Ppu(Cartridge cartridge)
        {
            _cartridge = cartridge ?? throw new ArgumentNullException(nameof(cartridge));

            _nameTableRam = new NameTableRam(cartridge.Image.Mirroring);
        }

        public void Reset()
        {

        }

        public PpuClockResult Clock()
        {
            if (_currentScanline == -1 && _currentCycle == 1)
            {
                Registers.SpriteOverflow = false;
                Registers.Sprite0Hit = false;
                Registers.VerticalBlankStarted = false;
            }

            if (_currentScanline >= -1 && _currentScanline <= 239) // All of the rendering scanlines.
            {
                if ((_currentCycle >= 2 && _currentCycle <= 257) || (_currentCycle >= 322 && _currentCycle <= 337))
                {
                    UpdateShiftRegisters();
                }

                if ((_currentCycle >= 2 && _currentCycle <= 256) || (_currentCycle >= 321 && _currentCycle <= 337))
                {
                    if (_currentCycle != 1 && _currentCycle != 321 && _currentCycle % 8 == 1)
                    {
                        LoadShiftRegisters();
                    }

                    if (_currentCycle % 8 == 2)
                    {
                        // Read the next name table byte.
                        _nextNameTableTileId = FetchNameTableTileId();
                    }

                    if (_currentCycle % 8 == 4)
                    {
                        // Read the name table attribute byte.
                        _nextNameTableAttribute = FetchNameTableAttribute();
                    }

                    if (_currentCycle % 8 == 6)
                    {
                        // Load the low pattern tile byte.
                        _nextLowPatternByte = FetchLowPatternByte(_nextNameTableTileId);
                    }

                    if (_currentCycle % 8 == 0)
                    {
                        // Load the high pattern tile byte.
                        _nextHighPatternByte = FetchHighPatternByte(_nextNameTableTileId);

                        Registers.IncrementX();
                    }
                }

                if (_currentCycle == 256)
                {
                    Registers.IncrementY();
                }

                if (_currentCycle == 257)
                {
                    Registers.TransferX();
                }
            }

            if (_currentScanline == -1 && _currentCycle >= 280 && _currentCycle <= 304)
            {
                Registers.TransferY();
            }

            if (_currentScanline == 241 && _currentCycle == 1)
            {
                Registers.VerticalBlankStarted = true;

                if (Registers.GenerateVerticalBlankingIntervalNmi)
                {
                    CallNmi = true;
                }
            }

            if (_currentScanline >= 0 && _currentScanline <= 239 && _currentCycle >= 1 && _currentCycle <= 256)
            {
                var shiftSelect = (ushort)0x8000; // By default we are interested in the most significant bit in the shift registers.

                // The fine X register controls our offset into the shift registers.
                shiftSelect >>= Registers.FineX;

                // Extract the data from the pattern tile shift registers.
                var patternTileLowBit = (_shiftLowPatternByte & shiftSelect) != 0 ? (byte)0b01 : (byte)0x0;
                var patternTileHighBit = (_shiftHighPatternByte & shiftSelect) != 0 ? (byte)0b10 : (byte)0x0;

                var patternTileByte = (byte)(patternTileLowBit | patternTileHighBit);

                // Extract the data from the name table attribute shift registers.
                var nameTableAttributeLowBit = (_shiftNameTableAttributeLow & shiftSelect) != 0 ? (byte)0b01 : (byte)0x0;
                var nameTableAttributeHighBit = (_shiftNameTableAttributeHigh & shiftSelect) != 0 ? (byte)0b10 : (byte)0x0;

                var nameTableAttribute = (byte)(nameTableAttributeLowBit | nameTableAttributeHighBit);

                // Set the pixel color.
                var palette = PaletteRam.GetEntry(nameTableAttribute);

                var color = patternTileByte switch
                {
                    0 => palette.Byte1,
                    1 => palette.Byte2,
                    2 => palette.Byte3,
                    3 => palette.Byte4,
                    _ => throw new InvalidOperationException(),
                };

                var pixelIndex = _currentScanline * 256 + (_currentCycle - 1);
                CurrentImageBuffer[pixelIndex] = color;
            }

            // Increment the scanline and cycle.
            _currentCycle++;

            if (_currentCycle > 340)
            {
                _currentCycle = 0;
                _currentScanline++;

                if (_currentScanline > 260)
                {
                    _currentScanline = -1;
                }
            }

            // Determine what value to return.
            if (_currentScanline == -1 && _currentCycle == 0)
            {
                (CurrentImageBuffer, PreviousImageBuffer) = (PreviousImageBuffer, CurrentImageBuffer); // Swap the buffers.

                FrameComplete?.Invoke(this, EventArgs.Empty);

                return PpuClockResult.FrameComplete;
            }
            else if (_currentScanline == 241 && _currentCycle == 1)
            {
                return PpuClockResult.VBlankStart;
            }
            else
            {
                return PpuClockResult.NormalCycle;
            }
        }

        private byte FetchNameTableTileId()
        {
            // This code was directly taken from https://wiki.nesdev.com/w/index.php/PPU_scrolling.

            var address = (ushort)(0x2000 | (Registers.CurrentAddress & 0x0fff));

            return PpuRead(address);
        }

        private byte FetchNameTableAttribute()
        {
            // This code was directly taken from https://wiki.nesdev.com/w/index.php/PPU_scrolling with some additions at the end.

            var address = (ushort)(0x23c0 | (Registers.CurrentAddress & 0x0c00) | ((Registers.CurrentAddress >> 4) & 0x38) | ((Registers.CurrentAddress >> 2) & 0x07));
            var attribute = PpuRead(address);


            if ((Registers.CurrentAddress.CourseY & 0x02) != 0)
            {
                attribute >>= 4;
            }

            if ((Registers.CurrentAddress.CourseX & 0x02) != 0)
            {
                attribute >>= 2;
            }

            return (byte)(attribute & 0x03);
        }

        private byte FetchLowPatternByte(byte nameTableTileId)
        {
            var address = (ushort)((nameTableTileId << 4) + Registers.CurrentAddress.FineY + 0);

            if (Registers.BackgroundPatternTableAddress)
            {
                address += 0x1000;
            }

            return PpuRead(address);
        }

        private byte FetchHighPatternByte(byte nameTableTileId)
        {
            var address = (ushort)((nameTableTileId << 4) + Registers.CurrentAddress.FineY + 8);

            if (Registers.BackgroundPatternTableAddress)
            {
                address += 0x1000;
            }

            return PpuRead(address);
        }

        private void LoadShiftRegisters()
        {
            _shiftNameTableAttributeLow = (ushort)((_shiftNameTableAttributeLow & 0xff00) | ((_nextNameTableAttribute & 0b01) != 0 ? 0xff : 0x00));
            _shiftNameTableAttributeHigh = (ushort)((_shiftNameTableAttributeHigh & 0xff00) | ((_nextNameTableAttribute & 0b10) != 0 ? 0xff : 0x00));

            _shiftLowPatternByte = (ushort)((_shiftLowPatternByte & 0xff00) | _nextLowPatternByte);
            _shiftHighPatternByte = (ushort)((_shiftHighPatternByte & 0xff00) | _nextHighPatternByte);
        }

        private void UpdateShiftRegisters()
        {
            _shiftNameTableAttributeLow <<= 1;
            _shiftNameTableAttributeHigh <<= 1;

            _shiftLowPatternByte <<= 1;
            _shiftHighPatternByte <<= 1;
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
                        data = Registers.ReadStatusRegister();
                        return true;

                    case 7:
                        // TODO: There is some tricky stuff that needs to be handled here dealing with what
                        // the PPU is currently doing.

                        if (Registers.CurrentAddress >= 0x3f00 && Registers.CurrentAddress <= 0x3fff)
                        {
                            // TODO: This is actually wrong. The palette data is returned directly and the
                            // read buffer is updated with name table data.

                            // Palette memory is read immediately.
                            Registers.ReadBuffer = PpuRead(Registers.CurrentAddress);

                            data = Registers.ReadBuffer;
                        }
                        else
                        {
                            // All other memory is delayed by one call.
                            data = Registers.ReadBuffer;

                            // Update the buffer with new data only after the current buffer is read.
                            Registers.ReadBuffer = PpuRead(Registers.CurrentAddress);
                        }

                        // Increment the address by either 1 or 32 depending on the VRAM address increment flag.
                        Registers.CurrentAddress += !Registers.VramAddressIncrement ? (ushort)1 : (ushort)32;

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
                        Registers.WriteControlRegister(data);
                        break;

                    case 1:
                        Registers.WriteMaskRegister(data);
                        break;

                    case 5:
                        Registers.WriteScroll(data);
                        break;

                    case 6:
                        Registers.WriteAddress(data);
                        break;

                    case 7:
                        PpuWrite(Registers.CurrentAddress, data);

                        // Increment the address by either 1 or 32 depending on the VRAM address increment flag.
                        Registers.CurrentAddress += !Registers.VramAddressIncrement ? (ushort)1 : (ushort)32;

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