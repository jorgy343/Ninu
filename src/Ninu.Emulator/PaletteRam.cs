using System;
using System.Diagnostics;

namespace Ninu.Emulator
{
    public class PaletteRam : IPpuBusComponent
    {
        private readonly byte[] _palette = new byte[32];

        public byte GetBackgroundColor()
        {
            return _palette[0];
        }

        public PaletteEntry GetEntry(int index)
        {
            if (index < 0 || index > 7) throw new ArgumentOutOfRangeException(nameof(index));

            var offset = 1 + index * 4;

            return new PaletteEntry(
                _palette[offset + 0],
                _palette[offset + 1],
                _palette[offset + 2],
                _palette[0]); // The fourth byte is always a mirror of the background color.
        }

        public bool PpuRead(ushort address, out byte data)
        {
            if (address >= 0x3f00 && address <= 0x3fff)
            {
                var translatedAddress = (ushort)(address & 0x001f);

                // Handle the mirroring.
                translatedAddress = translatedAddress switch
                {
                    0x0010 => 0x0000,
                    0x0014 => 0x0004,
                    0x0018 => 0x0008,
                    0x001c => 0x000c,
                    _ => translatedAddress,
                };

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
                translatedAddress = translatedAddress switch
                {
                    0x0010 => 0x0000,
                    0x0014 => 0x0004,
                    0x0018 => 0x0008,
                    0x001c => 0x000c,
                    _ => translatedAddress,
                };

                _palette[translatedAddress] = data;
                return true;
            }

            return false;
        }
    }
}