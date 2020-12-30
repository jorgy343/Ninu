using Microsoft.Extensions.Logging;

namespace Ninu.Emulator.Mappers
{
    public class Mapper002 : Mapper
    {
        [Save("BankSelect")]
        private byte _bankSelect;

        public Mapper002(int programRomBankCount, int patternRomBankCount, ILogger logger)
            : base(programRomBankCount, patternRomBankCount, logger)
        {

        }

        public override bool HandleWrite(ushort address, byte data)
        {
            if (address >= 0x8000 && address <= 0xffff)
            {
                _bankSelect = data;
                return true;
            }

            return false;
        }

        public override bool TranslateProgramRomAddress(ushort address, out int translatedAddress)
        {
            if (address >= 0x8000 && address <= 0xbfff)
            {
                // First program ROM bank is switchable.
                translatedAddress = (address & 0x3fff) + 16384 * _bankSelect;

                return true;
            }
            else if (address >= 0xc000 && address <= 0xffff)
            {
                // Second program ROM bank is fixed to the last bank.
                translatedAddress = (address & 0x3fff) + 16384 * (ProgramRomBankCount - 1);

                return true;
            }

            translatedAddress = 0;
            return false;
        }

        public override bool TranslatePatternRomAddress(ushort address, out int translatedAddress)
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