using System;

namespace Ninu.Emulator
{
    /// <summary>
    /// Represents one tile of the background where each pixel of the 8x8 is represented
    /// by the palette color. The palette color then only has to be mapped to the system
    /// palette.
    /// </summary>
    public class BackgroundSprite
    {
        public byte[] Colors { get; } = new byte[8 * 8];

        public BackgroundSprite(byte[] paletteColors)
        {
            Colors = paletteColors;
        }

        public BackgroundSprite(PatternTile patternTile, PaletteEntry palette)
        {
            for (var y = 0; y < 8; y++)
            {
                for (var x = 0; x < 8; x++)
                {
                    var paletteEntryIndex = patternTile.GetPaletteColorIndex(x, y);

                    var paletteColor = paletteEntryIndex switch
                    {
                        PaletteColor.Color0 => palette.Byte1,
                        PaletteColor.Color1 => palette.Byte2,
                        PaletteColor.Color2 => palette.Byte3,
                        PaletteColor.Color3 => palette.Byte4,
                        _ => throw new ArgumentOutOfRangeException()
                    };

                    Colors[y * 8 + x] = paletteColor;
                }
            }
        }
    }
}