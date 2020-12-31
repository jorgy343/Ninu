using Microsoft.Extensions.Logging;
using Ninu.Emulator.CentralProcessor;
using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace Ninu.Emulator.GraphicsProcessor
{
    public class Ppu : ICpuBusComponent
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger _logger;

        private Cartridge? _cartridge; // Don't save this as the console handles that.

        [SaveChildren("NameTableRam")]
        private readonly NameTableRam _nameTableRam = new();

        [SaveChildren]
        public PaletteRam PaletteRam { get; } = new();

        [SaveChildren]
        public PpuRegisters Registers { get; } = new();

        [SaveChildren]
        public Oam Oam { get; } = new(64);

        [SaveChildren]
        public Oam TemporaryOam { get; } = new(8);

        [Save("OamAddress")]
        private byte _oamAddress;

        [Save("Sprite0HitPossible")]
        private bool _sprite0HitPossible;

        [Save]
        public bool Nmi { get; set; }

        [Save("Cycle")]
        private int _cycle;

        [Save("Scanline")]
        private int _scanline;

        [Save("ReadAddress")]
        private ushort _readAddress;

        [SaveChildren("BackgroundState")]
        private readonly PpuBackgroundState _backgroundState = new();

        [Save("Odd")]
        private bool _odd;

        public byte[] CurrentImageBuffer { get; private set; } = new byte[256 * 240];
        public byte[] PreviousImageBuffer { get; private set; } = new byte[256 * 240];

        public Ppu(ILoggerFactory loggerFactory, ILogger logger)
        {
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void PowerOn()
        {
            Registers.Control = 0x00;
            Registers.Mask = 0x00;
            Registers.Status = 0x00;
            Registers.VAddress = 0x0000;
            Registers.TAddress = 0x0000;

            Registers.WriteLatch = false;
            _oamAddress = 0x00;

            _odd = false;
        }

        public void Reset()
        {
            Registers.Control = 0x00;
            Registers.Mask = 0x00;
            Registers.Status = 0x00;
            Registers.VAddress = 0x0000; // TODO: I think part of this is unchanged.
            Registers.TAddress = 0x0000; // TODO: I think part of this is unchanged.

            Registers.WriteLatch = false;
            _oamAddress = 0x00;

            _odd = false;
        }

        public void LoadCartridge(Cartridge cartridge)
        {
            _cartridge = cartridge ?? throw new ArgumentNullException(nameof(cartridge));
        }

        public ClockResult Clock()
        {
            if (_cartridge is null)
            {
                return ClockResult.Nothing;
            }

            if (_scanline == 0 && _cycle == 0)
            {
                // On odd frames, the first cycle on scanline 0 is skipped, but only if rendering
                // is enabled. If rendering is disabled, every frame is the same amount of cycles.
                if (Registers.RenderingEnabled && _odd)
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
                if (_scanline >= 0 && _scanline <= 239 && _cycle >= 1 && _cycle <= 256)
                {
                    byte backgroundPaletteIndex = 0;
                    var backgroundPaletteEntryIndex = new PaletteEntryIndex(0);

                    byte spritePaletteIndex = 0;
                    var spritePaletteEntryIndex = new PaletteEntryIndex(0);

                    var spritePriority = true;
                    var spriteZeroRendered = false;

                    // Render the background.
                    if (Registers.RenderBackground)
                    {
                        if (Registers.RenderBackgroundInLeftMost8PixelsOfScreen || _cycle >= 8)
                        {
                            var shiftSelect = (ushort)0x8000; // By default we are interested in the most significant bit in the shift registers.

                            // The fine X register controls our offset into the shift registers.
                            shiftSelect >>= Registers.FineX;

                            // Extract the data from the pattern tile shift registers.
                            var patternTileLowBit = (_backgroundState.ShiftLowPatternByte & shiftSelect) != 0 ? (byte)0x1 : (byte)0x0;
                            var patternTileHighBit = (_backgroundState.ShiftHighPatternByte & shiftSelect) != 0 ? (byte)0x1 : (byte)0x0;

                            var paletteEntryIndex = new PaletteEntryIndex(patternTileLowBit, patternTileHighBit); // This represents the index into the palette (0-3).

                            // Extract the data from the name table attribute shift registers.
                            var nameTableAttributeLowBit = (_backgroundState.ShiftNameTableAttributeLow & shiftSelect) != 0 ? (byte)0b01 : (byte)0b00;
                            var nameTableAttributeHighBit = (_backgroundState.ShiftNameTableAttributeHigh & shiftSelect) != 0 ? (byte)0b10 : (byte)0b00;

                            var paletteIndex = (byte)(nameTableAttributeLowBit | nameTableAttributeHighBit); // This represents which palette we are using (0-3).

                            // Set the pixel color.
                            backgroundPaletteIndex = paletteIndex;
                            backgroundPaletteEntryIndex = paletteEntryIndex;
                        }
                    }

                    // Render the sprites.
                    if (Registers.RenderSprites && _scanline != 0) // Can't write sprites on the first scanline.
                    {
                        if (Registers.RenderSpritesInLeftMost8PixelsOfScreen || _cycle >= 8) // Clip the left side of the screen if necessary.
                        {
                            var spriteHeight = Registers.SpriteSize ? 16 : 8;

                            for (var i = 0; i < TemporaryOam.Sprites.Length; i++)
                            {
                                var sprite = TemporaryOam.Sprites[i];

                                // Since the secondary OAM is completely cleared to 0xff, if the
                                // sprite's X coordinate is 0xff that means a sprite wasn't found
                                // in the primary OAM so we can skip it.
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

                                    PatternTableOffset patternTableOffset;

                                    if (Registers.SpriteSize)
                                    {
                                        // 8x16
                                        patternTableOffset = (sprite.TileIndex & 0x01) != 0 ? PatternTableOffset.Right : PatternTableOffset.Left;
                                    }
                                    else
                                    {
                                        // 8x8
                                        patternTableOffset = Registers.SpritePatternTableAddressFor8X8 ? PatternTableOffset.Right : PatternTableOffset.Left;
                                    }

                                    if (yIndex <= 7)
                                    {
                                        spritePaletteEntryIndex = GetPaletteColorIndex(patternTableOffset, sprite.TileIndex, (byte)xIndex, (byte)yIndex);
                                    }
                                    else
                                    {
                                        // It must be an 8x16 sprite and we are indexing into the lower half of the
                                        // sprite. For 8x16 sprites, the top tile of the sprite is the first tile in
                                        // memory and the bottom tile of the sprite is the tile directly adjacent on
                                        // the right of the first tile.
                                        spritePaletteEntryIndex = GetPaletteColorIndex(patternTableOffset, (byte)(sprite.TileIndex + 1), (byte)xIndex, (byte)(yIndex - 8));
                                    }

                                    // Set the pixel color.
                                    spritePaletteIndex = (byte)(sprite.PaletteIndex + 4);

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

                    // Perform pixel selection and render it to the buffer.
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
                                    if (!Registers.RenderBackgroundInLeftMost8PixelsOfScreen || !Registers.RenderSpritesInLeftMost8PixelsOfScreen)
                                    {
                                        if (_cycle >= 9)
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

                // Perform rendering setup.
                if (_scanline >= -1 && _scanline <= 239) // All of the rendering scanlines.
                {
                    if ((_cycle >= 9 && _cycle <= 257) || (_cycle >= 329 && _cycle <= 337))
                    {
                        if (_cycle % 8 == 1)
                        {
                            LoadShiftRegisters();
                        }
                    }

                    if ((_cycle >= 1 && _cycle <= 256) || (_cycle >= 321 && _cycle <= 336))
                    {
                        switch ((_cycle - 1) % 8)
                        {
                            case 0:
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
                        Registers.TransferX();
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

                // Get the prites ready for the next scanline. This is the correct way do it, but
                // this isn't fully implemented yet.
                //if (Registers.RenderSprites)
                //{
                //    if (_scanline >= -1 && _scanline <= 239) // All of the visible scanlines and the pre-render scanline.
                //    {
                //        // Clear secondary OAM to 0xff. For the first 64 cycles of each visible
                //        // scanline
                //        if (_cycle >= 1 && _cycle <= 64)
                //        {
                //            // The real hardware reads from the OAM memory and writes to the
                //            // secondary OAM memory every other cycle. However, the data line
                //            // during the read is pulled to 0xff so the read itself doesn't do
                //            // anything. We'll skip emulating the read and only emulate the writes.
                //            if (_cycle % 2 == 0)
                //            {
                //                var secondaryOamByteIndex = (_cycle / 2) - 1;

                //                TemporaryOam.Write((byte)secondaryOamByteIndex, 0xff);
                //            }
                //        }
                //    }
                //}

                // This is not the proper way to prepare sprites for the next scanline but it tends
                // to work pretty well.
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

            // The vertical blank nmi is raised on (scanline 241, cycle 1). A CPU access to memory
            // address 0x2002 (PPU STATUS register) near when the PPU raises the nmi, the nmi could
            // be delayed or completely missed. TODO: This is currently not handled.
            if (_scanline == 241 && _cycle == 1)
            {
                Registers.VerticalBlankStarted = true;

                if (Registers.GenerateVerticalBlankingIntervalNmi)
                {
                    Nmi = true;
                }
            }

            // Increment the scanline and cycle.
            _cycle++;

            if (_cycle > 340)
            {
                _cycle = 0;
                _scanline++;

                if (_scanline > 260)
                {
                    // The first scanline is considered to be -1 to simplify the definition of when
                    // a frame begins. Since -1 is the pre-render scanline, it is considered part
                    // of the frame that it is pre-rendering for. Documentation and other emulators
                    // may consider the pre-render scanline to be at the end of the frame and be
                    // scanline 261. According to visual 2c02, considering the pre-render scanline
                    // to be scanline 261 is correct.
                    _scanline = -1;
                }
            }

            // Determine what value to return.
            if (_scanline == 260 && _cycle == 340)
            {
                (CurrentImageBuffer, PreviousImageBuffer) = (PreviousImageBuffer, CurrentImageBuffer); // Swap the buffers.

                return ClockResult.FrameComplete;
            }
            else if (_scanline == 241 && _cycle == 1)
            {
                return ClockResult.VBlankInterruptComplete;
            }
            else
            {
                return ClockResult.NormalPpuCycleComplete;
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
        /// Gets a palette entry index from the pattern table offset, tile index, an X coordinate within the tile, and
        /// a Y coordinate within the tile. The origin for the X and Y coordinates are the top left of the tile. Each
        /// tile is 8 pixels by 8 pixels so both the X and Y coordinates must be an integer between 0 and 7 inclusive.
        /// Each pattern table consists of 256 tiles so the <paramref name="tileIndex"/> must be an integer between 0
        /// and 255 inclusive.
        /// </summary>
        /// <param name="patternTableOffset">Chooses either the left pattern table or right pattern table.</param>
        /// <param name="tileIndex">The tile to pull the palette entry index from.</param>
        /// <param name="x">The X coordinate within the tile.</param>
        /// <param name="y">The Y coordinate within the tile.</param>
        /// <returns>The palette entry index.</returns>
        public PaletteEntryIndex GetPaletteColorIndex(PatternTableOffset patternTableOffset, byte tileIndex, byte x, byte y)
        {
            if (x > 7) throw new ArgumentOutOfRangeException(nameof(x));
            if (y > 7) throw new ArgumentOutOfRangeException(nameof(y));
            if (!Enum.IsDefined(typeof(PatternTableOffset), patternTableOffset)) throw new InvalidEnumArgumentException(nameof(patternTableOffset), (int)patternTableOffset, typeof(PatternTableOffset));

            // Here is a view of half of the tile. This represents the low bit in the 2 bit palette entry index. This
            // is 8 bytes total which, with 8 bits in each byte, allows for 64 pixels.
            //
            //   7   6   5   4   3   2   1   0    <- Bit index into the byte (represents the X axes).
            // +---+---+---+---+---+---+---+---+
            // |   |   |   |   |   |   |   |   | 0 --+
            // +---+---+---+---+---+---+---+---+     |
            // |   |   |   |   |   |   |   |   | 1   |
            // +---+---+---+---+---+---+---+---+     |
            // |   |   |   |   |   |   |   |   | 2   |
            // +---+---+---+---+---+---+---+---+     |
            // |   |   |   |   |   |   |   |   | 3   |
            // +---+---+---+---+---+---+---+---+     | The byte index (represents the Y axes).
            // |   |   |   |   |   |   |   |   | 4   |
            // +---+---+---+---+---+---+---+---+     |
            // |   |   |   |   |   |   |   |   | 5   |
            // +---+---+---+---+---+---+---+---+     |
            // |   |   |   |   |   |   |   |   | 6   |
            // +---+---+---+---+---+---+---+---+     |
            // |   |   |   |   |   |   |   |   | 7 --+
            // +---+---+---+---+---+---+---+---+
            //
            // The byte represents the Y coordinate while the bit represents the X coordinate. It is important to note
            // that the MSB of the byte represents x = 0. This is because the MSB is physically the left most bit.
            //
            //
            // In memory, one complete tile is laid out in 16 bytes:
            //
            // +-------------------------------+-------------------------------+
            // |                               |                               |
            // | First 8 bytes are the low bits| Next 8 bytes are the high bits|
            // |                               |                               |
            // +---+---+---+---+---+---+---+---+---+---+---+---+---+---+---+---+
            // | 0 | 1 | 2 | 3 | 4 | 5 | 6 | 7 | 8 | 9 | A | B | C | D | E | F |  <- Bytes
            // +---+---+---+---+---+---+---+---+---+---+---+---+---+---+---+---+
            //           |                               |
            //           +---------------------+---------+
            //                                 |
            //                                 | If the y coordinate is 2, bytes 2 and A would be chosen. Their bits
            //                                 | shown below represent the palette entry index.
            //                                 |
            // +-------------------------------+-------------------------------+
            // |                               |                               |
            // | First 8 bits are the low bit  | Next 8 bits are the high bits |
            // | in the palette entry index    | in the palette entry index    |
            // +-------------------------------+-------------------------------+
            // |MSB        byte 2           LSB|MSB         byte A          LSB|
            // +-------------------------------+-------------------------------+
            // | 7 | 6 | 5 | 4 | 3 | 2 | 1 | 0 | F | E | D | C | B | A | 9 | 8 |  <- Bits
            // +---+---+---+---+---+---+---+---+---+---+---+---+---+---+---+---+
            //                           |                               |
            //                           +-------------------------------+
            //                                 |
            //                                 | If the x coordinate is 6, bits 1 and 9 are chosen. Bit 1 would
            //                                 | represent the low bit in the 2 bit index while bit 9 would represent
            //                                 | the high bit.

            var lowPlaneByte = PpuRead((ushort)((ushort)patternTableOffset + tileIndex * 16 + y));
            var highPlaneByte = PpuRead((ushort)((ushort)patternTableOffset + tileIndex * 16 + y + 8));

            // The MSB represents x = 0 while the LSB represents x = 7. Therefore, shift the byte to the left the
            // number of x positions we are interested in, mask off the MSB, then logical shift right 7 to get the bit
            // we are interested into the LSB.
            var lowPlaneBit = (lowPlaneByte << x & 0x80) >> 7;
            var highPlaneBit = (highPlaneByte << x & 0x80) >> 7;

            // Combine the low bit and high bit to create the 2 bit palette entry index.
            return new PaletteEntryIndex(lowPlaneBit, highPlaneBit);
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

        public PatternTile GetPatternTile(PatternTableOffset patternTableOffset, int index)
        {
            if (index < 0 || index > 255) throw new ArgumentOutOfRangeException(nameof(index));
            if (!Enum.IsDefined(typeof(PatternTableOffset), patternTableOffset)) throw new InvalidEnumArgumentException(nameof(patternTableOffset), (int)patternTableOffset, typeof(PatternTableOffset));

            Span<byte> plane1 = stackalloc byte[8];
            Span<byte> plane2 = stackalloc byte[8];

            for (var i = 0; i < 8; i++)
            {
                plane1[i] = PpuRead((ushort)((ushort)patternTableOffset + index * 16 + i));
                plane2[i] = PpuRead((ushort)((ushort)patternTableOffset + index * 16 + 8 + i)); // Add 8 bytes to skip the first plane.
            }

            return new PatternTile(plane1, plane2);
        }

        public bool CpuRead(ushort address, out byte data)
        {
            if (address >= 0x2000 && address <= 0x3fff)
            {
                switch (address & 0x7)
                {
                    case 2: // Register 0x2002
                        data = Registers.ReadStatusRegister();
                        return true;

                    case 4: // Register 0x2004
                        // When the PPU is clearing the secondary OAM, this memory address is
                        // pinned to the value 0xff. This is because the PPU is reading from the
                        // OAM and than writing what it read to the temproary OAM. During the
                        // cleaing process, memory address 0x2004 is pinned to 0xff so that the PPU
                        // will always read 0xff and always write 0xff to the secondary OAM.
                        if (Registers.RenderingEnabled && _scanline >= -1 && _scanline <= 239 && _cycle >= 1 && _cycle <= 64)
                        {
                            data = 0xff;
                        }
                        else
                        {
                            data = Oam.Read(_oamAddress);
                        }

                        return true;

                    case 7: // Register 0x2007
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
                    case 0: // Register 0x2000
                        Registers.WriteControlRegister(data);
                        return true;

                    case 1: // Register 0x2001
                        Registers.WriteMaskRegister(data);
                        return true;

                    case 3: // Register 0x2003
                        _oamAddress = data;
                        return true;

                    case 4: // Register 0x2004
                        Oam.Write(_oamAddress++, data); // Writing increments the OAM address, reading does not.
                        return true;

                    case 5: // Register 0x2005
                        Registers.WriteScroll(data);
                        return true;

                    case 6: // Register 0x2006
                        Registers.WriteAddress(data);
                        return true;

                    case 7: // Register 0x2007
                        PpuWrite(Registers.VAddress, data);

                        // Increment the address by either 1 or 32 depending on the VRAM address increment flag.
                        Registers.VAddress += !Registers.VramAddressIncrement ? (ushort)1 : (ushort)32;

                        return true;
                }
            }

            return false;
        }

        public byte PpuRead(ushort address)
        {
            address &= 0x3fff; // Ensure we never read outside of the PPU bus's address range.

            if (_cartridge is not null && _cartridge.PpuRead(address, out var data))
            {
                return data;
            }

            if (_cartridge is not null && _nameTableRam.PpuRead(_cartridge.GetMirrorMode(), address, out data))
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

            if (_cartridge is not null && _cartridge.PpuWrite(address, data))
            {

            }
            else if (_cartridge is not null && _nameTableRam.PpuWrite(_cartridge.GetMirrorMode(), address, data))
            {

            }
            else if (PaletteRam.PpuWrite(address, data))
            {

            }
        }
    }
}