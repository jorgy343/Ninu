namespace Ninu.Emulator
{
    // Palette RAM exists in the PPU address space at 0x3f00-0x3f1f which is 32 bytes in length. This is mirrored in
    // the address range 0x2f20-0x2fff seven times.
    //
    // Each palette is 4 bytes large which allows for 8 palettes in total. The first four palettes are only used for
    // backgrounds while the last four palettes are only used for sprites. Each palette specifies four colors where
    // each byte of the palette represents one color. Only the low 7 bits of each byte are used which allows for the
    // representation of 64 colors.
    //
    // The first byte of the first palette (0x3f00) is the universal background color. This color will represent the
    // transparent color for all palettes. During rendering, the first byte in every palette (bytes 0x3f00, 0x3f04,
    // 0x3f08, etc.) is always read from 0x3f00. Data can still be written to these bytes, but the rendering pipeline
    // will always use the value found in 0x3f00.
    //
    //        0x3f00   0x3f01   0x3f02   0x3f03   0x3f04   0x3f05   0x3f06   0x3f07   0x3f08   0x3f09   0x3f0a   0x3f0b
    //      +--------+--------+--------+--------+--------+--------+--------+--------+--------+--------+--------+--------+
    //      | Color0 | Color1 | Color2 | Color3 | Color0 | Color1 | Color2 | Color3 | Color0 | Color1 | Color2 | Color3 |
    //      +--------+--------+--------+--------+--------+--------+--------+--------+--------+--------+--------+--------+
    //      |    |        Palette 0             |    |        Palette 1             |    |        Palette 2             |
    //      +----|------------------------------+----|------------------------------+----|------------------------------+
    //           |                                   |                                   |
    //           +-----------------------------------+-----------------------------------+
    //           |
    //  Universal Background
    //         Color
    //
    // The memory addresses at 0x3f10, 0x3f14, 0x3f18, and 0x3f1c are actually mirrored to their lower counterparts.
    //
    // 0x3f10 => 0x3f00
    // 0x3f14 => 0x3f04
    // 0x3f18 => 0x3f08
    // 0x3f1c => 0x3f0c

    public class PaletteRam : IPpuBusComponent
    {
        [Save("Palette")]
        private readonly byte[] _palette = new byte[32];

        public bool PpuRead(ushort address, out byte data)
        {
            if (address >= 0x3f00 && address <= 0x3fff)
            {
                var translatedAddress = (ushort)(address & 0x001f);

                // Handle the mirroring.
                translatedAddress = (ushort)(translatedAddress switch
                {
                    0x0010 => 0x0000,
                    0x0014 => 0x0004,
                    0x0018 => 0x0008,
                    0x001c => 0x000c,
                    _ => translatedAddress,
                });

                data = _palette[translatedAddress];
                return true;
            }

            data = 0;
            return false;
        }

        public bool PpuWrite(ushort address, byte data)
        {
            if (address >= 0x3f00 && address <= 0x3fff)
            {
                var translatedAddress = (ushort)(address & 0x001f);

                // Handle the mirroring.
                translatedAddress = (ushort)(translatedAddress switch
                {
                    0x0010 => 0x0000,
                    0x0014 => 0x0004,
                    0x0018 => 0x0008,
                    0x001c => 0x000c,
                    _ => translatedAddress,
                });

                _palette[translatedAddress] = data;
                return true;
            }

            return false;
        }
    }
}