// ReSharper disable ConditionIsAlwaysTrueOrFalse

namespace Ninu.Emulator.Mappers
{
    // Mapper 001 contains 6 registers that can be written to. The only register directly accessible by the program is
    // the load register.
    //
    //
    // Load Register (8-bit)
    // 7       0
    // Rxxx xxxD
    // │       │
    // │       └ Data in bit - Puts a bit the shift register. See above for more information.
    // └──────── Reset bit - When 1 causes the shift register to clear and the control register is OR'd with 0x0c.
    //
    // The Load Register is accessed by writing to any address within 0x8000-0xffff. This is the entire program ROM
    // addressable space. Only the LSB and MSB of the load register are used. The MSB causes the shift register to
    // clear its state and performs an OR operation on the control register (control register |= 0x0c).
    //
    // When this register is written to with R cleared, the value in D is pushed into the shift register. The values
    // are pushed with LSB first. So if you want the value of 0b11001 in the shift register you would write the
    // following values into the Load Register in the following order: 0x01, 0x00, 0x00, 0x01, 0x01.
    //
    // When the fifth bit is written to the Load Register (without clearing in between), the 5 bits in the shift
    // register are written to one of the other registers. Which register it is written to depends on the address of
    // the write being performed by the CPU. Note that this is the only instance in which the address actually matters.
    // These are the address ranges and the register they correlate to.
    //
    // 0x8000-0x9fff => Control Register
    // 0xa000-0xbfff => Character ROM Index Select 0 Register (CRIS0)
    // 0xc000-0xdfff => Character ROM Index Select 1 Register (CRIS1)
    // 0xe000-0xffff => Program ROM Index Select Register (PRIS)
    //
    // When determining which register to write the shift register to, you only have to examine bits 13-14 of the
    // address.
    //
    //
    // Control Register (5-bit)
    // 7       0
    // xxxC PGMM
    //    │ ││││
    //    │ ││└┴ Mirror Mode - Determines the mirror mode the game is currently using.
    //    │ │└── Program ROM Swap Bank Select - Determines which bank of the program ROM is swappable.
    //    │ └─── Program ROM Bank Size - Determines the size of the program ROM banks (16KiB or 32KiB).
    //    └───── Character ROM Bank Size - Determines the size of the character ROM banks (4KiB or 8KiB).
    //
    // MM (Mirror Mode)
    // 0b00 - One screen using name table 0.
    // 0b01 - One screen using name table 1.
    // 0b10 - Vertical mirroring.
    // 0b11 - Horizontal mirroring.
    //
    // G (Program ROM Swap Bank Select)
    // 0 - Low bank is fixed; top bank is swappable.
    // 1 - Low bank is swappable; top bank is fixed. This is the default mode during boot.
    //
    // P (Program ROM Bank Size)
    // 0 - The banks are 32KiB in size and span the entire program ROM addressable range. When this mode is selected, G (Program ROM Swap Bank Select) is ignored.
    // 1 - The banks are 16KiB in size. This is the default mode during boot.
    //
    // C (Character ROM Bank Size)
    // 0 - The banks are 8KiB in size and span the entire name table addressable range. Both banks in memory are selected from a single bank on the cartridge.
    // 1 - The banks are 4KiB in size. Each bank in memory can be individually selected from banks on the cartridge.
    //
    //
    // To be continued...

    public class Mapper001 : Mapper
    {
        public int ProgramRomBankCount { get; }
        public int PatternRomBankCount { get; }

        private byte _loadRegister;
        private int _loadRegisterCount;

        private byte _controlRegister = 0x1c;
        private byte _patternRomBank0;
        private byte _patternRomBank1;
        private byte _programRomBank;

        public Mapper001(int programRomBankCount, int patternRomBankCount)
        {
            ProgramRomBankCount = programRomBankCount;
            PatternRomBankCount = patternRomBankCount;
        }

        public override bool GetMirrorMode(out NameTableMirrorMode mirrorMode)
        {
            switch (_controlRegister & 0x3)
            {
                case 0b10:
                    mirrorMode = NameTableMirrorMode.Vertical;
                    return true;

                case 0b11:
                    mirrorMode = NameTableMirrorMode.Horizontal;
                    return true;

                default: // TODO: Handle mirror modes 0b00 and 0b01.
                    mirrorMode = NameTableMirrorMode.Vertical;
                    return true;
            }
        }

        public override bool HandleWrite(ushort address, byte data)
        {
            if (address >= 0x8000 && address <= 0xffff)
            {
                if ((data & 0x80) != 0) // Bit 7 set means to perform a reset.
                {
                    _loadRegister = 0;
                    _loadRegisterCount = 0;

                    _controlRegister |= 0x0c; // See https://wiki.nesdev.com/w/index.php/MMC1
                }
                else
                {
                    // When shifting a bit into the load register, the LSB is shifted first. We will set bit 5 of the
                    // load register and then shift the load register to the right once. When the fifth bit is written,
                    // all of the bits will be shifted into the correct positions.
                    _loadRegister |= (byte)((data & 0x01) << 5);
                    _loadRegister >>= 1;

                    _loadRegisterCount++;
                }

                // After the fifth bit is written to the load register, write to the appropriate register.
                if (_loadRegisterCount == 5)
                {
                    var registerNumber = (address & 0x6000) >> 13; // Grab bits 13-14. This will tell us which register to load.

                    switch (registerNumber)
                    {
                        case 0: // Control: 0x8000-0x9fff
                            _controlRegister = _loadRegister;
                            break;

                        case 1: // Program ROM Bank 0: 0xa000-0xbfff
                            _patternRomBank0 = _loadRegister;
                            break;

                        case 2: // Program ROM Bank 1: 0xc000-0xdfff
                            _patternRomBank1 = _loadRegister;
                            break;

                        case 3: // Pattern ROM: 0xe000-0xffff
                            _programRomBank = _loadRegister;
                            break;
                    }

                    _loadRegister = 0;
                    _loadRegisterCount = 0;
                }

                return true;
            }

            return false;
        }

        public override bool TranslateProgramRomAddress(ushort address, out ushort translatedAddress)
        {
            if (address >= 0x8000 && address <= 0xffff)
            {
                if (CurrentProgramRomBankSize == ProgramRomBankSize.Swappable32KBanks)
                {
                    // The first four bits are used to select the program ROM bank. When in 32K bank mode, only bits
                    // 1-3 are used. The LSB is ignored.
                    var index = (_programRomBank & 0x0e) >> 1;

                    translatedAddress = (ushort)((address & 0x7ffff) + 32768 * index);
                }
                else // 16K swappable banks.
                {
                    if (CurrentProgramRomSwapBank == ProgramRomSwapBank.FirstBankFixed)
                    {
                        if (address >= 0x8000 && address <= 0xbfff)
                        {
                            // First bank is fixed so just translate the address one to one.
                            translatedAddress = (ushort)(address & 0x3fff);
                        }
                        else
                        {
                            var index = _programRomBank & 0x0f;

                            translatedAddress = (ushort)((address & 0x3fff) + 16384 * index);
                        }
                    }
                    else
                    {
                        if (address >= 0x8000 && address <= 0xbfff)
                        {
                            var index = _programRomBank & 0x0f;

                            translatedAddress = (ushort)((address & 0x3fff) + 16384 * index);
                        }
                        else
                        {
                            // Second bank is fixed so just translate the address one to one.
                            translatedAddress = (ushort)((address & 0x3fff) + 16384 * (ProgramRomBankCount - 1));
                        }
                    }
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
                if (CurrentPatternRomBankSize == PatternRomBankSize.Single8KBanks)
                {
                    // The first five bits are used to select the pattern ROM bank. When in 8K bank mode, only bits 1-4
                    // are used. The LSB is ignored.
                    var index = (_patternRomBank0 & 0x1e) >> 1;

                    translatedAddress = (ushort)((address & 0x1fff) + 8192 * index);
                }
                else // 4K swappable banks.
                {
                    if (address >= 0x0000 && address <= 0x0fff)
                    {
                        var index = _patternRomBank0 & 0x1f;

                        translatedAddress = (ushort)((address & 0x0fff) + 4096 * index);
                    }
                    else
                    {
                        var index = _patternRomBank1 & 0x1f;

                        translatedAddress = (ushort)((address & 0x0fff) + 4096 * index);
                    }
                }

                return true;
            }

            translatedAddress = 0;
            return false;
        }

        private ProgramRomSwapBank CurrentProgramRomSwapBank => (ProgramRomSwapBank)Bits.GetBits(_controlRegister, 2, 1);
        private ProgramRomBankSize CurrentProgramRomBankSize => (ProgramRomBankSize)Bits.GetBits(_controlRegister, 3, 1);
        private PatternRomBankSize CurrentPatternRomBankSize => (PatternRomBankSize)Bits.GetBits(_controlRegister, 4, 1);

        private enum ProgramRomSwapBank
        {
            /// <summary>
            /// The first bank at 0x8000-0xbfff is fixed and the second bank at 0xc000-0xffff is swappable.
            /// </summary>
            FirstBankFixed = 0,

            /// <summary>
            /// The first bank at 0x8000-0xbfff is swappable and the second bank at 0xc000-0xffff is fixed.
            /// </summary>
            SecondBankFixed = 1,
        }

        private enum ProgramRomBankSize
        {
            Swappable32KBanks = 0,
            Swappable16KBanks = 1,
        }

        private enum PatternRomBankSize
        {
            Single8KBanks = 0,
            Two4KBanks = 1,
        }
    }
}