using System;
using System.ComponentModel;
using static Ninu.Emulator.InstructionOperations;

namespace Ninu.Emulator
{
    public delegate int InstructionExecutor(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState);

    public delegate int InstructionExecutorEx(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState, byte? dataIn, out byte result);

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
        /// Gets the number of cycles required to fully execute the instruction. This does not
        /// include any potential penalties.
        /// </summary>
        public int BaseCycles { get; }

        /// <summary>
        /// The addressing mode used when executing the instruction.
        /// </summary>
        public AddressingMode AddressingMode { get; }

        /// <summary>
        /// The function that performs the instructions operation upon execution.
        /// </summary>
        private readonly InstructionExecutor? _instructionExecutor;

        /// <summary>
        /// The function that performs the instructions operation upon execution.
        /// </summary>
        private readonly InstructionExecutorEx? _instructionExecutorEx;

        /// <summary>
        /// An array of all 256 possible instructions. The position in the array represents
        /// the instructions opcode. For example, the opcode 0x00 is the BRK instruction. The
        /// opcode 0x60 is the BVC instruction. A lot of opcodes are not officially defined,
        /// but are still accounted for and often implemented.
        /// </summary>
        private static readonly Instruction[] Instructions =
        {
            new Instruction("brk", 0x00, 1, 7, AddressingMode.Implied, Brk),
            new Instruction("ora", 0x01, 2, 6, AddressingMode.IndirectZeroPageWithXOffset, Ora),
            new Instruction("jam", 0x02, 1, 2, AddressingMode.Implied, Jam),
            new Instruction("slo", 0x03, 2, 8, AddressingMode.IndirectZeroPageWithXOffset, Slo),
            new Instruction("dop", 0x04, 2, 3, AddressingMode.ZeroPage, Dop),
            new Instruction("ora", 0x05, 2, 3, AddressingMode.ZeroPage, Ora),
            new Instruction("asl", 0x06, 2, 5, AddressingMode.ZeroPage, Asl),
            new Instruction("slo", 0x07, 2, 5, AddressingMode.ZeroPage, Slo),
            new Instruction("php", 0x08, 1, 3, AddressingMode.Implied, Php),
            new Instruction("ora", 0x09, 2, 2, AddressingMode.Immediate, Ora),
            new Instruction("asl", 0x0a, 1, 2, AddressingMode.Accumulator, Asl),
            new Instruction("???", 0x0b, 1, 2, AddressingMode.Implied, Nop),
            new Instruction("top", 0x0c, 3, 4, AddressingMode.Absolute, Top),
            new Instruction("ora", 0x0d, 3, 4, AddressingMode.Absolute, Ora),
            new Instruction("asl", 0x0e, 3, 6, AddressingMode.Absolute, Asl),
            new Instruction("slo", 0x0f, 3, 6, AddressingMode.Absolute, Slo),
            new Instruction("bpl", 0x10, 2, 2, AddressingMode.Relative, Bpl),
            new Instruction("ora", 0x11, 2, 5, AddressingMode.IndirectZeroPageWithYOffset, Ora),
            new Instruction("jam", 0x12, 1, 2, AddressingMode.Implied, Jam),
            new Instruction("slo", 0x13, 2, 8, AddressingMode.IndirectZeroPageWithYOffset, Slo),
            new Instruction("dop", 0x14, 2, 4, AddressingMode.ZeroPageWithXOffset, Dop),
            new Instruction("ora", 0x15, 2, 4, AddressingMode.ZeroPageWithXOffset, Ora),
            new Instruction("asl", 0x16, 2, 6, AddressingMode.ZeroPageWithXOffset, Asl),
            new Instruction("slo", 0x17, 2, 6, AddressingMode.ZeroPageWithXOffset, Slo),
            new Instruction("clc", 0x18, 1, 2, AddressingMode.Implied, Clc),
            new Instruction("ora", 0x19, 3, 4, AddressingMode.AbsoluteWithYOffset, Ora),
            new Instruction("nop", 0x1a, 1, 2, AddressingMode.Implied, Nop),
            new Instruction("slo", 0x1b, 3, 7, AddressingMode.AbsoluteWithYOffset, Slo),
            new Instruction("top", 0x1c, 3, 4, AddressingMode.AbsoluteWithXOffset, Top),
            new Instruction("ora", 0x1d, 3, 4, AddressingMode.AbsoluteWithXOffset, Ora),
            new Instruction("asl", 0x1e, 3, 7, AddressingMode.AbsoluteWithXOffset, Asl),
            new Instruction("slo", 0x1f, 3, 7, AddressingMode.AbsoluteWithXOffset, Slo),
            new Instruction("jsr", 0x20, 3, 6, AddressingMode.Absolute, Jsr),
            new Instruction("and", 0x21, 2, 6, AddressingMode.IndirectZeroPageWithXOffset, And),
            new Instruction("jam", 0x22, 1, 2, AddressingMode.Implied, Jam),
            new Instruction("rla", 0x23, 2, 8, AddressingMode.IndirectZeroPageWithXOffset, Rla),
            new Instruction("bit", 0x24, 2, 3, AddressingMode.ZeroPage, Bit),
            new Instruction("and", 0x25, 2, 3, AddressingMode.ZeroPage, And),
            new Instruction("rol", 0x26, 2, 5, AddressingMode.ZeroPage, Rol),
            new Instruction("rla", 0x27, 2, 5, AddressingMode.ZeroPage, Rla),
            new Instruction("plp", 0x28, 1, 4, AddressingMode.Implied, Plp),
            new Instruction("and", 0x29, 2, 2, AddressingMode.Immediate, And),
            new Instruction("rol", 0x2a, 1, 2, AddressingMode.Accumulator, Rol),
            new Instruction("???", 0x2b, 1, 2, AddressingMode.Implied, Nop),
            new Instruction("bit", 0x2c, 3, 4, AddressingMode.Absolute, Bit),
            new Instruction("and", 0x2d, 3, 4, AddressingMode.Absolute, And),
            new Instruction("rol", 0x2e, 3, 6, AddressingMode.Absolute, Rol),
            new Instruction("rla", 0x2f, 3, 6, AddressingMode.Absolute, Rla),
            new Instruction("bmi", 0x30, 2, 2, AddressingMode.Relative, Bmi),
            new Instruction("and", 0x31, 2, 5, AddressingMode.IndirectZeroPageWithYOffset, And),
            new Instruction("jam", 0x32, 1, 2, AddressingMode.Implied, Jam),
            new Instruction("rla", 0x33, 2, 8, AddressingMode.IndirectZeroPageWithYOffset, Rla),
            new Instruction("dop", 0x34, 2, 4, AddressingMode.ZeroPageWithXOffset, Dop),
            new Instruction("and", 0x35, 2, 4, AddressingMode.ZeroPageWithXOffset, And),
            new Instruction("rol", 0x36, 2, 6, AddressingMode.ZeroPageWithXOffset, Rol),
            new Instruction("rla", 0x37, 2, 6, AddressingMode.ZeroPageWithXOffset, Rla),
            new Instruction("sec", 0x38, 1, 2, AddressingMode.Implied, Sec),
            new Instruction("and", 0x39, 3, 4, AddressingMode.AbsoluteWithYOffset, And),
            new Instruction("nop", 0x3a, 1, 2, AddressingMode.Implied, Nop),
            new Instruction("rla", 0x3b, 3, 7, AddressingMode.AbsoluteWithYOffset, Rla),
            new Instruction("top", 0x3c, 3, 4, AddressingMode.AbsoluteWithXOffset, Top),
            new Instruction("and", 0x3d, 3, 4, AddressingMode.AbsoluteWithXOffset, And),
            new Instruction("rol", 0x3e, 3, 7, AddressingMode.AbsoluteWithXOffset, Rol),
            new Instruction("rla", 0x3f, 3, 7, AddressingMode.AbsoluteWithXOffset, Rla),
            new Instruction("rti", 0x40, 1, 6, AddressingMode.Implied, Rti),
            new Instruction("eor", 0x41, 2, 6, AddressingMode.IndirectZeroPageWithXOffset, Eor),
            new Instruction("jam", 0x42, 1, 2, AddressingMode.Implied, Jam),
            new Instruction("???", 0x43, 1, 2, AddressingMode.Implied, Nop),
            new Instruction("dop", 0x44, 2, 3, AddressingMode.ZeroPage, Dop),
            new Instruction("eor", 0x45, 2, 3, AddressingMode.ZeroPage, Eor),
            new Instruction("lsr", 0x46, 2, 5, AddressingMode.ZeroPage, Lsr),
            new Instruction("???", 0x47, 1, 2, AddressingMode.Implied, Nop),
            new Instruction("pha", 0x48, 1, 3, AddressingMode.Implied, Pha),
            new Instruction("eor", 0x49, 2, 2, AddressingMode.Immediate, Eor),
            new Instruction("lsr", 0x4a, 1, 2, AddressingMode.Accumulator, Lsr),
            new Instruction("???", 0x4b, 1, 2, AddressingMode.Implied, Nop),
            new Instruction("jmp", 0x4c, 3, 3, AddressingMode.Absolute, Jmp),
            new Instruction("eor", 0x4d, 3, 4, AddressingMode.Absolute, Eor),
            new Instruction("lsr", 0x4e, 3, 6, AddressingMode.Absolute, Lsr),
            new Instruction("???", 0x4f, 1, 2, AddressingMode.Implied, Nop),
            new Instruction("bvc", 0x50, 2, 2, AddressingMode.Relative, Bvc),
            new Instruction("eor", 0x51, 2, 5, AddressingMode.IndirectZeroPageWithYOffset, Eor),
            new Instruction("jam", 0x52, 1, 2, AddressingMode.Implied, Jam),
            new Instruction("???", 0x53, 1, 2, AddressingMode.Implied, Nop),
            new Instruction("dop", 0x54, 2, 4, AddressingMode.ZeroPageWithXOffset, Dop),
            new Instruction("eor", 0x55, 2, 4, AddressingMode.ZeroPageWithXOffset, Eor),
            new Instruction("lsr", 0x56, 2, 6, AddressingMode.ZeroPageWithXOffset, Lsr),
            new Instruction("???", 0x57, 1, 2, AddressingMode.Implied, Nop),
            new Instruction("cli", 0x58, 1, 2, AddressingMode.Implied, Cli),
            new Instruction("eor", 0x59, 3, 4, AddressingMode.AbsoluteWithYOffset, Eor),
            new Instruction("nop", 0x5a, 1, 2, AddressingMode.Implied, Nop),
            new Instruction("???", 0x5b, 1, 2, AddressingMode.Implied, Nop),
            new Instruction("top", 0x5c, 3, 4, AddressingMode.AbsoluteWithXOffset, Top),
            new Instruction("eor", 0x5d, 3, 4, AddressingMode.AbsoluteWithXOffset, Eor),
            new Instruction("lsr", 0x5e, 3, 7, AddressingMode.AbsoluteWithXOffset, Lsr),
            new Instruction("???", 0x5f, 1, 2, AddressingMode.Implied, Nop),
            new Instruction("rts", 0x60, 1, 6, AddressingMode.Implied, Rts),
            new Instruction("adc", 0x61, 2, 6, AddressingMode.IndirectZeroPageWithXOffset, Adc),
            new Instruction("jam", 0x62, 1, 2, AddressingMode.Implied, Jam),
            new Instruction("rra", 0x63, 2, 8, AddressingMode.IndirectZeroPageWithXOffset, Rra),
            new Instruction("dop", 0x64, 2, 3, AddressingMode.ZeroPage, Dop),
            new Instruction("adc", 0x65, 2, 3, AddressingMode.ZeroPage, Adc),
            new Instruction("ror", 0x66, 2, 5, AddressingMode.ZeroPage, Ror),
            new Instruction("rra", 0x67, 2, 5, AddressingMode.ZeroPage, Rra),
            new Instruction("pla", 0x68, 1, 4, AddressingMode.Implied, Pla),
            new Instruction("adc", 0x69, 2, 2, AddressingMode.Immediate, Adc),
            new Instruction("ror", 0x6a, 1, 2, AddressingMode.Accumulator, Ror),
            new Instruction("???", 0x6b, 1, 2, AddressingMode.Implied, Nop),
            new Instruction("jmp", 0x6c, 3, 5, AddressingMode.Indirect, Jmp),
            new Instruction("adc", 0x6d, 3, 4, AddressingMode.Absolute, Adc),
            new Instruction("ror", 0x6e, 3, 6, AddressingMode.Absolute, Ror),
            new Instruction("rra", 0x6f, 3, 6, AddressingMode.Absolute, Rra),
            new Instruction("bvs", 0x70, 2, 2, AddressingMode.Relative, Bvs),
            new Instruction("adc", 0x71, 2, 5, AddressingMode.IndirectZeroPageWithYOffset, Adc),
            new Instruction("jam", 0x72, 1, 2, AddressingMode.Implied, Jam),
            new Instruction("rra", 0x73, 2, 8, AddressingMode.IndirectZeroPageWithYOffset, Rra),
            new Instruction("dop", 0x74, 2, 4, AddressingMode.ZeroPageWithXOffset, Dop),
            new Instruction("adc", 0x75, 2, 4, AddressingMode.ZeroPageWithXOffset, Adc),
            new Instruction("ror", 0x76, 2, 6, AddressingMode.ZeroPageWithXOffset, Ror),
            new Instruction("rra", 0x77, 2, 6, AddressingMode.ZeroPageWithXOffset, Rra),
            new Instruction("sei", 0x78, 1, 2, AddressingMode.Implied, Sei),
            new Instruction("adc", 0x79, 3, 4, AddressingMode.AbsoluteWithYOffset, Adc),
            new Instruction("nop", 0x7a, 1, 2, AddressingMode.Implied, Nop),
            new Instruction("rra", 0x7b, 3, 7, AddressingMode.AbsoluteWithYOffset, Rra),
            new Instruction("top", 0x7c, 3, 4, AddressingMode.AbsoluteWithXOffset, Top),
            new Instruction("adc", 0x7d, 3, 4, AddressingMode.AbsoluteWithXOffset, Adc),
            new Instruction("ror", 0x7e, 3, 7, AddressingMode.AbsoluteWithXOffset, Ror),
            new Instruction("rra", 0x7f, 3, 7, AddressingMode.AbsoluteWithXOffset, Rra),
            new Instruction("dop", 0x80, 2, 2, AddressingMode.Immediate, Dop),
            new Instruction("sta", 0x81, 2, 6, AddressingMode.IndirectZeroPageWithXOffset, Sta),
            new Instruction("dop", 0x82, 2, 2, AddressingMode.Immediate, Dop),
            new Instruction("sax", 0x83, 2, 6, AddressingMode.IndirectZeroPageWithXOffset, Sax),
            new Instruction("sty", 0x84, 2, 3, AddressingMode.ZeroPage, Sty),
            new Instruction("sta", 0x85, 2, 3, AddressingMode.ZeroPage, Sta),
            new Instruction("stx", 0x86, 2, 3, AddressingMode.ZeroPage, Stx),
            new Instruction("sax", 0x87, 2, 3, AddressingMode.ZeroPage, Sax),
            new Instruction("dey", 0x88, 1, 2, AddressingMode.Implied, Dey),
            new Instruction("dop", 0x89, 2, 2, AddressingMode.Immediate, Dop),
            new Instruction("txa", 0x8a, 1, 2, AddressingMode.Implied, Txa),
            new Instruction("???", 0x8b, 1, 2, AddressingMode.Implied, Nop),
            new Instruction("sty", 0x8c, 3, 4, AddressingMode.Absolute, Sty),
            new Instruction("sta", 0x8d, 3, 4, AddressingMode.Absolute, Sta),
            new Instruction("stx", 0x8e, 3, 4, AddressingMode.Absolute, Stx),
            new Instruction("sax", 0x8f, 3, 4, AddressingMode.Absolute, Sax),
            new Instruction("bcc", 0x90, 2, 2, AddressingMode.Relative, Bcc),
            new Instruction("sta", 0x91, 2, 6, AddressingMode.IndirectZeroPageWithYOffset, Sta),
            new Instruction("jam", 0x92, 1, 2, AddressingMode.Implied, Jam),
            new Instruction("???", 0x93, 1, 2, AddressingMode.Implied, Nop),
            new Instruction("sty", 0x94, 2, 4, AddressingMode.ZeroPageWithXOffset, Sty),
            new Instruction("sta", 0x95, 2, 4, AddressingMode.ZeroPageWithXOffset, Sta),
            new Instruction("stx", 0x96, 2, 4, AddressingMode.ZeroPageWithYOffset, Stx),
            new Instruction("sax", 0x97, 2, 4, AddressingMode.ZeroPageWithYOffset, Sax),
            new Instruction("tya", 0x98, 1, 2, AddressingMode.Implied, Tya),
            new Instruction("sta", 0x99, 3, 5, AddressingMode.AbsoluteWithYOffset, Sta),
            new Instruction("txs", 0x9a, 1, 2, AddressingMode.Implied, Txs),
            new Instruction("???", 0x9b, 1, 2, AddressingMode.Implied, Nop),
            new Instruction("???", 0x9c, 1, 2, AddressingMode.Implied, Nop),
            new Instruction("sta", 0x9d, 3, 5, AddressingMode.AbsoluteWithXOffset, Sta),
            new Instruction("???", 0x9e, 1, 2, AddressingMode.Implied, Nop),
            new Instruction("???", 0x9f, 1, 2, AddressingMode.Implied, Nop),
            new Instruction("ldy", 0xa0, 2, 2, AddressingMode.Immediate, Ldy),
            new Instruction("lda", 0xa1, 2, 6, AddressingMode.IndirectZeroPageWithXOffset, Lda),
            new Instruction("ldx", 0xa2, 2, 2, AddressingMode.Immediate, Ldx),
            new Instruction("lax", 0xa3, 2, 6, AddressingMode.IndirectZeroPageWithXOffset, Lax),
            new Instruction("ldy", 0xa4, 2, 3, AddressingMode.ZeroPage, Ldy),
            new Instruction("lda", 0xa5, 2, 3, AddressingMode.ZeroPage, Lda),
            new Instruction("ldx", 0xa6, 2, 3, AddressingMode.ZeroPage, Ldx),
            new Instruction("lax", 0xa7, 2, 3, AddressingMode.ZeroPage, Lax),
            new Instruction("tay", 0xa8, 1, 2, AddressingMode.Implied, Tay),
            new Instruction("lda", 0xa9, 2, 2, AddressingMode.Immediate, Lda),
            new Instruction("tax", 0xaa, 1, 2, AddressingMode.Implied, Tax),
            new Instruction("???", 0xab, 1, 2, AddressingMode.Implied, Nop),
            new Instruction("ldy", 0xac, 3, 4, AddressingMode.Absolute, Ldy),
            new Instruction("lda", 0xad, 3, 4, AddressingMode.Absolute, Lda),
            new Instruction("ldx", 0xae, 3, 4, AddressingMode.Absolute, Ldx),
            new Instruction("lax", 0xaf, 3, 4, AddressingMode.Absolute, Lax),
            new Instruction("bcs", 0xb0, 2, 2, AddressingMode.Relative, Bcs),
            new Instruction("lda", 0xb1, 2, 5, AddressingMode.IndirectZeroPageWithYOffset, Lda),
            new Instruction("jam", 0xb2, 1, 2, AddressingMode.Implied, Jam),
            new Instruction("lax", 0xb3, 2, 5, AddressingMode.IndirectZeroPageWithYOffset, Lax),
            new Instruction("ldy", 0xb4, 2, 4, AddressingMode.ZeroPageWithXOffset, Ldy),
            new Instruction("lda", 0xb5, 2, 4, AddressingMode.ZeroPageWithXOffset, Lda),
            new Instruction("ldx", 0xb6, 2, 4, AddressingMode.ZeroPageWithYOffset, Ldx),
            new Instruction("lax", 0xb7, 2, 4, AddressingMode.ZeroPageWithYOffset, Lax),
            new Instruction("clv", 0xb8, 1, 2, AddressingMode.Implied, Clv),
            new Instruction("lda", 0xb9, 3, 4, AddressingMode.AbsoluteWithYOffset, Lda),
            new Instruction("tsx", 0xba, 1, 2, AddressingMode.Implied, Tsx),
            new Instruction("???", 0xbb, 1, 2, AddressingMode.Implied, Nop),
            new Instruction("ldy", 0xbc, 3, 4, AddressingMode.AbsoluteWithXOffset, Ldy),
            new Instruction("lda", 0xbd, 3, 4, AddressingMode.AbsoluteWithXOffset, Lda),
            new Instruction("ldx", 0xbe, 3, 4, AddressingMode.AbsoluteWithYOffset, Ldx),
            new Instruction("lax", 0xbf, 3, 4, AddressingMode.AbsoluteWithYOffset, Lax),
            new Instruction("cpy", 0xc0, 2, 2, AddressingMode.Immediate, Cpy),
            new Instruction("cmp", 0xc1, 2, 6, AddressingMode.IndirectZeroPageWithXOffset, Cmp),
            new Instruction("dop", 0xc2, 2, 2, AddressingMode.Immediate, Dop),
            new Instruction("???", 0xc3, 1, 2, AddressingMode.Implied, Nop),
            new Instruction("cpy", 0xc4, 2, 3, AddressingMode.ZeroPage, Cpy),
            new Instruction("cmp", 0xc5, 2, 3, AddressingMode.ZeroPage, Cmp),
            new Instruction("dec", 0xc6, 2, 5, AddressingMode.ZeroPage, Dec),
            new Instruction("???", 0xc7, 1, 2, AddressingMode.Implied, Nop),
            new Instruction("iny", 0xc8, 1, 2, AddressingMode.Implied, Iny),
            new Instruction("cmp", 0xc9, 2, 2, AddressingMode.Immediate, Cmp),
            new Instruction("dex", 0xca, 1, 2, AddressingMode.Implied, Dex),
            new Instruction("sbx", 0xcb, 2, 2, AddressingMode.Immediate, Sbx),
            new Instruction("cpy", 0xcc, 3, 4, AddressingMode.Absolute, Cpy),
            new Instruction("cmp", 0xcd, 3, 4, AddressingMode.Absolute, Cmp),
            new Instruction("dec", 0xce, 3, 6, AddressingMode.Absolute, Dec),
            new Instruction("???", 0xcf, 1, 2, AddressingMode.Implied, Nop),
            new Instruction("bne", 0xd0, 2, 2, AddressingMode.Relative, Bne),
            new Instruction("cmp", 0xd1, 2, 5, AddressingMode.IndirectZeroPageWithYOffset, Cmp),
            new Instruction("jam", 0xd2, 1, 2, AddressingMode.Implied, Jam),
            new Instruction("???", 0xd3, 1, 2, AddressingMode.Implied, Nop),
            new Instruction("dop", 0xd4, 2, 4, AddressingMode.ZeroPageWithXOffset, Dop),
            new Instruction("cmp", 0xd5, 2, 4, AddressingMode.ZeroPageWithXOffset, Cmp),
            new Instruction("dec", 0xd6, 2, 6, AddressingMode.ZeroPageWithXOffset, Dec),
            new Instruction("???", 0xd7, 1, 2, AddressingMode.Implied, Nop),
            new Instruction("cld", 0xd8, 1, 2, AddressingMode.Implied, Cld),
            new Instruction("cmp", 0xd9, 3, 4, AddressingMode.AbsoluteWithYOffset, Cmp),
            new Instruction("nop", 0xda, 1, 2, AddressingMode.Implied, Nop),
            new Instruction("???", 0xdb, 1, 2, AddressingMode.Implied, Nop),
            new Instruction("top", 0xdc, 3, 4, AddressingMode.AbsoluteWithXOffset, Top),
            new Instruction("cmp", 0xdd, 3, 4, AddressingMode.AbsoluteWithXOffset, Cmp),
            new Instruction("dec", 0xde, 3, 7, AddressingMode.AbsoluteWithXOffset, Dec),
            new Instruction("???", 0xdf, 1, 2, AddressingMode.Implied, Nop),
            new Instruction("cpx", 0xe0, 2, 2, AddressingMode.Immediate, Cpx),
            new Instruction("sbc", 0xe1, 2, 6, AddressingMode.IndirectZeroPageWithXOffset, Sbc),
            new Instruction("dop", 0xe2, 2, 2, AddressingMode.Immediate, Dop),
            new Instruction("???", 0xe3, 1, 2, AddressingMode.Implied, Nop),
            new Instruction("cpx", 0xe4, 2, 3, AddressingMode.ZeroPage, Cpx),
            new Instruction("sbc", 0xe5, 2, 3, AddressingMode.ZeroPage, Sbc),
            new Instruction("inc", 0xe6, 2, 5, AddressingMode.ZeroPage, Inc),
            new Instruction("???", 0xe7, 1, 2, AddressingMode.Implied, Nop),
            new Instruction("inx", 0xe8, 1, 2, AddressingMode.Implied, Inx),
            new Instruction("sbc", 0xe9, 2, 2, AddressingMode.Immediate, Sbc),
            new Instruction("nop", 0xea, 1, 2, AddressingMode.Implied, Nop),
            new Instruction("sbc", 0xeb, 2, 2, AddressingMode.Immediate, Sbc),
            new Instruction("cpx", 0xec, 3, 4, AddressingMode.Absolute, Cpx),
            new Instruction("sbc", 0xed, 3, 4, AddressingMode.Absolute, Sbc),
            new Instruction("inc", 0xee, 3, 6, AddressingMode.Absolute, Inc),
            new Instruction("???", 0xef, 1, 2, AddressingMode.Implied, Nop),
            new Instruction("beq", 0xf0, 2, 2, AddressingMode.Relative, Beq),
            new Instruction("sbc", 0xf1, 2, 5, AddressingMode.IndirectZeroPageWithYOffset, Sbc),
            new Instruction("jam", 0xf2, 1, 2, AddressingMode.Implied, Jam),
            new Instruction("???", 0xf3, 1, 2, AddressingMode.Implied, Nop),
            new Instruction("dop", 0xf4, 2, 4, AddressingMode.ZeroPageWithXOffset, Dop),
            new Instruction("sbc", 0xf5, 2, 4, AddressingMode.ZeroPageWithXOffset, Sbc),
            new Instruction("inc", 0xf6, 2, 6, AddressingMode.ZeroPageWithXOffset, Inc),
            new Instruction("???", 0xf7, 1, 2, AddressingMode.Implied, Nop),
            new Instruction("sed", 0xf8, 1, 2, AddressingMode.Implied, Sed),
            new Instruction("sbc", 0xf9, 3, 4, AddressingMode.AbsoluteWithYOffset, Sbc),
            new Instruction("nop", 0xfa, 1, 2, AddressingMode.Implied, Nop),
            new Instruction("???", 0xfb, 1, 2, AddressingMode.Implied, Nop),
            new Instruction("top", 0xfc, 3, 4, AddressingMode.AbsoluteWithXOffset, Top),
            new Instruction("sbc", 0xfd, 3, 4, AddressingMode.AbsoluteWithXOffset, Sbc),
            new Instruction("inc", 0xfe, 3, 7, AddressingMode.AbsoluteWithXOffset, Inc),
            new Instruction("???", 0xff, 1, 2, AddressingMode.Implied, Nop),
        };

        private Instruction(string name, byte opCode, int size, int baseCycles, AddressingMode addressingMode, InstructionExecutor instructionExecutor)
        {
            if (!Enum.IsDefined(typeof(AddressingMode), addressingMode)) throw new InvalidEnumArgumentException(nameof(addressingMode), (int)addressingMode, typeof(AddressingMode));
            if (baseCycles <= 0) throw new ArgumentOutOfRangeException(nameof(baseCycles));

            Name = name ?? throw new ArgumentNullException(nameof(name));
            OpCode = opCode;
            Size = size;
            BaseCycles = baseCycles;
            AddressingMode = addressingMode;
            _instructionExecutor = instructionExecutor ?? throw new ArgumentNullException(nameof(instructionExecutor));
        }

        private Instruction(string name, byte opCode, int size, int baseCycles, AddressingMode addressingMode, InstructionExecutorEx instructionExecutorEx)
        {
            if (!Enum.IsDefined(typeof(AddressingMode), addressingMode)) throw new InvalidEnumArgumentException(nameof(addressingMode), (int)addressingMode, typeof(AddressingMode));
            if (baseCycles <= 0) throw new ArgumentOutOfRangeException(nameof(baseCycles));

            Name = name ?? throw new ArgumentNullException(nameof(name));
            OpCode = opCode;
            Size = size;
            BaseCycles = baseCycles;
            AddressingMode = addressingMode;
            _instructionExecutorEx = instructionExecutorEx ?? throw new ArgumentNullException(nameof(instructionExecutorEx));
        }

        public int Execute(IBus bus, CpuState cpuState)
        {
            if (_instructionExecutor != null)
            {
                return _instructionExecutor(AddressingMode, BaseCycles, bus, cpuState);
            }

            if (_instructionExecutorEx != null)
            {
                return _instructionExecutorEx(AddressingMode, BaseCycles, bus, cpuState, null, out _);
            }

            return 0;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{Name}";
        }

        public static Instruction GetInstruction(byte opCode) => Instructions[opCode];
    }
}