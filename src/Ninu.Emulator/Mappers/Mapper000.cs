// ReSharper disable ConditionIsAlwaysTrueOrFalse
using Microsoft.Extensions.Logging;

namespace Ninu.Emulator.Mappers
{
    public class Mapper000 : Mapper
    {
        public Mapper000(int programRomBankCount, int patternRomBankCount, ILogger logger)
            : base(programRomBankCount, patternRomBankCount, logger)
        {

        }

        public override bool HandleWrite(ushort address, byte data)
        {
            return false;
        }

        public override bool TranslateProgramRomAddress(ushort address, out ushort translatedAddress)
        {
            if (address >= 0x8000 && address <= 0xffff)
            {
                if (ProgramRomBankCount == 1)
                {
                    // Handle the mirror of the first bank to the second bank.
                    translatedAddress = (ushort)(address & 0x3fff);
                }
                else
                {
                    // Assume there are just two banks.
                    translatedAddress = (ushort)(address & 0x7fff);
                }

                return true;
            }

            translatedAddress = 0;
            return false;
        }

        public override bool TranslatePatternRomAddress(ushort address, out ushort translatedAddress)
        {
            if (address >= 0x0000 && address <= 0x1fff)
            {
                translatedAddress = address;
                return true;
            }

            translatedAddress = 0;
            return false;
        }
    }
}