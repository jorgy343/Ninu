using Ninu.Emulator.Mappers;
using System;

namespace Ninu.Emulator
{
    public class Cartridge : ICpuBusComponent, IPpuBusComponent
    {
        public NesImage Image { get; }
        public Mapper Mapper { get; }

        public byte[] Ram { get; } = new byte[8192];

        public Cartridge(NesImage image)
        {
            Image = image ?? throw new ArgumentNullException(nameof(image));

            Mapper = Image.MapperType switch
            {
                000 => new Mapper000(Image.ProgramRomBankCount, Image.PatternRomBankCount),
                001 => new Mapper001(Image.ProgramRomBankCount, Image.PatternRomBankCount),
                _ => throw new Exception(), // TODO: Throw a better exception.
            };
        }

        public NameTableMirrorMode GetMirrorMode()
        {
            return Mapper.GetMirrorMode(out var mirrorMode) ? mirrorMode : Image.MirrorMode;
        }

        public bool CpuRead(ushort address, out byte data)
        {
            // The translated address will start at 0 so that we can easily index the ROM.

            var translated = Mapper.TranslateProgramRomAddress(address, out var translatedAddress);

            if (translated)
            {
                data = Image.ProgramRom[translatedAddress];
                return true;
            }

            if (address >= 0x6000 && address <= 0x7fff)
            {
                data = Ram[address - 0x6000];
                return true;
            }

            data = 0;
            return false;
        }

        public bool CpuWrite(ushort address, byte data)
        {
            if (Mapper.HandleWrite(address, data))
            {
                return true;
            }

            if (address >= 0x6000 && address <= 0x7fff)
            {
                Ram[address - 0x6000] = data;
                return true;
            }

            return false;
        }

        public bool PpuRead(ushort address, out byte data)
        {
            var translated = Mapper.TranslatePatternRomAddress(address, out var translatedAddress);

            if (translated)
            {
                data = Image.PatternRom[translatedAddress];
                return true;
            }

            data = 0;
            return false;
        }

        public bool PpuWrite(ushort address, byte data)
        {
            return false;
        }
    }
}