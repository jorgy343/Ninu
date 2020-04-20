using Microsoft.Extensions.Logging;
using System;

namespace Ninu.Emulator
{
    public class Ppu : ICpuBusComponent, IPersistable
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger _logger;

        private readonly Cartridge _cartridge;
        private readonly NameTableRam _nameTableRam = new NameTableRam();

        public PaletteRam PaletteRam { get; } = new PaletteRam();

        public PpuRegisters Registers { get; } = new PpuRegisters();

        public Oam Oam { get; } = new Oam(64);
        public Oam TemporaryOam { get; } = new Oam(8);

        private byte _oamAddress;
        private bool _sprite0HitPossible;

        public bool CallNmi { get; set; }

        private int _cycle;
        private int _scanline;
        private ushort _readAddress;

        private readonly PpuBackgroundState _backgroundState = new PpuBackgroundState();

        public byte[] CurrentImageBuffer { get; private set; } = new byte[256 * 240];
        public byte[] PreviousImageBuffer { get; private set; } = new byte[256 * 240];

        public Ppu(Cartridge cartridge, ILoggerFactory loggerFactory, ILogger logger)
        {
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _cartridge = cartridge ?? throw new ArgumentNullException(nameof(cartridge));
        }

        public void Reset()
        {

        }

        private bool _odd;

        public PpuClockResult Clock()
        {
            if (_scanline == 0 && _cycle == 0)
            {
                if (_odd)
                {
                    _cycle = 1;
                }

                _odd = !_odd;
            }

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
                    if ((_cycle >= 1 && _cycle <= 256) || (_cycle >= 321 && _cycle <= 336))
                    {
                        switch ((_cycle - 1) % 8)
                        {
                            case 0:
                                LoadShiftRegisters();

                                _readAddress = 0x2000 | (Registers.VAddress & 0x0fff);
                                break;

                            case 1:
                                _backgroundState.NextNameTableTileId = PpuRead(_readAddress);
                                break;

                            case 2:
                                _readAddress = 0x23c0 | (Registers.VAddress & 0x0c00) | ((Registers.VAddress >> 4) & 0x38) | ((Registers.VAddress >> 2) & 0x07);
                                break;

                            case 3:
                                var attribute = PpuRead(_readAddress);

                                if ((Registers.VAddress.CourseY & 0x02) != 0)
                                {
                                    attribute >>= 4;
                                }

                                if ((Registers.VAddress.CourseX & 0x02) != 0)
                                {
                                    attribute >>= 2;
                                }

                                _backgroundState.NextNameTableAttribute = (byte)(attribute & 0x03);
                                break;

                            case 4:
                                _readAddress = (ushort)(_backgroundState.NextNameTableTileId * 16 + Registers.VAddress.FineY + 0);

                                if (Registers.BackgroundPatternTableAddress)
                                {
                                    _readAddress += 0x1000;
                                }

                                break;

                            case 5:
                                _backgroundState.NextLowPatternByte = PpuRead(_readAddress);
                                break;

                            case 6:
                                _readAddress = (ushort)(_backgroundState.NextNameTableTileId * 16 + Registers.VAddress.FineY + 8);

                                if (Registers.BackgroundPatternTableAddress)
                                {
                                    _readAddress += 0x1000;
                                }

                                break;

                            case 7:
                                _backgroundState.NextHighPatternByte = PpuRead(_readAddress);

                                Registers.IncrementX();
                                break;
                        }
                    }

                    // Update the shift registers.
                    if ((_cycle >= 1 && _cycle <= 256) || (_cycle >= 321 && _cycle <= 336))
                    {
                        UpdateShiftRegisters();
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
                        _readAddress = _readAddress = 0x2000 | (Registers.VAddress & 0x0fff);
                    }

                    if (_cycle == 338 || _cycle == 340)
                    {
                        _backgroundState.NextNameTableTileId = PpuRead(_readAddress);
                    }
                }

                if (_scanline == -1 && _cycle >= 280 && _cycle <= 304)
                {
                    Registers.TransferY();
                }

                if (Registers.RenderSprites && _cycle == 340)
                {
                    // TODO: Should this be called on the final visible scanline? It would load sprites for the next
                    // scanline which isn't visible.

                    // Initialize the temporary OAM to 0xff.
                    TemporaryOam.ResetAllData(0xff);

                    // Find all sprites that will need to be rendered for the next scanline.
                    var insertIndex = 0;

                    var spriteHeight = Registers.SpriteSize ? 16 : 8;

                    for (var i = 0; i < Oam.Sprites.Length; i++)
                    {
                        var sprite = Oam.Sprites[i];
                        var nextScanline = _scanline + 1;

                        if (nextScanline >= sprite.Y && nextScanline <= sprite.Y + spriteHeight - 1)
                        {
                            if (insertIndex == 8)
                            {
                                // We have overflowed. No need to check any other sprites.

                                // TODO: Set the overflow flag correctly (i.e. the buggy way).
                                Registers.SpriteOverflow = true;
                                break;
                            }

                            if (i == 0)
                            {
                                _sprite0HitPossible = true;
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
                byte backgroundPaletteIndex = 0;
                byte backgroundPaletteEntryIndex = 0;

                byte spritePaletteIndex = 0;
                byte spritePaletteEntryIndex = 0;

                var spritePriority = true;

                if (Registers.RenderBackground)
                {
                    if (Registers.RenderBackgroundInLeftMost8PixelsOfScreen || _cycle >= 8)
                    {
                        var shiftSelect = (ushort)0x8000; // By default we are interested in the most significant bit in the shift registers.

                        // The fine X register controls our offset into the shift registers.
                        shiftSelect >>= Registers.FineX;

                        // Extract the data from the pattern tile shift registers.
                        var patternTileLowBit = (_backgroundState.ShiftLowPatternByte & shiftSelect) != 0 ? (byte)0b01 : (byte)0b00;
                        var patternTileHighBit = (_backgroundState.ShiftHighPatternByte & shiftSelect) != 0 ? (byte)0b10 : (byte)0b00;

                        var paletteEntryIndex = (byte)(patternTileLowBit | patternTileHighBit); // This represents the index into palette (0-3).

                        // Extract the data from the name table attribute shift registers.
                        var nameTableAttributeLowBit = (_backgroundState.ShiftNameTableAttributeLow & shiftSelect) != 0 ? (byte)0b01 : (byte)0b00;
                        var nameTableAttributeHighBit = (_backgroundState.ShiftNameTableAttributeHigh & shiftSelect) != 0 ? (byte)0b10 : (byte)0b00;

                        var paletteIndex = (byte)(nameTableAttributeLowBit | nameTableAttributeHighBit); // This represents which palette we are using (0-3).

                        // Set the pixel color.
                        backgroundPaletteIndex = paletteIndex;
                        backgroundPaletteEntryIndex = paletteEntryIndex;
                    }
                }

                var spriteZeroRendered = false;

                if (Registers.RenderSprites && _scanline != 0) // Can't write sprites on the first scanline.
                {
                    if (Registers.RenderSpritesInLeftMost8PixelsOfScreen || _cycle >= 8) // Clip the left side of the screen if necessary.
                    {
                        var spriteHeight = Registers.SpriteSize ? 16 : 8;

                        for (var i = 0; i < TemporaryOam.Sprites.Length; i++)
                        {
                            var sprite = TemporaryOam.Sprites[i];

                            if (sprite.X == 0xff)
                            {
                                continue;
                            }

                            if (_cycle >= sprite.X && _cycle <= sprite.X + 7)
                            {
                                spritePriority = sprite.Priority;

                                var xIndex = _cycle - sprite.X;
                                var yIndex = _scanline - sprite.Y;

                                if (sprite.FlipHorizontal)
                                {
                                    xIndex = 7 - xIndex;
                                }

                                if (sprite.FlipVertical)
                                {
                                    yIndex = spriteHeight - 1 - yIndex;
                                }

                                // TODO: Handle the reading of the pattern through the bus.
                                PatternTableEntry patternTableEntry;

                                if (Registers.SpriteSize)
                                {
                                    patternTableEntry = (sprite.TileIndex & 0x01) != 0 ? PatternTableEntry.Right : PatternTableEntry.Left;
                                }
                                else
                                {
                                    patternTableEntry = Registers.SpritePatternTableAddressFor8X8 ? PatternTableEntry.Right : PatternTableEntry.Left;
                                }

                                PatternTile tile;

                                if (yIndex <= 7)
                                {
                                    tile = GetPatternTile(patternTableEntry, sprite.TileIndex);
                                }
                                else
                                {
                                    tile = GetPatternTile(patternTableEntry, sprite.TileIndex + 1);
                                }

                                // Set the pixel color.
                                spritePaletteIndex = (byte)(sprite.PaletteIndex + 4);
                                spritePaletteEntryIndex = tile.GetPaletteColorIndex(xIndex, yIndex > 7 ? yIndex - 8 : yIndex);

                                if (i == 0 && spritePaletteEntryIndex != 0)
                                {
                                    spriteZeroRendered = true;
                                }

                                // This simulates priority between the sprites. Sprites first in the list have priority over later sprites.
                                if (spritePaletteEntryIndex != 0)
                                {
                                    break;
                                }
                            }
                        }
                    }
                }

                var pixelIndex = _scanline * 256 + (_cycle - 1);

                byte pixelPaletteIndex = 0;
                byte pixelPaletteEntryIndex = 0;

                if (backgroundPaletteEntryIndex == 0 && spritePaletteEntryIndex != 0) // Transparent background, non transparent sprite.
                {
                    pixelPaletteIndex = spritePaletteIndex;
                    pixelPaletteEntryIndex = spritePaletteEntryIndex;
                }
                else if (backgroundPaletteEntryIndex != 0 && spritePaletteEntryIndex == 0) // Non transparent background, transparent sprite.
                {
                    pixelPaletteIndex = backgroundPaletteIndex;
                    pixelPaletteEntryIndex = backgroundPaletteEntryIndex;
                }
                else if (backgroundPaletteEntryIndex != 0 && spritePaletteEntryIndex != 0) // Non transparent background and non transparent sprite.
                {
                    if (spritePriority) // Yes, these seems like it should be the opposite, but this is correct.
                    {
                        pixelPaletteIndex = backgroundPaletteIndex;
                        pixelPaletteEntryIndex = backgroundPaletteEntryIndex;
                    }
                    else
                    {
                        pixelPaletteIndex = spritePaletteIndex;
                        pixelPaletteEntryIndex = spritePaletteEntryIndex;
                    }

                    // Now we need to calculate a sprite 0 hit.
                    if (_sprite0HitPossible && spriteZeroRendered)
                    {
                        if (Registers.RenderingEnabled)
                        {
                            if (_cycle >= 1 && _cycle <= 254)
                            {
                                if (Registers.RenderBackgroundInLeftMost8PixelsOfScreen || Registers.RenderSpritesInLeftMost8PixelsOfScreen)
                                {
                                    if (_cycle >= 8)
                                    {
                                        Registers.Sprite0Hit = true;
                                    }
                                }
                                else
                                {
                                    Registers.Sprite0Hit = true;
                                }
                            }
                        }
                    }
                }

                CurrentImageBuffer[pixelIndex] = GetPaletteColor(pixelPaletteIndex, pixelPaletteEntryIndex);
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
            _backgroundState.ShiftNameTableAttributeLow = (ushort)((_backgroundState.ShiftNameTableAttributeLow & 0xff00) | ((_backgroundState.NextNameTableAttribute & 0b01) != 0 ? 0xff : 0x00));
            _backgroundState.ShiftNameTableAttributeHigh = (ushort)((_backgroundState.ShiftNameTableAttributeHigh & 0xff00) | ((_backgroundState.NextNameTableAttribute & 0b10) != 0 ? 0xff : 0x00));

            _backgroundState.ShiftLowPatternByte = (ushort)((_backgroundState.ShiftLowPatternByte & 0xff00) | _backgroundState.NextLowPatternByte);
            _backgroundState.ShiftHighPatternByte = (ushort)((_backgroundState.ShiftHighPatternByte & 0xff00) | _backgroundState.NextHighPatternByte);
        }

        private void UpdateShiftRegisters()
        {
            _backgroundState.ShiftNameTableAttributeLow <<= 1;
            _backgroundState.ShiftNameTableAttributeHigh <<= 1;

            _backgroundState.ShiftLowPatternByte <<= 1;
            _backgroundState.ShiftHighPatternByte <<= 1;
        }

        /// <summary>
        /// Gets a color from the palette based on the palette index (which palette which are interested in) and the
        /// palette entry index (which entry within the selected palette we are interested in). The first four palettes
        /// are for the background and the next four palettes are for the sprites. When you have a sprite palette
        /// index, you need to add four to the index before passing it into this method in order to get the correct
        /// sprite palette.
        /// </summary>
        /// <param name="paletteIndex">The index (0-7) of the palette to get.</param>
        /// <param name="paletteEntryIndex">The index (0-3) of the entry within the palette to get.</param>
        /// <returns>The color which will always be a number 0-63.</returns>
        public byte GetPaletteColor(byte paletteIndex, byte paletteEntryIndex)
        {
            var address = (ushort)(0x3F00 + (paletteIndex * 4) + paletteEntryIndex);

            return (byte)(PpuRead(address) & 0x3f); // The & ensures that the number is between 0-63.
        }

        public PatternTile GetPatternTile(PatternTableEntry entry, int index)
        {
            if (index < 0 || index > 255) throw new ArgumentOutOfRangeException(nameof(index));

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
                        // TODO: There is some tricky stuff that needs to be handled here dealing with what the PPU is
                        // currently doing.

                        if (Registers.VAddress >= 0x3f00 && Registers.VAddress <= 0x3fff)
                        {
                            // TODO: This is actually wrong. The palette data is returned directly and the read buffer
                            // is updated with name table data.

                            // Palette memory is read immediately.
                            Registers.ReadBuffer = PpuRead(Registers.VAddress);

                            data = Registers.ReadBuffer;
                        }
                        else
                        {
                            // All other memory is delayed by one call.
                            data = Registers.ReadBuffer;

                            // Update the buffer with new data only after the current buffer is read.
                            Registers.ReadBuffer = PpuRead(Registers.VAddress);
                        }

                        // Increment the address by either 1 or 32 depending on the VRAM address increment flag.
                        Registers.VAddress += !Registers.VramAddressIncrement ? (ushort)1 : (ushort)32;

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
                        PpuWrite(Registers.VAddress, data);

                        // Increment the address by either 1 or 32 depending on the VRAM address increment flag.
                        Registers.VAddress += !Registers.VramAddressIncrement ? (ushort)1 : (ushort)32;

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

            if (_nameTableRam.PpuRead(_cartridge.GetMirrorMode(), address, out data))
            {
                return data;
            }

            if (PaletteRam.PpuRead(address, out data))
            {
                // TODO: Black colors are supposed to be replaced by gray colors.
                if (Registers.EnableGrayscale)
                {
                    data &= 0x30;
                }

                return data;
            }

            return 0;
        }

        public void PpuWrite(ushort address, byte data)
        {
            address &= 0x3fff; // Ensure we never write outside of the PPU bus's address range.

            _cartridge.PpuWrite(address, data);
            _nameTableRam.PpuWrite(_cartridge.GetMirrorMode(), address, data);

            PaletteRam.PpuWrite(address, data);
        }

        public void SaveState(SaveStateContext context)
        {
            context.AddToState("Ppu.OamAddress", _oamAddress);
            context.AddToState("Ppu.Sprite0HitPossible", _sprite0HitPossible);

            context.AddToState("Ppu.CallNmi", CallNmi);

            context.AddToState("Ppu.Cycle", _cycle);
            context.AddToState("Ppu.Scanline", _scanline);
            context.AddToState("Ppu.ReadAddress", _readAddress);

            _nameTableRam.SaveState(context);

            Oam.SaveState(context);
            //TemporaryOam.SaveState(context);

            PaletteRam.SaveState(context);

            Registers.SaveState(context);

            _backgroundState.SaveState(context);
        }

        public void LoadState(SaveStateContext context)
        {
            //_oamAddress = context.GetFromState<byte>("Ppu.OamAddress");
            //_sprite0HitPossible = context.GetFromState<bool>("Ppu.Sprite0HitPossible");
            //
            //CallNmi = context.GetFromState<bool>("Ppu.CallNmi");

            //_cycle = context.GetFromState<int>("Ppu.Cycle");
            //_scanline = context.GetFromState<int>("Ppu.Scanline");
            //_readAddress = context.GetFromState<ushort>("Ppu.ReadAddress");

            _nameTableRam.LoadState(context);

            Oam.LoadState(context);
            //TemporaryOam.LoadState(context);

            PaletteRam.LoadState(context);

            Registers.LoadState(context);

            _backgroundState.LoadState(context);
        }
    }
}