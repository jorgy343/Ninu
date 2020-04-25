// ReSharper disable ConditionIsAlwaysTrueOrFalse
using Microsoft.Extensions.Logging;

namespace Ninu.Emulator.Mappers
{
    // Mapper 001 contains 6 registers that can be written to. The only register directly accessible by the program is
    // the load register. The default state of the Control Register is 0x1C.
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
    // Character ROM Index Select 0 Register (CRIS0)
    // 7       0
    // xxxC CCCC
    //    │ ││││
    //    └─┴┴┴┴ Selects the bank of character ROM to use for PPU address range 0x0000-0x0fff in 4KiB mode or 0x0000-0x1fff in 8KiB mode.
    //
    // When the Control Register's C flag is set to 1 (4KiB mode), this register selects the bank of character ROM that
    // will be mapped to the PPU address range 0x0000-0x0fff. When the Control Register's C flag is set to 0 (8KiB)
    // mode, this register selects the 8KiB character ROM bank to use for the entire PPU character ROM address space
    // which is 0x0000-0x1fff.
    //
    // In 8KiB mode, the LSB of CRIS0 is unused. To get the actual bank index, shift the value of this register to the
    // right once.
    //
    //
    // Character ROM Index Select 1 Register (CRIS1)
    // 7       0
    // xxxC CCCC
    //    │ ││││
    //    └─┴┴┴┴ Selects the bank of character ROM to use for PPU address range 0x1000-0x1fff in 4KiB mode.
    //
    // When the Control Register's C flag is set to 1 (4KiB mode), this register selects the bank of character ROM that
    // will be mapped to the PPU address range 0x1000-0x1fff. When the Control Register's C flag is set to 0 (8KiB)
    // mode, this register is unused since CRIS0 will map the entire range of 0x0000-0x1fff.
    //
    // When in 4KiB mode, the address within the range 0x1000-0x1fff needs to be translated such that it indexes a byte
    // within the bank. So if CRIS1 is set to 0x2 and the address to translate is 0x1007, you would return the 8th byte
    // within the 3rd bank of character ROM. Effectively you can just subtract 0x1000 from the address and use that to
    // read the byte within the selected bank.
    //
    //
    // Program ROM Index Select Register (PRIS)
    // 7       0
    // xxxR PPPP
    //    │ ││││
    //    │ └┴┴┴ Selects the bank of program ROM to use for all or some of the CPU program ROM address range of 0x8000-0xffff. See below for more details.
    //    └───── When 1, program RAM chip exists and is enabled; when 0, program RAM chip either doesn't exist or is disabled.
    //
    // When the Control Register's P flag is set to 0 (32KiB) mode, this register selects the 32KiB bank of program ROM
    // that will be mapped to CPU address space 0x8000-0xffff. In this mode, the LSB of PRIS is unused. To get the
    // actual bank index, shift the value of this register to the right once.
    //
    // When in 16KiB mode, this register selects the 8KiB bank of program ROM that will be mapped to either CPU address
    // space 0x8000-0xbfff or 0xc000-0xffff. Which address space is dynamically mapped depends on the Control
    // Register's G flag.
    //
    // When in 16KiB mode and the G flag is 0, the low address space at 0x8000-0xbfff is fixed meaning that it always
    // maps to the first 16KiB program ROM bank. The high address space at 0xc000-0xffff is dynamically mapped based on
    // the value of PRIS. The high address space needs to be translated such that it indexes a byte within the bank. So
    // if PRIS is set to 0x5 and the address to translate is 0x8008, you would return the 9th byte within the 6th bank
    // of program ROM. Effectively you can just subtract 0x8000 from the address and use that to read the byte within
    // the selected bank.
    //
    // When in 16KiB mode and the G flag is 1, the low address space at 0x8000-0xbfff is dynamically mapped based on
    // the value of PRIS while the high address space at 0xc000-0xffff is fixed to the last bank of program ROM.
    // Everything works as it otherwise does when the G flag is 0 except that the low address space is dynamically
    // mapped and the high address space is fixed.
    //
    // The R flag is currently ignored in this implementation. 8KiB of RAM is assumed to exist at address 0x6000.

    public class Mapper001 : Mapper
    {
        [Save("LoadRegister")]
        private byte _loadRegister;

        [Save("LoadRegisterCount")]
        private int _loadRegisterCount;

        [Save("ControlRegister")]
        private byte _controlRegister = 0x1c;

        [Save("PatternRomBank0")]
        private byte _patternRomBank0;

        [Save("PatternRomBank1")]
        private byte _patternRomBank1;

        [Save("ProgramRomBank")]
        private byte _programRomBank;

        public Mapper001(int programRomBankCount, int patternRomBankCount, ILogger logger)
            : base(programRomBankCount, patternRomBankCount, logger)
        {

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

                    Logger.LogTrace("Load register completed. Loaded {Value} into register {Register}.", _loadRegister, registerNumber);

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
                            // Second bank is fixed so just translate the address one to one to the last bank.
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