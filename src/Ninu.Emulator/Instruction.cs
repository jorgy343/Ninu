using System;
using System.ComponentModel;

namespace Ninu.Emulator
{
    public delegate int InstructionExecutor(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState);

    public class Instruction
    {
        public string Name { get; }
        public byte OpCode { get; }
        public int Size { get; }
        public int BaseCycles { get; }
        public AddressingMode AddressingMode { get; }

        private readonly InstructionExecutor _instructionExecutor;

        private static readonly Instruction[] Instructions =
        {
            new Instruction("brk", 0x00, 1, 7, AddressingMode.Implied, Brk),
            new Instruction("ora", 0x01, 2, 6, AddressingMode.IndirectZeroPageWithXOffset, Ora),
            new Instruction("???", 0x02, 1, 2, AddressingMode.Implied, Nop),
            new Instruction("???", 0x03, 1, 2, AddressingMode.Implied, Nop),
            new Instruction("dop", 0x04, 2, 3, AddressingMode.ZeroPage, Dop),
            new Instruction("ora", 0x05, 2, 3, AddressingMode.ZeroPage, Ora),
            new Instruction("asl", 0x06, 2, 5, AddressingMode.ZeroPage, Asl),
            new Instruction("???", 0x07, 1, 2, AddressingMode.Implied, Nop),
            new Instruction("php", 0x08, 1, 3, AddressingMode.Implied, Php),
            new Instruction("ora", 0x09, 2, 2, AddressingMode.Immediate, Ora),
            new Instruction("asl", 0x0a, 1, 2, AddressingMode.Accumulator, Asl),
            new Instruction("???", 0x0b, 1, 2, AddressingMode.Implied, Nop),
            new Instruction("top", 0x0c, 3, 4, AddressingMode.Absolute, Top),
            new Instruction("ora", 0x0d, 3, 4, AddressingMode.Absolute, Ora),
            new Instruction("asl", 0x0e, 3, 6, AddressingMode.Absolute, Asl),
            new Instruction("???", 0x0f, 1, 2, AddressingMode.Implied, Nop),
            new Instruction("bpl", 0x10, 2, 2, AddressingMode.Relative, Bpl),
            new Instruction("ora", 0x11, 2, 5, AddressingMode.IndirectZeroPageWithYOffset, Ora),
            new Instruction("???", 0x12, 1, 2, AddressingMode.Implied, Nop),
            new Instruction("???", 0x13, 1, 2, AddressingMode.Implied, Nop),
            new Instruction("dop", 0x14, 2, 4, AddressingMode.ZeroPageWithXOffset, Dop),
            new Instruction("ora", 0x15, 2, 4, AddressingMode.ZeroPageWithXOffset, Ora),
            new Instruction("asl", 0x16, 2, 6, AddressingMode.ZeroPageWithXOffset, Asl),
            new Instruction("???", 0x17, 1, 2, AddressingMode.Implied, Nop),
            new Instruction("clc", 0x18, 1, 2, AddressingMode.Implied, Clc),
            new Instruction("ora", 0x19, 3, 4, AddressingMode.AbsoluteWithYOffset, Ora),
            new Instruction("nop", 0x1a, 1, 2, AddressingMode.Implied, Nop),
            new Instruction("???", 0x1b, 1, 2, AddressingMode.Implied, Nop),
            new Instruction("top", 0x1c, 3, 4, AddressingMode.AbsoluteWithXOffset, Top),
            new Instruction("ora", 0x1d, 3, 4, AddressingMode.AbsoluteWithXOffset, Ora),
            new Instruction("asl", 0x1e, 3, 7, AddressingMode.AbsoluteWithXOffset, Asl),
            new Instruction("???", 0x1f, 1, 2, AddressingMode.Implied, Nop),
            new Instruction("jsr", 0x20, 3, 6, AddressingMode.Absolute, Jsr),
            new Instruction("and", 0x21, 2, 6, AddressingMode.IndirectZeroPageWithXOffset, And),
            new Instruction("???", 0x22, 1, 2, AddressingMode.Implied, Nop),
            new Instruction("???", 0x23, 1, 2, AddressingMode.Implied, Nop),
            new Instruction("bit", 0x24, 2, 3, AddressingMode.ZeroPage, Bit),
            new Instruction("and", 0x25, 2, 3, AddressingMode.ZeroPage, And),
            new Instruction("rol", 0x26, 2, 5, AddressingMode.ZeroPage, Rol),
            new Instruction("???", 0x27, 1, 2, AddressingMode.Implied, Nop),
            new Instruction("plp", 0x28, 1, 4, AddressingMode.Implied, Plp),
            new Instruction("and", 0x29, 2, 2, AddressingMode.Immediate, And),
            new Instruction("rol", 0x2a, 1, 2, AddressingMode.Accumulator, Rol),
            new Instruction("???", 0x2b, 1, 2, AddressingMode.Implied, Nop),
            new Instruction("bit", 0x2c, 3, 4, AddressingMode.Absolute, Bit),
            new Instruction("and", 0x2d, 3, 4, AddressingMode.Absolute, And),
            new Instruction("rol", 0x2e, 3, 6, AddressingMode.Absolute, Rol),
            new Instruction("???", 0x2f, 1, 2, AddressingMode.Implied, Nop),
            new Instruction("bmi", 0x30, 2, 2, AddressingMode.Relative, Bmi),
            new Instruction("and", 0x31, 2, 5, AddressingMode.IndirectZeroPageWithYOffset, And),
            new Instruction("???", 0x32, 1, 2, AddressingMode.Implied, Nop),
            new Instruction("???", 0x33, 1, 2, AddressingMode.Implied, Nop),
            new Instruction("dop", 0x34, 2, 4, AddressingMode.ZeroPageWithXOffset, Dop),
            new Instruction("and", 0x35, 2, 4, AddressingMode.ZeroPageWithXOffset, And),
            new Instruction("rol", 0x36, 2, 6, AddressingMode.ZeroPageWithXOffset, Rol),
            new Instruction("???", 0x37, 1, 2, AddressingMode.Implied, Nop),
            new Instruction("sec", 0x38, 1, 2, AddressingMode.Implied, Sec),
            new Instruction("and", 0x39, 3, 4, AddressingMode.AbsoluteWithYOffset, And),
            new Instruction("nop", 0x3a, 1, 2, AddressingMode.Implied, Nop),
            new Instruction("???", 0x3b, 1, 2, AddressingMode.Implied, Nop),
            new Instruction("top", 0x3c, 3, 4, AddressingMode.AbsoluteWithXOffset, Top),
            new Instruction("and", 0x3d, 3, 4, AddressingMode.AbsoluteWithXOffset, And),
            new Instruction("rol", 0x3e, 3, 7, AddressingMode.AbsoluteWithXOffset, Rol),
            new Instruction("???", 0x3f, 1, 2, AddressingMode.Implied, Nop),
            new Instruction("rti", 0x40, 1, 6, AddressingMode.Implied, Rti),
            new Instruction("eor", 0x41, 2, 6, AddressingMode.IndirectZeroPageWithXOffset, Eor),
            new Instruction("???", 0x42, 1, 2, AddressingMode.Implied, Nop),
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
            new Instruction("???", 0x52, 1, 2, AddressingMode.Implied, Nop),
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
            new Instruction("???", 0x62, 1, 2, AddressingMode.Implied, Nop),
            new Instruction("???", 0x63, 1, 2, AddressingMode.Implied, Nop),
            new Instruction("dop", 0x64, 2, 3, AddressingMode.ZeroPage, Dop),
            new Instruction("adc", 0x65, 2, 3, AddressingMode.ZeroPage, Adc),
            new Instruction("ror", 0x66, 2, 5, AddressingMode.ZeroPage, Ror),
            new Instruction("???", 0x67, 1, 2, AddressingMode.Implied, Nop),
            new Instruction("pla", 0x68, 1, 4, AddressingMode.Implied, Pla),
            new Instruction("adc", 0x69, 2, 2, AddressingMode.Immediate, Adc),
            new Instruction("ror", 0x6a, 1, 2, AddressingMode.Accumulator, Ror),
            new Instruction("???", 0x6b, 1, 2, AddressingMode.Implied, Nop),
            new Instruction("jmp", 0x6c, 3, 5, AddressingMode.Indirect, Jmp),
            new Instruction("adc", 0x6d, 3, 4, AddressingMode.Absolute, Adc),
            new Instruction("ror", 0x6e, 3, 6, AddressingMode.Absolute, Ror),
            new Instruction("???", 0x6f, 1, 2, AddressingMode.Implied, Nop),
            new Instruction("bvs", 0x70, 2, 2, AddressingMode.Relative, Bvs),
            new Instruction("adc", 0x71, 2, 5, AddressingMode.IndirectZeroPageWithYOffset, Adc),
            new Instruction("???", 0x72, 1, 2, AddressingMode.Implied, Nop),
            new Instruction("???", 0x73, 1, 2, AddressingMode.Implied, Nop),
            new Instruction("dop", 0x74, 2, 4, AddressingMode.ZeroPageWithXOffset, Dop),
            new Instruction("adc", 0x75, 2, 4, AddressingMode.ZeroPageWithXOffset, Adc),
            new Instruction("ror", 0x76, 2, 6, AddressingMode.ZeroPageWithXOffset, Ror),
            new Instruction("???", 0x77, 1, 2, AddressingMode.Implied, Nop),
            new Instruction("sei", 0x78, 1, 2, AddressingMode.Implied, Sei),
            new Instruction("adc", 0x79, 3, 4, AddressingMode.AbsoluteWithYOffset, Adc),
            new Instruction("nop", 0x7a, 1, 2, AddressingMode.Implied, Nop),
            new Instruction("???", 0x7b, 1, 2, AddressingMode.Implied, Nop),
            new Instruction("top", 0x7c, 3, 4, AddressingMode.AbsoluteWithXOffset, Top),
            new Instruction("adc", 0x7d, 3, 4, AddressingMode.AbsoluteWithXOffset, Adc),
            new Instruction("ror", 0x7e, 3, 7, AddressingMode.AbsoluteWithXOffset, Ror),
            new Instruction("???", 0x7f, 1, 2, AddressingMode.Implied, Nop),
            new Instruction("dop", 0x80, 2, 2, AddressingMode.Immediate, Dop),
            new Instruction("sta", 0x81, 2, 6, AddressingMode.IndirectZeroPageWithXOffset, Sta),
            new Instruction("dop", 0x82, 2, 2, AddressingMode.Immediate, Dop),
            new Instruction("???", 0x83, 1, 2, AddressingMode.Implied, Nop),
            new Instruction("sty", 0x84, 2, 3, AddressingMode.ZeroPage, Sty),
            new Instruction("sta", 0x85, 2, 3, AddressingMode.ZeroPage, Sta),
            new Instruction("stx", 0x86, 2, 3, AddressingMode.ZeroPage, Stx),
            new Instruction("???", 0x87, 1, 2, AddressingMode.Implied, Nop),
            new Instruction("dey", 0x88, 1, 2, AddressingMode.Implied, Dey),
            new Instruction("dop", 0x89, 2, 2, AddressingMode.Immediate, Dop),
            new Instruction("txa", 0x8a, 1, 2, AddressingMode.Implied, Txa),
            new Instruction("???", 0x8b, 1, 2, AddressingMode.Implied, Nop),
            new Instruction("sty", 0x8c, 3, 4, AddressingMode.Absolute, Sty),
            new Instruction("sta", 0x8d, 3, 4, AddressingMode.Absolute, Sta),
            new Instruction("stx", 0x8e, 3, 4, AddressingMode.Absolute, Stx),
            new Instruction("???", 0x8f, 1, 2, AddressingMode.Implied, Nop),
            new Instruction("bcc", 0x90, 2, 2, AddressingMode.Relative, Bcc),
            new Instruction("sta", 0x91, 2, 6, AddressingMode.IndirectZeroPageWithYOffset, Sta),
            new Instruction("???", 0x92, 1, 2, AddressingMode.Implied, Nop),
            new Instruction("???", 0x93, 1, 2, AddressingMode.Implied, Nop),
            new Instruction("sty", 0x94, 2, 4, AddressingMode.ZeroPageWithXOffset, Sty),
            new Instruction("sta", 0x95, 2, 4, AddressingMode.ZeroPageWithXOffset, Sta),
            new Instruction("stx", 0x96, 2, 4, AddressingMode.ZeroPageWithYOffset, Stx),
            new Instruction("???", 0x97, 1, 2, AddressingMode.Implied, Nop),
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
            new Instruction("???", 0xb2, 1, 2, AddressingMode.Implied, Nop),
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
            new Instruction("???", 0xcb, 1, 2, AddressingMode.Implied, Nop),
            new Instruction("cpy", 0xcc, 3, 4, AddressingMode.Absolute, Cpy),
            new Instruction("cmp", 0xcd, 3, 4, AddressingMode.Absolute, Cmp),
            new Instruction("dec", 0xce, 3, 6, AddressingMode.Absolute, Dec),
            new Instruction("???", 0xcf, 1, 2, AddressingMode.Implied, Nop),
            new Instruction("bne", 0xd0, 2, 2, AddressingMode.Relative, Bne),
            new Instruction("cmp", 0xd1, 2, 5, AddressingMode.IndirectZeroPageWithYOffset, Cmp),
            new Instruction("???", 0xd2, 1, 2, AddressingMode.Implied, Nop),
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
            new Instruction("???", 0xeb, 1, 2, AddressingMode.Implied, Nop),
            new Instruction("cpx", 0xec, 3, 4, AddressingMode.Absolute, Cpx),
            new Instruction("sbc", 0xed, 3, 4, AddressingMode.Absolute, Sbc),
            new Instruction("inc", 0xee, 3, 6, AddressingMode.Absolute, Inc),
            new Instruction("???", 0xef, 1, 2, AddressingMode.Implied, Nop),
            new Instruction("beq", 0xf0, 2, 2, AddressingMode.Relative, Beq),
            new Instruction("sbc", 0xf1, 2, 5, AddressingMode.IndirectZeroPageWithYOffset, Sbc),
            new Instruction("???", 0xf2, 1, 2, AddressingMode.Implied, Nop),
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

        public Instruction(string name, byte opCode, int size, int baseCycles, AddressingMode addressingMode, InstructionExecutor instructionExecutor)
        {
            if (!Enum.IsDefined(typeof(AddressingMode), addressingMode))
                throw new InvalidEnumArgumentException(nameof(addressingMode), (int)addressingMode, typeof(AddressingMode));
            if (baseCycles <= 0) throw new ArgumentOutOfRangeException(nameof(baseCycles));

            Name = name ?? throw new ArgumentNullException(nameof(name));
            OpCode = opCode;
            Size = size;
            BaseCycles = baseCycles;
            AddressingMode = addressingMode;
            _instructionExecutor = instructionExecutor ?? throw new ArgumentNullException(nameof(instructionExecutor));
        }

        public int Execute(IBus bus, CpuState cpuState)
        {
            return _instructionExecutor(AddressingMode, BaseCycles, bus, cpuState);
        }

        public static Instruction GetInstruction(byte opCode) => Instructions[opCode];

        public static (ushort Address, int AdditionalCycles) GetAddress(AddressingMode addressingMode, IBus bus,
            CpuState cpuState)
        {
            switch (addressingMode)
            {
                case AddressingMode.Implied:
                case AddressingMode.Accumulator:
                case AddressingMode.Immediate:
                    {
                        throw new InvalidOperationException("Cannot get an address when addressing mode is Implied, Accumulator, or Immediate.");
                    }

                case AddressingMode.ZeroPage:
                    {
                        ushort address = bus.Read(cpuState.PC++);

                        return (address, 0);
                    }

                case AddressingMode.ZeroPageWithXOffset:
                    {
                        var address = (ushort)bus.Read(cpuState.PC++);
                        var offsetAddress = (ushort)(address + cpuState.X);

                        offsetAddress &= 0x00ff;

                        return (offsetAddress, 0);
                    }

                case AddressingMode.ZeroPageWithYOffset:
                    {
                        var address = (ushort)bus.Read(cpuState.PC++);
                        var offsetAddress = (ushort)(address + cpuState.Y);

                        offsetAddress &= 0x00ff;

                        return (offsetAddress, 0);
                    }

                case AddressingMode.Absolute:
                    {
                        var addressLow = (ushort)bus.Read(cpuState.PC++);
                        var addressHigh = (ushort)(bus.Read(cpuState.PC++) << 8);

                        var address = (ushort)(addressLow | addressHigh);

                        return (address, 0);
                    }

                case AddressingMode.AbsoluteWithXOffset:
                    {
                        var addressLow = (ushort)bus.Read(cpuState.PC++);
                        var addressHigh = (ushort)(bus.Read(cpuState.PC++) << 8);

                        var address = (ushort)(addressLow | addressHigh);
                        var offsetAddress = (ushort)(address + cpuState.X);

                        // Compare the high byte of the address and offset address. If they are different,
                        // add an additional cycle as a penalty.
                        var additionalCycles = IsDifferentPage(address, offsetAddress) ? 1 : 0;

                        return (offsetAddress, additionalCycles);
                    }

                case AddressingMode.AbsoluteWithYOffset:
                    {
                        var addressLow = (ushort)bus.Read(cpuState.PC++);
                        var addressHigh = (ushort)(bus.Read(cpuState.PC++) << 8);

                        var address = (ushort)(addressLow | addressHigh);
                        var offsetAddress = (ushort)(address + cpuState.Y);

                        // Compare the high byte of the address and offset address. If they are different,
                        // add an additional cycle as a penalty.
                        var additionalCycles = IsDifferentPage(address, offsetAddress) ? 1 : 0;

                        return (offsetAddress, additionalCycles);
                    }

                case AddressingMode.Indirect:
                    {
                        var addressLow = (ushort)bus.Read(cpuState.PC++);
                        var addressHigh = (ushort)(bus.Read(cpuState.PC++) << 8);

                        var address = (ushort)(addressLow | addressHigh);

                        // This simulates a bug in the 6502 hardware.
                        if (addressLow == 0x00ff)
                        {
                            var newAddressLow = (ushort)bus.Read(address);
                            var newAddressHigh = (ushort)(bus.Read((ushort)(address & 0xff00)) << 8);

                            var newAddress = (ushort)(newAddressLow | newAddressHigh);

                            return (newAddress, 0);
                        }
                        else
                        {
                            var newAddressLow = (ushort)bus.Read(address);
                            var newAddressHigh = (ushort)(bus.Read((ushort)(address + 1)) << 8);

                            var newAddress = (ushort)(newAddressLow | newAddressHigh);

                            return (newAddress, 0);
                        }
                    }

                case AddressingMode.IndirectZeroPageWithXOffset:
                    {
                        var address = (ushort)bus.Read(cpuState.PC++);

                        address = (ushort)((address + cpuState.X) & 0x00ff);

                        var newAddressLow = (ushort)bus.Read(address);
                        var newAddressHigh = (ushort)(bus.Read((ushort)((address + 1) & 0x00ff)) << 8);

                        var newAddress = (ushort)(newAddressLow | newAddressHigh);

                        return (newAddress, 0);
                    }

                case AddressingMode.IndirectZeroPageWithYOffset:
                    {
                        var indirectAddress = (ushort)bus.Read(cpuState.PC++);

                        var addressLow = (ushort)bus.Read((ushort)(indirectAddress & 0x00ff));
                        var addressHigh = (ushort)(bus.Read((ushort)((indirectAddress + 1) & 0x00ff)) << 8);

                        var address = (ushort)(addressLow | addressHigh);
                        var finalAddress = (ushort)(address + cpuState.Y);

                        var additionalCycles = IsDifferentPage(address, finalAddress) ? 1 : 0;

                        return (finalAddress, additionalCycles);
                    }

                case AddressingMode.Relative:
                    {
                        var data = bus.Read(cpuState.PC++);

                        var address = (ushort)((short)cpuState.PC + (sbyte)data);

                        return (address, 0);
                    }

                default:
                    throw new InvalidAddressingModeException();
            }
        }

        public static (byte Data, ushort Address, int AdditionalCycles) FetchData(AddressingMode addressingMode, IBus bus, CpuState cpuState)
        {
            switch (addressingMode)
            {
                case AddressingMode.Implied:
                case AddressingMode.Indirect:
                case AddressingMode.Relative:
                    {
                        throw new InvalidOperationException("Cannot fetch data when addressing mode is Implied, Indirect, or Relative.");
                    }

                case AddressingMode.Accumulator:
                    {
                        return (cpuState.A, 0, 0);
                    }

                case AddressingMode.Immediate:
                    {
                        var data = bus.Read(cpuState.PC++);

                        return (data, 0, 0);
                    }

                case AddressingMode.ZeroPage:
                case AddressingMode.ZeroPageWithXOffset:
                case AddressingMode.ZeroPageWithYOffset:
                case AddressingMode.Absolute:
                case AddressingMode.AbsoluteWithXOffset:
                case AddressingMode.AbsoluteWithYOffset:
                case AddressingMode.IndirectZeroPageWithXOffset:
                case AddressingMode.IndirectZeroPageWithYOffset:
                    {
                        var (address, additionalCycles) = GetAddress(addressingMode, bus, cpuState);
                        var data = bus.Read(address);

                        return (data, address, additionalCycles);
                    }

                default:
                    throw new InvalidAddressingModeException();
            }
        }

        public static void Push(IBus bus, CpuState cpuState, byte data)
        {
            bus.Write((ushort)(0x0100 + cpuState.S), data);
            cpuState.S--;
        }

        public static byte Pop(IBus bus, CpuState cpuState)
        {
            cpuState.S++;
            return bus.Read((ushort)(0x0100 + cpuState.S));
        }

        private static bool IsDifferentPage(ushort address1, ushort address2) => (address1 & 0xff00) != (address2 & 0xff00);

        private static int PerformConditionalJump(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState, bool condition)
        {
            var (address, additionalCycles) = GetAddress(addressingMode, bus, cpuState);

            if (condition)
            {
                var oldPc = cpuState.PC;
                cpuState.PC = address;

                // If the new PC is on a different page, add an additional cycle penalty.
                if (IsDifferentPage(oldPc, address))
                {
                    additionalCycles++;
                }

                return baseCycles + additionalCycles + 1;
            }

            return baseCycles + additionalCycles;
        }

        public static int Adc(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            var (data, _, additionalCycles) = FetchData(addressingMode, bus, cpuState);

            var result = cpuState.A + data + (cpuState.GetFlag(CpuFlags.C) ? 1 : 0);
            var resultByte = (byte)(result & 0xff);

            cpuState.SetFlag(CpuFlags.C, result > 0xff);
            cpuState.SetZeroFlag(resultByte);
            cpuState.SetFlag(CpuFlags.V, ((cpuState.A ^ data) & 0x80) == 0 && ((cpuState.A ^ result) & 0x80) != 0);
            cpuState.SetNegativeFlag(resultByte);

            cpuState.A = resultByte;

            return baseCycles + additionalCycles;
        }

        public static int And(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            var (data, _, additionalCycles) = FetchData(addressingMode, bus, cpuState);

            cpuState.A = (byte)(cpuState.A & data);

            cpuState.SetZeroFlag(cpuState.A);
            cpuState.SetNegativeFlag(cpuState.A);

            return baseCycles + additionalCycles;
        }

        public static int Asl(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            // Note: This instruction operators on and stores the result to either memory or accumulator.
            // Note: This instruction always takes a constant amount of cycles to complete.

            var (data, address, _) = FetchData(addressingMode, bus, cpuState);

            cpuState.SetFlag(CpuFlags.C, (data & 0x80) != 0); // Carry flag is set to the bit that is being shifted out.

            data = (byte)(data << 1);

            cpuState.SetZeroFlag(data);
            cpuState.SetNegativeFlag(data);

            if (addressingMode == AddressingMode.Accumulator)
            {
                cpuState.A = data;
            }
            else
            {
                bus.Write(address, data);
            }

            return baseCycles;
        }

        public static int Bcc(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            return PerformConditionalJump(addressingMode, baseCycles, bus, cpuState, !cpuState.GetFlag(CpuFlags.C));
        }

        public static int Bcs(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            return PerformConditionalJump(addressingMode, baseCycles, bus, cpuState, cpuState.GetFlag(CpuFlags.C));
        }

        public static int Beq(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            return PerformConditionalJump(addressingMode, baseCycles, bus, cpuState, cpuState.GetFlag(CpuFlags.Z));
        }

        public static int Bit(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            var (data, _, _) = FetchData(addressingMode, bus, cpuState);

            cpuState.SetZeroFlag((byte)(data & cpuState.A));
            cpuState.SetFlag(CpuFlags.V, (data & 0x40) != 0); // Set overflow flag to bit 6 of data
            cpuState.SetNegativeFlag(data);

            return baseCycles;
        }

        public static int Bmi(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            return PerformConditionalJump(addressingMode, baseCycles, bus, cpuState, cpuState.GetFlag(CpuFlags.N));
        }

        public static int Bne(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            return PerformConditionalJump(addressingMode, baseCycles, bus, cpuState, !cpuState.GetFlag(CpuFlags.Z));
        }

        public static int Bpl(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            return PerformConditionalJump(addressingMode, baseCycles, bus, cpuState, !cpuState.GetFlag(CpuFlags.N));
        }

        public static int Brk(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            cpuState.PC++;

            var pcHigh = (byte)((uint)cpuState.PC >> 8);
            var pcLow = (byte)(cpuState.PC & 0x00ff);

            Push(bus, cpuState, pcHigh);
            Push(bus, cpuState, pcLow);

            cpuState.SetFlag(CpuFlags.B, true);

            Push(bus, cpuState, (byte)cpuState.Flags);

            cpuState.SetFlag(CpuFlags.I, true);

            var addressLow = (ushort)bus.Read(0xfffe);
            var addressHigh = (ushort)(bus.Read(0xffff) << 8);

            var address = (ushort)(addressLow | addressHigh);

            cpuState.PC = address;

            return baseCycles;
        }

        public static int Bvc(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            return PerformConditionalJump(addressingMode, baseCycles, bus, cpuState, !cpuState.GetFlag(CpuFlags.V));
        }

        public static int Bvs(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            return PerformConditionalJump(addressingMode, baseCycles, bus, cpuState, cpuState.GetFlag(CpuFlags.V));
        }

        public static int Clc(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            cpuState.SetFlag(CpuFlags.C, false);
            return baseCycles;
        }

        public static int Cld(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            cpuState.SetFlag(CpuFlags.D, false);
            return baseCycles;
        }

        public static int Cli(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            cpuState.SetFlag(CpuFlags.I, false);
            return baseCycles;
        }

        public static int Clv(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            cpuState.SetFlag(CpuFlags.V, false);
            return baseCycles;
        }

        public static int Cmp(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            var (data, _, additionalCycles) = FetchData(addressingMode, bus, cpuState);

            var result = (ushort)(cpuState.A - data);

            cpuState.SetFlag(CpuFlags.C, cpuState.A >= data);
            cpuState.SetZeroFlag(result);
            cpuState.SetNegativeFlag(result);

            return baseCycles + additionalCycles;
        }

        public static int Cpx(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            var (data, _, additionalCycles) = FetchData(addressingMode, bus, cpuState);

            var result = (ushort)(cpuState.X - data);

            cpuState.SetFlag(CpuFlags.C, cpuState.X >= data);
            cpuState.SetZeroFlag(result);
            cpuState.SetNegativeFlag(result);

            return baseCycles + additionalCycles;
        }

        public static int Cpy(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            var (data, _, additionalCycles) = FetchData(addressingMode, bus, cpuState);

            var result = (ushort)(cpuState.Y - data);

            cpuState.SetFlag(CpuFlags.C, cpuState.Y >= data);
            cpuState.SetZeroFlag(result);
            cpuState.SetNegativeFlag(result);

            return baseCycles + additionalCycles;
        }

        public static int Dec(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            // No addressing mode of this instruction incurs additional addressing penalties. All cycles are
            // counted in the base cycles of this instruction.
            var (address, _) = GetAddress(addressingMode, bus, cpuState);

            var data = bus.Read(address);

            data--;

            bus.Write(address, data);

            cpuState.SetZeroFlag(data);
            cpuState.SetNegativeFlag(data);

            return baseCycles;
        }

        public static int Dex(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            cpuState.X--;

            cpuState.SetZeroFlag(cpuState.X);
            cpuState.SetNegativeFlag(cpuState.X);

            return baseCycles;
        }

        public static int Dey(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            cpuState.Y--;

            cpuState.SetZeroFlag(cpuState.Y);
            cpuState.SetNegativeFlag(cpuState.Y);

            return baseCycles;
        }

        // Undocumented Instruction
        public static int Dop(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            FetchData(addressingMode, bus, cpuState);

            return baseCycles;
        }

        public static int Eor(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            var (data, _, additionalCycles) = FetchData(addressingMode, bus, cpuState);

            cpuState.A = (byte)(cpuState.A ^ data);

            cpuState.SetZeroFlag(cpuState.A);
            cpuState.SetNegativeFlag(cpuState.A);

            return baseCycles + additionalCycles;
        }

        public static int Inc(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            // No addressing mode of this instruction incurs additional addressing penalties. All cycles are
            // counted in the base cycles of this instruction.
            var (address, _) = GetAddress(addressingMode, bus, cpuState);

            var data = bus.Read(address);

            data++;

            bus.Write(address, data);

            cpuState.SetZeroFlag(data);
            cpuState.SetNegativeFlag(data);

            return baseCycles;
        }

        public static int Inx(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            cpuState.X++;

            cpuState.SetZeroFlag(cpuState.X);
            cpuState.SetNegativeFlag(cpuState.X);

            return baseCycles;
        }

        public static int Iny(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            cpuState.Y++;

            cpuState.SetZeroFlag(cpuState.Y);
            cpuState.SetNegativeFlag(cpuState.Y);

            return baseCycles;
        }

        public static int Jmp(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            var (address, _) = GetAddress(addressingMode, bus, cpuState);

            cpuState.PC = address;

            return baseCycles;
        }

        public static int Jsr(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            var (address, _) = GetAddress(addressingMode, bus, cpuState);

            cpuState.PC--;

            var pcHigh = (byte)((uint)cpuState.PC >> 8);
            var pcLow = (byte)(cpuState.PC & 0x00ff);

            Push(bus, cpuState, pcHigh);
            Push(bus, cpuState, pcLow);

            cpuState.PC = address;

            return baseCycles;
        }

        // Undocumented Instruction
        public static int Lax(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            var (data, _, additionalCycles) = FetchData(addressingMode, bus, cpuState);

            cpuState.A = data;
            cpuState.X = data;

            cpuState.SetZeroFlag(cpuState.A);
            cpuState.SetNegativeFlag(cpuState.A);

            return baseCycles + additionalCycles;
        }

        public static int Lda(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            var (data, _, additionalCycles) = FetchData(addressingMode, bus, cpuState);

            cpuState.A = data;

            cpuState.SetZeroFlag(cpuState.A);
            cpuState.SetNegativeFlag(cpuState.A);

            return baseCycles + additionalCycles;
        }

        public static int Ldx(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            var (data, _, additionalCycles) = FetchData(addressingMode, bus, cpuState);

            cpuState.X = data;

            cpuState.SetZeroFlag(cpuState.X);
            cpuState.SetNegativeFlag(cpuState.X);

            return baseCycles + additionalCycles;
        }

        public static int Ldy(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            var (data, _, additionalCycles) = FetchData(addressingMode, bus, cpuState);

            cpuState.Y = data;

            cpuState.SetZeroFlag(cpuState.Y);
            cpuState.SetNegativeFlag(cpuState.Y);

            return baseCycles + additionalCycles;
        }

        public static int Lsr(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            // Note: This instruction operators on and stores the result to either memory or accumulator.
            // Note: This instruction always takes a constant amount of cycles to complete.

            var (data, address, _) = FetchData(addressingMode, bus, cpuState);

            cpuState.SetFlag(CpuFlags.C, (data & 0x01) != 0); // Carry flag is set to the bit that is being shifted out.

            data = (byte)(data >> 1);

            cpuState.SetZeroFlag(data);
            cpuState.SetNegativeFlag(data);

            if (addressingMode == AddressingMode.Accumulator)
            {
                cpuState.A = data;
            }
            else
            {
                bus.Write(address, data);
            }

            return baseCycles;
        }

        public static int Nop(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            return baseCycles;
        }

        public static int Ora(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            var (data, _, additionalCycles) = FetchData(addressingMode, bus, cpuState);

            cpuState.A = (byte)(cpuState.A | data);

            cpuState.SetZeroFlag(cpuState.A);
            cpuState.SetNegativeFlag(cpuState.A);

            return baseCycles + additionalCycles;
        }

        public static int Pha(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            Push(bus, cpuState, cpuState.A);

            return baseCycles;
        }

        public static int Php(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            Push(bus, cpuState, (byte)(cpuState.Flags | CpuFlags.B));

            return baseCycles;
        }

        public static int Pla(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            cpuState.A = Pop(bus, cpuState);

            cpuState.SetZeroFlag(cpuState.A);
            cpuState.SetNegativeFlag(cpuState.A);

            return baseCycles;
        }

        public static int Plp(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            cpuState.Flags = (CpuFlags)Pop(bus, cpuState);

            cpuState.SetFlag(CpuFlags.U, true);
            cpuState.SetFlag(CpuFlags.B, false);

            return baseCycles;
        }

        public static int Rol(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            // Note: This instruction operators on and stores the result to either memory or accumulator.
            // Note: This instruction always takes a constant amount of cycles to complete.

            var (data, address, _) = FetchData(addressingMode, bus, cpuState);

            var newCarry = (data & 0x80) != 0;

            data = (byte)(data << 1);

            if (cpuState.GetFlag(CpuFlags.C))
            {
                data |= 0x01;
            }

            cpuState.SetFlag(CpuFlags.C, newCarry);
            cpuState.SetZeroFlag(data);
            cpuState.SetNegativeFlag(data);

            if (addressingMode == AddressingMode.Accumulator)
            {
                cpuState.A = data;
            }
            else
            {
                bus.Write(address, data);
            }

            return baseCycles;
        }

        public static int Ror(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            // Note: This instruction operators on and stores the result to either memory or accumulator.
            // Note: This instruction always takes a constant amount of cycles to complete.

            var (data, address, _) = FetchData(addressingMode, bus, cpuState);

            var newCarry = (data & 0x01) != 0;

            data = (byte)(data >> 1);

            if (cpuState.GetFlag(CpuFlags.C))
            {
                data |= 0x80;
            }

            cpuState.SetFlag(CpuFlags.C, newCarry);
            cpuState.SetZeroFlag(data);
            cpuState.SetNegativeFlag(data);

            if (addressingMode == AddressingMode.Accumulator)
            {
                cpuState.A = data;
            }
            else
            {
                bus.Write(address, data);
            }

            return baseCycles;
        }

        public static int Rti(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            cpuState.Flags = (CpuFlags)Pop(bus, cpuState);

            cpuState.SetFlag(CpuFlags.U, true);
            cpuState.SetFlag(CpuFlags.B, false);

            var pcLow = (ushort)Pop(bus, cpuState);
            var pcHigh = (ushort)(Pop(bus, cpuState) << 8);

            cpuState.PC = (ushort)(pcLow | pcHigh);

            return baseCycles;
        }

        public static int Rts(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            var pcLow = (ushort)Pop(bus, cpuState);
            var pcHigh = (ushort)(Pop(bus, cpuState) << 8);

            cpuState.PC = (ushort)(pcLow | pcHigh);
            cpuState.PC++;

            return baseCycles;
        }

        public static int Sbc(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            var (data, _, additionalCycles) = FetchData(addressingMode, bus, cpuState);

            var result = cpuState.A - data - (cpuState.GetFlag(CpuFlags.C) ? 0 : 1);
            var resultByte = (byte)(result & 0xff);

            cpuState.SetFlag(CpuFlags.C, (ushort)result < 0x100);
            cpuState.SetZeroFlag(resultByte);
            cpuState.SetFlag(CpuFlags.V, ((cpuState.A ^ data) & 0x80) != 0 && ((cpuState.A ^ result) & 0x80) != 0);
            cpuState.SetNegativeFlag(resultByte);

            cpuState.A = resultByte;

            return baseCycles + additionalCycles;
        }

        public static int Sec(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            cpuState.SetFlag(CpuFlags.C, true);
            return baseCycles;
        }

        public static int Sed(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            cpuState.SetFlag(CpuFlags.D, true);
            return baseCycles;
        }

        public static int Sei(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            cpuState.SetFlag(CpuFlags.I, true);
            return baseCycles;
        }

        public static int Sta(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            // Note: This instruction always takes a constant amount of cycles to complete.
            var (address, _) = GetAddress(addressingMode, bus, cpuState);

            bus.Write(address, cpuState.A);

            return baseCycles;
        }

        public static int Stx(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            var (address, _) = GetAddress(addressingMode, bus, cpuState);

            bus.Write(address, cpuState.X);

            return baseCycles;
        }

        public static int Sty(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            var (address, _) = GetAddress(addressingMode, bus, cpuState);

            bus.Write(address, cpuState.Y);

            return baseCycles;
        }

        public static int Tax(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            cpuState.X = cpuState.A;

            cpuState.SetZeroFlag(cpuState.X);
            cpuState.SetNegativeFlag(cpuState.X);

            return baseCycles;
        }

        public static int Tay(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            cpuState.Y = cpuState.A;

            cpuState.SetZeroFlag(cpuState.Y);
            cpuState.SetNegativeFlag(cpuState.Y);

            return baseCycles;
        }

        // Undocumented Instruction
        public static int Top(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            var (_, _, additionalCycles) = FetchData(addressingMode, bus, cpuState);

            return baseCycles + additionalCycles;
        }

        public static int Tsx(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            cpuState.X = cpuState.S;

            cpuState.SetZeroFlag(cpuState.X);
            cpuState.SetNegativeFlag(cpuState.X);

            return baseCycles;
        }

        public static int Txa(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            cpuState.A = cpuState.X;

            cpuState.SetZeroFlag(cpuState.A);
            cpuState.SetNegativeFlag(cpuState.A);

            return baseCycles;
        }

        public static int Txs(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            cpuState.S = cpuState.X;
            return baseCycles;
        }

        public static int Tya(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            cpuState.A = cpuState.Y;

            cpuState.SetZeroFlag(cpuState.A);
            cpuState.SetNegativeFlag(cpuState.A);

            return baseCycles;
        }
    }
}