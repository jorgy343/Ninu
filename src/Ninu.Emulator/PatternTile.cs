using System;

namespace Ninu.Emulator
{
    // The basic pattern ram is 8KiB on the cartridge. This is typically split into two sections: the left
    // and the right. Each tile is 8 pixels by 8 pixels. Each section contains 256 tiles in a 16 by 16 grid.

    public readonly struct PatternTile
    {
        private readonly byte[] _paletteEntryIndices;

        public PatternTile(ReadOnlySpan<byte> plane1, ReadOnlySpan<byte> plane2)
        {
            if (plane1.Length != 8) throw new ArgumentException($"The argument for parameter {nameof(plane1)} must be 8 bytes in length.");
            if (plane2.Length != 8) throw new ArgumentException($"The argument for parameter {nameof(plane2)} must be 8 bytes in length.");

            _paletteEntryIndices = new byte[64];

            for (var i = 0; i < 8; i++) // Represents the byte we are working with (or the y-coordinate).
            {
                for (var j = 0; j < 8; j++) // Represents the bit within the byte we are working with (or the x-coordinate).
                {
                    var bitLow = (plane1[i] >> j) & 0x01;
                    var bitHigh = (plane2[i] >> j) & 0x01;

                    _paletteEntryIndices[i * 8 + (7 - j)] = (byte)(bitLow | (bitHigh << 1));
                }
            }
        }

        public byte GetPaletteColorIndex(int x, int y)
        {
            if (x < 0 || x > 7) throw new ArgumentOutOfRangeException(nameof(x));
            if (y < 0 || y > 7) throw new ArgumentOutOfRangeException(nameof(y));

            return _paletteEntryIndices[y * 8 + x];
        }
    }
}