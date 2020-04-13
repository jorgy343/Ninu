// ReSharper disable ConditionIsAlwaysTrueOrFalse

namespace Ninu.Emulator.Mappers
{
    public class Mapper000 : Mapper
    {
        public int ProgramRomBankCount { get; }
        public int PatternRomBankCount { get; }

        public Mapper000(int programRomBankCount, int patternRomBankCount)
        {
            ProgramRomBankCount = programRomBankCount;
            PatternRomBankCount = patternRomBankCount;
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