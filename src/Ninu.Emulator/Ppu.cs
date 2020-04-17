using System;

namespace Ninu.Emulator
{
    public delegate void FrameCompleteHandler(object source, EventArgs e);

    public class Ppu : ICpuBusComponent
    {
        private readonly Cartridge _cartridge;
        private readonly NameTableRam _nameTableRam;

        public PaletteRam PaletteRam { get; } = new PaletteRam();

        public Oam Oam { get; } = new Oam(64);
        public Oam TemporaryOam { get; } = new Oam(8);

        private byte _oamAddress;

        public PpuRegisters Registers { get; } = new PpuRegisters();

        public bool CallNmi { get; set; }

        private int _cycle;
        private int _scanline;
        private ushort _readAddress;

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

        //private bool _odd;

        public PpuClockResult Clock()
        {
            //if (_scanline == -1 && _cycle == 0)
            //{
            //    if (_odd)
            //    {
            //        _cycle = 1;
            //    }

            //    _odd = !_odd;
            //}

            if (_scanline == -1 && _cycle == 1)
            {
                Registers.SpriteOverflow = false;
                Registers.Sprite0Hit = false;
                Registers.VerticalBlankStarted = false;
            }

            if (Registers.RenderingEnabled)
            {
                if (_scanline >= -1 && _scanline <= 239) // All of the rendering scanlines.
                {
                    // Update the shift registers.
                    if ((_cycle >= 2 && _cycle <= 257) || (_cycle >= 322 && _cycle <= 337))
                    {
                        UpdateShiftRegisters();
                    }

                    if ((_cycle >= 2 && _cycle <= 257) || (_cycle >= 321 && _cycle <= 337))
                    {
                        switch ((_cycle - 1) % 8)
                        {
                            case 0:
                                LoadShiftRegisters();

                                _readAddress = 0x2000 | (Registers.CurrentAddress & 0x0fff);
                                break;

                            case 1:
                                _nextNameTableTileId = PpuRead(_readAddress);
                                break;

                            case 2:
                                _readAddress = 0x23c0 | (Registers.CurrentAddress & 0x0c00) | ((Registers.CurrentAddress >> 4) & 0x38) | ((Registers.CurrentAddress >> 2) & 0x07);
                                break;

                            case 3:
                                var attribute = PpuRead(_readAddress);

                                if ((Registers.CurrentAddress.CourseY & 0x02) != 0)
                                {
                                    attribute >>= 4;
                                }

                                if ((Registers.CurrentAddress.CourseX & 0x02) != 0)
                                {
                                    attribute >>= 2;
                                }

                                _nextNameTableAttribute = (byte)(attribute & 0x03);
                                break;

                            case 4:
                                _readAddress = (ushort)((_nextNameTableTileId << 4) + Registers.CurrentAddress.FineY + 0);

                                if (Registers.BackgroundPatternTableAddress)
                                {
                                    _readAddress += 0x1000;
                                }

                                break;

                            case 5:
                                _nextLowPatternByte = PpuRead(_readAddress);
                                break;

                            case 6:
                                _readAddress = (ushort)((_nextNameTableTileId << 4) + Registers.CurrentAddress.FineY + 8);

                                if (Registers.BackgroundPatternTableAddress)
                                {
                                    _readAddress += 0x1000;
                                }

                                break;

                            case 7:
                                _nextHighPatternByte = PpuRead(_readAddress);

                                Registers.IncrementX();
                                break;
                        }
                    }

                    if (_cycle == 256)
                    {
                        Registers.IncrementY();
                    }

                    if (_cycle == 257)
                    {
                        Registers.TransferX();
                    }

                    // These reads do nothing but the real PPU does perform them.
                    if (_cycle == 337 || _cycle == 339)
                    {
                        _readAddress = _readAddress = 0x2000 | (Registers.CurrentAddress & 0x0fff);
                    }

                    if (_cycle == 338 || _cycle == 340)
                    {
                        _nextNameTableTileId = PpuRead(_readAddress);
                    }
                }

                if (_scanline == -1 && _cycle >= 280 && _cycle <= 304)
                {
                    Registers.TransferY();
                }

                if (Registers.RenderSprites && _cycle == 340)
                {
                    // TODO: Should this be called on the final visible scanline? It would load sprites for the
                    // next scanline which isn't visible.

                    // Initialize the temporary OAM to 0xff.
                    TemporaryOam.ResetAllData(0xff);

                    // Find all sprites that will need to be rendered for the next scanline.
                    var insertIndex = 0;

                    foreach (var sprite in Oam.Sprites)
                    {
                        var nextScanline = _scanline + 1;

                        // TODO: Are we checking the correct Y coordinate? This could be off by one.
                        if (nextScanline >= sprite.Y && nextScanline <= sprite.Y + 7)
                        {
                            if (insertIndex == 8)
                            {
                                // We have overflowed. No need to check any other sprites.

                                // TODO: Set the overflow flag?
                                break;
                            }

                            sprite.CopyTo(TemporaryOam.Sprites[insertIndex]);

                            insertIndex++;
                        }
                    }
                }
            }

            if (_scanline == 241 && _cycle == 1)
            {
                Registers.VerticalBlankStarted = true;

                if (Registers.GenerateVerticalBlankingIntervalNmi)
                {
                    CallNmi = true;
                }
            }

            if (_scanline >= 0 && _scanline <= 239 && _cycle >= 1 && _cycle <= 256)
            {
                byte backgroundPaletteEntryIndex = 0;
                byte spritePaletteEntryIndex = 0;

                byte backgroundColor = 0;
                byte spriteColor = 0;

                bool spritePriority = true;

                if (Registers.RenderBackground)
                {
                    var shiftSelect = (ushort)0x8000; // By default we are interested in the most significant bit in the shift registers.

                    // The fine X register controls our offset into the shift registers.
                    shiftSelect >>= Registers.FineX;

                    // Extract the data from the pattern tile shift registers.
                    var patternTileLowBit = (_shiftLowPatternByte & shiftSelect) != 0 ? (byte)0b01 : (byte)0b00;
                    var patternTileHighBit = (_shiftHighPatternByte & shiftSelect) != 0 ? (byte)0b10 : (byte)0b00;

                    var paletteEntryIndex = (byte)(patternTileLowBit | patternTileHighBit); // This represents the index into palette (0-3).

                    // Extract the data from the name table attribute shift registers.
                    var nameTableAttributeLowBit = (_shiftNameTableAttributeLow & shiftSelect) != 0 ? (byte)0b01 : (byte)0b00;
                    var nameTableAttributeHighBit = (_shiftNameTableAttributeHigh & shiftSelect) != 0 ? (byte)0b10 : (byte)0b00;

                    var paletteIndex = (byte)(nameTableAttributeLowBit | nameTableAttributeHighBit); // This represents which palette we are using (0-3).

                    // Set the pixel color.
                    backgroundPaletteEntryIndex = paletteEntryIndex;
                    backgroundColor = GetPaletteColor(false, paletteIndex, backgroundPaletteEntryIndex);
                }

                // Calculate sprite zero hit.
                if (Registers.RenderingEnabled && !Registers.Sprite0Hit)
                {
                    if (_cycle >= 0 && _cycle <= 7 && (!Registers.RenderBackgroundInLeftMost8PixelsOfScreen || !Registers.RenderSpritesInLeftMost8PixelsOfScreen))
                    {
                        goto skipSpriteZero;
                    }

                    if (_cycle == 255)
                    {
                        goto skipSpriteZero;
                    }

                    if (backgroundPaletteEntryIndex == 0)
                    {
                        goto skipSpriteZero;
                    }

                    var sprite0 = Oam.Sprites[0];

                    if (_scanline >= sprite0.Y && _scanline <= sprite0.Y + 7)
                    {
                        if (_cycle >= sprite0.X && _cycle <= sprite0.X + 7)
                        {
                            // TODO: Handle the reading of the pattern through the bus.
                            var tile = GetPatternTile(PatternTableEntry.Left, sprite0.TileIndex);

                            var xIndex = _cycle - sprite0.X;
                            var yIndex = _scanline - sprite0.Y;

                            if (sprite0.FlipHorizontal)
                            {
                                xIndex = 7 - xIndex;
                            }

                            if (sprite0.FlipVertical)
                            {
                                yIndex = 7 - yIndex;
                            }

                            var colorIndex = tile.GetPaletteColorIndex(xIndex, yIndex);

                            if (colorIndex != 0)
                            {
                                Registers.Sprite0Hit = true;
                            }
                        }
                    }
                }

                skipSpriteZero:

                if (Registers.RenderSprites && _scanline != 0) // Can't write sprites on the first scanline.
                {
                    foreach (var sprite in TemporaryOam.Sprites)
                    {
                        if (sprite.X == 0xff)
                        {
                            continue;
                        }

                        if (_cycle >= sprite.X && _cycle <= sprite.X + 7)
                        {
                            // TODO: Handle the reading of the pattern through the bus.
                            var tile = GetPatternTile(Registers.SpritePatternTableAddressFor8X8 ? PatternTableEntry.Right : PatternTableEntry.Left, sprite.TileIndex);

                            spritePriority = sprite.Priority;

                            var xIndex = _cycle - sprite.X;
                            var yIndex = _scanline - sprite.Y;

                            if (sprite.FlipHorizontal)
                            {
                                xIndex = 7 - xIndex;
                            }

                            if (sprite.FlipVertical)
                            {
                                yIndex = 7 - yIndex;
                            }

                            // Set the pixel color.
                            spritePaletteEntryIndex = (byte)tile.GetPaletteColorIndex(xIndex, yIndex);
                            spriteColor = GetPaletteColor(true, sprite.PaletteIndex, spritePaletteEntryIndex);

                            // This simulates priority between the sprites. Sprites first in the list have priority over later sprites.
                            if (spritePaletteEntryIndex != 0)
                            {
                                break;
                            }
                        }
                    }
                }

                var pixelIndex = _scanline * 256 + (_cycle - 1);

                // Simulate the priority muxer.
                var pixelColor = backgroundColor;

                if (backgroundPaletteEntryIndex == 0 && spritePaletteEntryIndex != 0)
                {
                    pixelColor = spriteColor;
                }
                else if (backgroundPaletteEntryIndex != 0 && spritePaletteEntryIndex == 0)
                {
                    pixelColor = backgroundColor;
                }
                else if (backgroundPaletteEntryIndex != 0 && spritePaletteEntryIndex != 0 && !spritePriority)
                {
                    pixelColor = spriteColor;
                }
                else if (backgroundPaletteEntryIndex != 0 && spritePaletteEntryIndex != 0 && spritePriority)
                {
                    pixelColor = backgroundColor;
                }

                CurrentImageBuffer[pixelIndex] = pixelColor;
            }

            // Increment the scanline and cycle.
            _cycle++;

            if (_cycle > 340)
            {
                _cycle = 0;
                _scanline++;

                if (_scanline > 260)
                {
                    _scanline = -1;
                }
            }

            // Determine what value to return.
            if (_scanline == 260 && _cycle == 340)
            {
                (CurrentImageBuffer, PreviousImageBuffer) = (PreviousImageBuffer, CurrentImageBuffer); // Swap the buffers.

                return PpuClockResult.FrameComplete;
            }
            else if (_scanline == 241 && _cycle == 1)
            {
                return PpuClockResult.VBlankStart;
            }
            else
            {
                return PpuClockResult.NormalCycle;
            }
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

        public byte GetPaletteColor(bool isSprite, byte paletteIndex, byte paletteEntryIndex)
        {
            var paletteAddress = (ushort)(0x3F00 + (paletteIndex * 4) + paletteEntryIndex);

            if (isSprite)
            {
                // The sprite palettes start after the four background palettes which are four bytes each.
                paletteAddress += 16;
            }

            return (byte)(PpuRead(paletteAddress) & 0x3F);
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

                    case 4:
                        // TODO: Implement the weirdness of reading this register.
                        data = Oam.Read(_oamAddress);
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

                    case 3:
                        _oamAddress = data;
                        break;

                    case 4:
                        Oam.Write(_oamAddress++, data); // Writing increments the OAM address, reading does not.
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