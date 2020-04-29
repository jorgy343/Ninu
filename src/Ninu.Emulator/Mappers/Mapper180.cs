// ReSharper disable ConditionIsAlwaysTrueOrFalse
using Microsoft.Extensions.Logging;

namespace Ninu.Emulator.Mappers
{
    public class Mapper180 : Mapper
    {
        [Save("BankSelect")]
        private byte _bankSelect;

        public Mapper180(int programRomBankCount, int patternRomBankCount, ILogger logger)
            : base(programRomBankCount, patternRomBankCount, logger)
        {

        }

        public override bool HandleWrite(ushort address, byte data)
        {
            if (address >= 0x8000 && address <= 0xffff)
            {
                _bankSelect = (byte)(data & 0x07);
                return true;
            }

            return false;
        }

        public override bool TranslateProgramRomAddress(ushort address, out int translatedAddress)
        {
            if (address >= 0x8000 && address <= 0xbfff)
            {
                // First bank is fixed so just translate the address one to one.
                translatedAddress = address & 0x3fff;

                return true;
            }
            else if (address >= 0xc000 && address <= 0xffff)
            {
                // Second bank is selectable.
                translatedAddress = (address & 0x3fff) + 16384 * _bankSelect;

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