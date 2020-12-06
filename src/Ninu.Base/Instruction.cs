using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Ninu.Base
{
    public sealed class Instruction
    {
        /// <summary>
        /// Gets the three character name of the instruction in lower case.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the opcode of the instruction.
        /// </summary>
        public byte OpCode { get; }

        /// <summary>
        /// Gets the size of the instruction in bytes. This will be either 1, 2, or 3.
        /// </summary>
        public int Size { get; }

        /// <summary>
        /// Gets the number of cycles required to fully execute the instruction. This does not include any potential
        /// penalties.
        /// </summary>
        public int BaseCycles { get; }

        /// <summary>
        /// The addressing mode used when executing the instruction.
        /// </summary>
        public AddressingMode AddressingMode { get; }

        /// <summary>
        /// An array of all 256 possible instructions. The position in the array represents the instructions opcode.
        /// For example, the opcode 0x00 is the BRK instruction. The opcode 0x60 is the BVC instruction. A lot of
        /// opcodes are not officially defined, but are still accounted for and often implemented.
        /// </summary>
        private static readonly Instruction[] _instructions =
        {
            new Instruction("brk", 0x00, 1, 7, AddressingMode.Implied),
            new Instruction("ora", 0x01, 2, 6, AddressingMode.IndirectZeroPageWithXOffset),
            new Instruction("kil", 0x02, 1, 2, AddressingMode.Implied),
            new Instruction("slo", 0x03, 2, 8, AddressingMode.IndirectZeroPageWithXOffset),
            new Instruction("dop", 0x04, 2, 3, AddressingMode.ZeroPage),
            new Instruction("ora", 0x05, 2, 3, AddressingMode.ZeroPage),
            new Instruction("asl", 0x06, 2, 5, AddressingMode.ZeroPage),
            new Instruction("slo", 0x07, 2, 5, AddressingMode.ZeroPage),
            new Instruction("php", 0x08, 1, 3, AddressingMode.Implied),
            new Instruction("ora", 0x09, 2, 2, AddressingMode.Immediate),
            new Instruction("asl", 0x0a, 1, 2, AddressingMode.Accumulator),
            new Instruction("???", 0x0b, 1, 2, AddressingMode.Implied),
            new Instruction("top", 0x0c, 3, 4, AddressingMode.Absolute),
            new Instruction("ora", 0x0d, 3, 4, AddressingMode.Absolute),
            new Instruction("asl", 0x0e, 3, 6, AddressingMode.Absolute),
            new Instruction("slo", 0x0f, 3, 6, AddressingMode.Absolute),
            new Instruction("bpl", 0x10, 2, 2, AddressingMode.Relative),
            new Instruction("ora", 0x11, 2, 5, AddressingMode.IndirectZeroPageWithYOffset),
            new Instruction("kil", 0x12, 1, 2, AddressingMode.Implied),
            new Instruction("slo", 0x13, 2, 8, AddressingMode.IndirectZeroPageWithYOffset),
            new Instruction("dop", 0x14, 2, 4, AddressingMode.ZeroPageWithXOffset),
            new Instruction("ora", 0x15, 2, 4, AddressingMode.ZeroPageWithXOffset),
            new Instruction("asl", 0x16, 2, 6, AddressingMode.ZeroPageWithXOffset),
            new Instruction("slo", 0x17, 2, 6, AddressingMode.ZeroPageWithXOffset),
            new Instruction("clc", 0x18, 1, 2, AddressingMode.Implied),
            new Instruction("ora", 0x19, 3, 4, AddressingMode.AbsoluteWithYOffset),
            new Instruction("nop", 0x1a, 1, 2, AddressingMode.Implied),
            new Instruction("slo", 0x1b, 3, 7, AddressingMode.AbsoluteWithYOffset),
            new Instruction("top", 0x1c, 3, 4, AddressingMode.AbsoluteWithXOffset),
            new Instruction("ora", 0x1d, 3, 4, AddressingMode.AbsoluteWithXOffset),
            new Instruction("asl", 0x1e, 3, 7, AddressingMode.AbsoluteWithXOffset),
            new Instruction("slo", 0x1f, 3, 7, AddressingMode.AbsoluteWithXOffset),
            new Instruction("jsr", 0x20, 3, 6, AddressingMode.Absolute),
            new Instruction("and", 0x21, 2, 6, AddressingMode.IndirectZeroPageWithXOffset),
            new Instruction("kil", 0x22, 1, 2, AddressingMode.Implied),
            new Instruction("rla", 0x23, 2, 8, AddressingMode.IndirectZeroPageWithXOffset),
            new Instruction("bit", 0x24, 2, 3, AddressingMode.ZeroPage),
            new Instruction("and", 0x25, 2, 3, AddressingMode.ZeroPage),
            new Instruction("rol", 0x26, 2, 5, AddressingMode.ZeroPage),
            new Instruction("rla", 0x27, 2, 5, AddressingMode.ZeroPage),
            new Instruction("plp", 0x28, 1, 4, AddressingMode.Implied),
            new Instruction("and", 0x29, 2, 2, AddressingMode.Immediate),
            new Instruction("rol", 0x2a, 1, 2, AddressingMode.Accumulator),
            new Instruction("???", 0x2b, 1, 2, AddressingMode.Implied),
            new Instruction("bit", 0x2c, 3, 4, AddressingMode.Absolute),
            new Instruction("and", 0x2d, 3, 4, AddressingMode.Absolute),
            new Instruction("rol", 0x2e, 3, 6, AddressingMode.Absolute),
            new Instruction("rla", 0x2f, 3, 6, AddressingMode.Absolute),
            new Instruction("bmi", 0x30, 2, 2, AddressingMode.Relative),
            new Instruction("and", 0x31, 2, 5, AddressingMode.IndirectZeroPageWithYOffset),
            new Instruction("kil", 0x32, 1, 2, AddressingMode.Implied),
            new Instruction("rla", 0x33, 2, 8, AddressingMode.IndirectZeroPageWithYOffset),
            new Instruction("dop", 0x34, 2, 4, AddressingMode.ZeroPageWithXOffset),
            new Instruction("and", 0x35, 2, 4, AddressingMode.ZeroPageWithXOffset),
            new Instruction("rol", 0x36, 2, 6, AddressingMode.ZeroPageWithXOffset),
            new Instruction("rla", 0x37, 2, 6, AddressingMode.ZeroPageWithXOffset),
            new Instruction("sec", 0x38, 1, 2, AddressingMode.Implied),
            new Instruction("and", 0x39, 3, 4, AddressingMode.AbsoluteWithYOffset),
            new Instruction("nop", 0x3a, 1, 2, AddressingMode.Implied),
            new Instruction("rla", 0x3b, 3, 7, AddressingMode.AbsoluteWithYOffset),
            new Instruction("top", 0x3c, 3, 4, AddressingMode.AbsoluteWithXOffset),
            new Instruction("and", 0x3d, 3, 4, AddressingMode.AbsoluteWithXOffset),
            new Instruction("rol", 0x3e, 3, 7, AddressingMode.AbsoluteWithXOffset),
            new Instruction("rla", 0x3f, 3, 7, AddressingMode.AbsoluteWithXOffset),
            new Instruction("rti", 0x40, 1, 6, AddressingMode.Implied),
            new Instruction("eor", 0x41, 2, 6, AddressingMode.IndirectZeroPageWithXOffset),
            new Instruction("kil", 0x42, 1, 2, AddressingMode.Implied),
            new Instruction("???", 0x43, 1, 2, AddressingMode.Implied),
            new Instruction("dop", 0x44, 2, 3, AddressingMode.ZeroPage),
            new Instruction("eor", 0x45, 2, 3, AddressingMode.ZeroPage),
            new Instruction("lsr", 0x46, 2, 5, AddressingMode.ZeroPage),
            new Instruction("???", 0x47, 1, 2, AddressingMode.Implied),
            new Instruction("pha", 0x48, 1, 3, AddressingMode.Implied),
            new Instruction("eor", 0x49, 2, 2, AddressingMode.Immediate),
            new Instruction("lsr", 0x4a, 1, 2, AddressingMode.Accumulator),
            new Instruction("???", 0x4b, 1, 2, AddressingMode.Implied),
            new Instruction("jmp", 0x4c, 3, 3, AddressingMode.Absolute),
            new Instruction("eor", 0x4d, 3, 4, AddressingMode.Absolute),
            new Instruction("lsr", 0x4e, 3, 6, AddressingMode.Absolute),
            new Instruction("???", 0x4f, 1, 2, AddressingMode.Implied),
            new Instruction("bvc", 0x50, 2, 2, AddressingMode.Relative),
            new Instruction("eor", 0x51, 2, 5, AddressingMode.IndirectZeroPageWithYOffset),
            new Instruction("kil", 0x52, 1, 2, AddressingMode.Implied),
            new Instruction("???", 0x53, 1, 2, AddressingMode.Implied),
            new Instruction("dop", 0x54, 2, 4, AddressingMode.ZeroPageWithXOffset),
            new Instruction("eor", 0x55, 2, 4, AddressingMode.ZeroPageWithXOffset),
            new Instruction("lsr", 0x56, 2, 6, AddressingMode.ZeroPageWithXOffset),
            new Instruction("???", 0x57, 1, 2, AddressingMode.Implied),
            new Instruction("cli", 0x58, 1, 2, AddressingMode.Implied),
            new Instruction("eor", 0x59, 3, 4, AddressingMode.AbsoluteWithYOffset),
            new Instruction("nop", 0x5a, 1, 2, AddressingMode.Implied),
            new Instruction("???", 0x5b, 1, 2, AddressingMode.Implied),
            new Instruction("top", 0x5c, 3, 4, AddressingMode.AbsoluteWithXOffset),
            new Instruction("eor", 0x5d, 3, 4, AddressingMode.AbsoluteWithXOffset),
            new Instruction("lsr", 0x5e, 3, 7, AddressingMode.AbsoluteWithXOffset),
            new Instruction("???", 0x5f, 1, 2, AddressingMode.Implied),
            new Instruction("rts", 0x60, 1, 6, AddressingMode.Implied),
            new Instruction("adc", 0x61, 2, 6, AddressingMode.IndirectZeroPageWithXOffset),
            new Instruction("kil", 0x62, 1, 2, AddressingMode.Implied),
            new Instruction("rra", 0x63, 2, 8, AddressingMode.IndirectZeroPageWithXOffset),
            new Instruction("dop", 0x64, 2, 3, AddressingMode.ZeroPage),
            new Instruction("adc", 0x65, 2, 3, AddressingMode.ZeroPage),
            new Instruction("ror", 0x66, 2, 5, AddressingMode.ZeroPage),
            new Instruction("rra", 0x67, 2, 5, AddressingMode.ZeroPage),
            new Instruction("pla", 0x68, 1, 4, AddressingMode.Implied),
            new Instruction("adc", 0x69, 2, 2, AddressingMode.Immediate),
            new Instruction("ror", 0x6a, 1, 2, AddressingMode.Accumulator),
            new Instruction("???", 0x6b, 1, 2, AddressingMode.Implied),
            new Instruction("jmp", 0x6c, 3, 5, AddressingMode.Indirect),
            new Instruction("adc", 0x6d, 3, 4, AddressingMode.Absolute),
            new Instruction("ror", 0x6e, 3, 6, AddressingMode.Absolute),
            new Instruction("rra", 0x6f, 3, 6, AddressingMode.Absolute),
            new Instruction("bvs", 0x70, 2, 2, AddressingMode.Relative),
            new Instruction("adc", 0x71, 2, 5, AddressingMode.IndirectZeroPageWithYOffset),
            new Instruction("kil", 0x72, 1, 2, AddressingMode.Implied),
            new Instruction("rra", 0x73, 2, 8, AddressingMode.IndirectZeroPageWithYOffset),
            new Instruction("dop", 0x74, 2, 4, AddressingMode.ZeroPageWithXOffset),
            new Instruction("adc", 0x75, 2, 4, AddressingMode.ZeroPageWithXOffset),
            new Instruction("ror", 0x76, 2, 6, AddressingMode.ZeroPageWithXOffset),
            new Instruction("rra", 0x77, 2, 6, AddressingMode.ZeroPageWithXOffset),
            new Instruction("sei", 0x78, 1, 2, AddressingMode.Implied),
            new Instruction("adc", 0x79, 3, 4, AddressingMode.AbsoluteWithYOffset),
            new Instruction("nop", 0x7a, 1, 2, AddressingMode.Implied),
            new Instruction("rra", 0x7b, 3, 7, AddressingMode.AbsoluteWithYOffset),
            new Instruction("top", 0x7c, 3, 4, AddressingMode.AbsoluteWithXOffset),
            new Instruction("adc", 0x7d, 3, 4, AddressingMode.AbsoluteWithXOffset),
            new Instruction("ror", 0x7e, 3, 7, AddressingMode.AbsoluteWithXOffset),
            new Instruction("rra", 0x7f, 3, 7, AddressingMode.AbsoluteWithXOffset),
            new Instruction("dop", 0x80, 2, 2, AddressingMode.Immediate),
            new Instruction("sta", 0x81, 2, 6, AddressingMode.IndirectZeroPageWithXOffset),
            new Instruction("dop", 0x82, 2, 2, AddressingMode.Immediate),
            new Instruction("aax", 0x83, 2, 6, AddressingMode.IndirectZeroPageWithXOffset),
            new Instruction("sty", 0x84, 2, 3, AddressingMode.ZeroPage),
            new Instruction("sta", 0x85, 2, 3, AddressingMode.ZeroPage),
            new Instruction("stx", 0x86, 2, 3, AddressingMode.ZeroPage),
            new Instruction("aax", 0x87, 2, 3, AddressingMode.ZeroPage),
            new Instruction("dey", 0x88, 1, 2, AddressingMode.Implied),
            new Instruction("dop", 0x89, 2, 2, AddressingMode.Immediate),
            new Instruction("txa", 0x8a, 1, 2, AddressingMode.Implied),
            new Instruction("???", 0x8b, 1, 2, AddressingMode.Implied),
            new Instruction("sty", 0x8c, 3, 4, AddressingMode.Absolute),
            new Instruction("sta", 0x8d, 3, 4, AddressingMode.Absolute),
            new Instruction("stx", 0x8e, 3, 4, AddressingMode.Absolute),
            new Instruction("aax", 0x8f, 3, 4, AddressingMode.Absolute),
            new Instruction("bcc", 0x90, 2, 2, AddressingMode.Relative),
            new Instruction("sta", 0x91, 2, 6, AddressingMode.IndirectZeroPageWithYOffset),
            new Instruction("kil", 0x92, 1, 2, AddressingMode.Implied),
            new Instruction("???", 0x93, 1, 2, AddressingMode.Implied),
            new Instruction("sty", 0x94, 2, 4, AddressingMode.ZeroPageWithXOffset),
            new Instruction("sta", 0x95, 2, 4, AddressingMode.ZeroPageWithXOffset),
            new Instruction("stx", 0x96, 2, 4, AddressingMode.ZeroPageWithYOffset),
            new Instruction("aax", 0x97, 2, 4, AddressingMode.ZeroPageWithYOffset),
            new Instruction("tya", 0x98, 1, 2, AddressingMode.Implied),
            new Instruction("sta", 0x99, 3, 5, AddressingMode.AbsoluteWithYOffset),
            new Instruction("txs", 0x9a, 1, 2, AddressingMode.Implied),
            new Instruction("???", 0x9b, 1, 2, AddressingMode.Implied),
            new Instruction("???", 0x9c, 1, 2, AddressingMode.Implied),
            new Instruction("sta", 0x9d, 3, 5, AddressingMode.AbsoluteWithXOffset),
            new Instruction("???", 0x9e, 1, 2, AddressingMode.Implied),
            new Instruction("???", 0x9f, 1, 2, AddressingMode.Implied),
            new Instruction("ldy", 0xa0, 2, 2, AddressingMode.Immediate),
            new Instruction("lda", 0xa1, 2, 6, AddressingMode.IndirectZeroPageWithXOffset),
            new Instruction("ldx", 0xa2, 2, 2, AddressingMode.Immediate),
            new Instruction("lax", 0xa3, 2, 6, AddressingMode.IndirectZeroPageWithXOffset),
            new Instruction("ldy", 0xa4, 2, 3, AddressingMode.ZeroPage),
            new Instruction("lda", 0xa5, 2, 3, AddressingMode.ZeroPage),
            new Instruction("ldx", 0xa6, 2, 3, AddressingMode.ZeroPage),
            new Instruction("lax", 0xa7, 2, 3, AddressingMode.ZeroPage),
            new Instruction("tay", 0xa8, 1, 2, AddressingMode.Implied),
            new Instruction("lda", 0xa9, 2, 2, AddressingMode.Immediate),
            new Instruction("tax", 0xaa, 1, 2, AddressingMode.Implied),
            new Instruction("???", 0xab, 1, 2, AddressingMode.Implied),
            new Instruction("ldy", 0xac, 3, 4, AddressingMode.Absolute),
            new Instruction("lda", 0xad, 3, 4, AddressingMode.Absolute),
            new Instruction("ldx", 0xae, 3, 4, AddressingMode.Absolute),
            new Instruction("lax", 0xaf, 3, 4, AddressingMode.Absolute),
            new Instruction("bcs", 0xb0, 2, 2, AddressingMode.Relative),
            new Instruction("lda", 0xb1, 2, 5, AddressingMode.IndirectZeroPageWithYOffset),
            new Instruction("kil", 0xb2, 1, 2, AddressingMode.Implied),
            new Instruction("lax", 0xb3, 2, 5, AddressingMode.IndirectZeroPageWithYOffset),
            new Instruction("ldy", 0xb4, 2, 4, AddressingMode.ZeroPageWithXOffset),
            new Instruction("lda", 0xb5, 2, 4, AddressingMode.ZeroPageWithXOffset),
            new Instruction("ldx", 0xb6, 2, 4, AddressingMode.ZeroPageWithYOffset),
            new Instruction("lax", 0xb7, 2, 4, AddressingMode.ZeroPageWithYOffset),
            new Instruction("clv", 0xb8, 1, 2, AddressingMode.Implied),
            new Instruction("lda", 0xb9, 3, 4, AddressingMode.AbsoluteWithYOffset),
            new Instruction("tsx", 0xba, 1, 2, AddressingMode.Implied),
            new Instruction("???", 0xbb, 1, 2, AddressingMode.Implied),
            new Instruction("ldy", 0xbc, 3, 4, AddressingMode.AbsoluteWithXOffset),
            new Instruction("lda", 0xbd, 3, 4, AddressingMode.AbsoluteWithXOffset),
            new Instruction("ldx", 0xbe, 3, 4, AddressingMode.AbsoluteWithYOffset),
            new Instruction("lax", 0xbf, 3, 4, AddressingMode.AbsoluteWithYOffset),
            new Instruction("cpy", 0xc0, 2, 2, AddressingMode.Immediate),
            new Instruction("cmp", 0xc1, 2, 6, AddressingMode.IndirectZeroPageWithXOffset),
            new Instruction("dop", 0xc2, 2, 2, AddressingMode.Immediate),
            new Instruction("???", 0xc3, 1, 2, AddressingMode.Implied),
            new Instruction("cpy", 0xc4, 2, 3, AddressingMode.ZeroPage),
            new Instruction("cmp", 0xc5, 2, 3, AddressingMode.ZeroPage),
            new Instruction("dec", 0xc6, 2, 5, AddressingMode.ZeroPage),
            new Instruction("???", 0xc7, 1, 2, AddressingMode.Implied),
            new Instruction("iny", 0xc8, 1, 2, AddressingMode.Implied),
            new Instruction("cmp", 0xc9, 2, 2, AddressingMode.Immediate),
            new Instruction("dex", 0xca, 1, 2, AddressingMode.Implied),
            new Instruction("axs", 0xcb, 2, 2, AddressingMode.Immediate),
            new Instruction("cpy", 0xcc, 3, 4, AddressingMode.Absolute),
            new Instruction("cmp", 0xcd, 3, 4, AddressingMode.Absolute),
            new Instruction("dec", 0xce, 3, 6, AddressingMode.Absolute),
            new Instruction("???", 0xcf, 1, 2, AddressingMode.Implied),
            new Instruction("bne", 0xd0, 2, 2, AddressingMode.Relative),
            new Instruction("cmp", 0xd1, 2, 5, AddressingMode.IndirectZeroPageWithYOffset),
            new Instruction("kil", 0xd2, 1, 2, AddressingMode.Implied),
            new Instruction("???", 0xd3, 1, 2, AddressingMode.Implied),
            new Instruction("dop", 0xd4, 2, 4, AddressingMode.ZeroPageWithXOffset),
            new Instruction("cmp", 0xd5, 2, 4, AddressingMode.ZeroPageWithXOffset),
            new Instruction("dec", 0xd6, 2, 6, AddressingMode.ZeroPageWithXOffset),
            new Instruction("???", 0xd7, 1, 2, AddressingMode.Implied),
            new Instruction("cld", 0xd8, 1, 2, AddressingMode.Implied),
            new Instruction("cmp", 0xd9, 3, 4, AddressingMode.AbsoluteWithYOffset),
            new Instruction("nop", 0xda, 1, 2, AddressingMode.Implied),
            new Instruction("???", 0xdb, 1, 2, AddressingMode.Implied),
            new Instruction("top", 0xdc, 3, 4, AddressingMode.AbsoluteWithXOffset),
            new Instruction("cmp", 0xdd, 3, 4, AddressingMode.AbsoluteWithXOffset),
            new Instruction("dec", 0xde, 3, 7, AddressingMode.AbsoluteWithXOffset),
            new Instruction("???", 0xdf, 1, 2, AddressingMode.Implied),
            new Instruction("cpx", 0xe0, 2, 2, AddressingMode.Immediate),
            new Instruction("sbc", 0xe1, 2, 6, AddressingMode.IndirectZeroPageWithXOffset),
            new Instruction("dop", 0xe2, 2, 2, AddressingMode.Immediate),
            new Instruction("???", 0xe3, 1, 2, AddressingMode.Implied),
            new Instruction("cpx", 0xe4, 2, 3, AddressingMode.ZeroPage),
            new Instruction("sbc", 0xe5, 2, 3, AddressingMode.ZeroPage),
            new Instruction("inc", 0xe6, 2, 5, AddressingMode.ZeroPage),
            new Instruction("???", 0xe7, 1, 2, AddressingMode.Implied),
            new Instruction("inx", 0xe8, 1, 2, AddressingMode.Implied),
            new Instruction("sbc", 0xe9, 2, 2, AddressingMode.Immediate),
            new Instruction("nop", 0xea, 1, 2, AddressingMode.Implied),
            new Instruction("sbc", 0xeb, 2, 2, AddressingMode.Immediate),
            new Instruction("cpx", 0xec, 3, 4, AddressingMode.Absolute),
            new Instruction("sbc", 0xed, 3, 4, AddressingMode.Absolute),
            new Instruction("inc", 0xee, 3, 6, AddressingMode.Absolute),
            new Instruction("???", 0xef, 1, 2, AddressingMode.Implied),
            new Instruction("beq", 0xf0, 2, 2, AddressingMode.Relative),
            new Instruction("sbc", 0xf1, 2, 5, AddressingMode.IndirectZeroPageWithYOffset),
            new Instruction("kil", 0xf2, 1, 2, AddressingMode.Implied),
            new Instruction("???", 0xf3, 1, 2, AddressingMode.Implied),
            new Instruction("dop", 0xf4, 2, 4, AddressingMode.ZeroPageWithXOffset),
            new Instruction("sbc", 0xf5, 2, 4, AddressingMode.ZeroPageWithXOffset),
            new Instruction("inc", 0xf6, 2, 6, AddressingMode.ZeroPageWithXOffset),
            new Instruction("???", 0xf7, 1, 2, AddressingMode.Implied),
            new Instruction("sed", 0xf8, 1, 2, AddressingMode.Implied),
            new Instruction("sbc", 0xf9, 3, 4, AddressingMode.AbsoluteWithYOffset),
            new Instruction("nop", 0xfa, 1, 2, AddressingMode.Implied),
            new Instruction("???", 0xfb, 1, 2, AddressingMode.Implied),
            new Instruction("top", 0xfc, 3, 4, AddressingMode.AbsoluteWithXOffset),
            new Instruction("sbc", 0xfd, 3, 4, AddressingMode.AbsoluteWithXOffset),
            new Instruction("inc", 0xfe, 3, 7, AddressingMode.AbsoluteWithXOffset),
            new Instruction("???", 0xff, 1, 2, AddressingMode.Implied),
        };

        private static readonly Dictionary<(string Name, AddressingMode AddressingMode), Instruction> _instructionsByNameAndAddressingMode = new(256);

        static Instruction()
        {
            foreach (var instruction in _instructions)
            {
                _instructionsByNameAndAddressingMode[(instruction.Name, instruction.AddressingMode)] = instruction;
            }
        }

        private Instruction(string name, byte opCode, int size, int baseCycles, AddressingMode addressingMode)
        {
            if (!Enum.IsDefined(typeof(AddressingMode), addressingMode)) throw new InvalidEnumArgumentException(nameof(addressingMode), (int)addressingMode, typeof(AddressingMode));
            if (size <= 0) throw new ArgumentOutOfRangeException(nameof(size));
            if (baseCycles <= 0) throw new ArgumentOutOfRangeException(nameof(baseCycles));

            Name = name ?? throw new ArgumentNullException(nameof(name));
            OpCode = opCode;
            Size = size;
            BaseCycles = baseCycles;
            AddressingMode = addressingMode;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{Name}: {OpCode}";
        }

        public static Instruction GetByOpCode(byte opCode) => _instructions[opCode];

        public static Instruction? GetByNameAndAddressingMode(string instructionName, AddressingMode addressingMode)
        {
            if (instructionName is null) throw new ArgumentNullException(nameof(instructionName));
            if (!Enum.IsDefined(typeof(AddressingMode), addressingMode)) throw new InvalidEnumArgumentException(nameof(addressingMode), (int)addressingMode, typeof(AddressingMode));

            return _instructionsByNameAndAddressingMode.TryGetValue((instructionName.ToLowerInvariant(), addressingMode), out var instruction) ? instruction : null;
        }
    }
}