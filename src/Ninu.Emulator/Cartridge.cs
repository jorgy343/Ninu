// ReSharper disable ConditionIsAlwaysTrueOrFalse

using Microsoft.Extensions.Logging;
using Ninu.Emulator.Mappers;
using System;

namespace Ninu.Emulator
{
    public class Cartridge : ICpuBusComponent, IPpuBusComponent
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger _logger;

        public NesImage Image { get; }

        [SaveChildren]
        public Mapper Mapper { get; }

        [Save]
        public byte[] Ram { get; } = new byte[8192];

        [Save]
        public byte[]? CharacterRam { get; }

        public Cartridge(NesImage image, ILoggerFactory loggerFactory, ILogger logger)
        {
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            Image = image ?? throw new ArgumentNullException(nameof(image));

            if (Image.PatternRamInsteadOfRom)
            {
                CharacterRam = new byte[8192];
            }

            Mapper = Image.MapperType switch
            {
                000 => new Mapper000(Image.ProgramRomBankCount, Image.PatternRomBankCount, _loggerFactory.CreateLogger<Mapper000>()),
                001 => new Mapper001(Image.ProgramRomBankCount, Image.PatternRomBankCount, _loggerFactory.CreateLogger<Mapper>()),
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
            if (address >= 0x0000 && address <= 0x1fff)
            {
                if (CharacterRam != null)
                {
                    data = CharacterRam[address];
                    return true;
                }
            }

            var translated = Mapper.TranslatePatternRomAddress(address, out var translatedAddress);

            if (translated)
            {
                if (Image.PatternRom.Length == 0)
                {
                    data = 0;
                    return true;
                }

                data = Image.PatternRom[translatedAddress];
                return true;
            }

            data = 0;
            return false;
        }

        public bool PpuWrite(ushort address, byte data)
        {
            if (address >= 0x0000 && address <= 0x1fff)
            {
                if (CharacterRam != null)
                {
                    CharacterRam[address] = data;
                    return true;
                }
            }

            return false;
        }
    }
}