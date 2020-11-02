using System;
using System.ComponentModel;
using static Ninu.Emulator.CentralProcessor.CpuInstructionOperations;

namespace Ninu.Emulator.CentralProcessor
{
    public delegate int InstructionExecutor(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState);

    public delegate int InstructionExecutorEx(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState, byte? dataIn, out byte result);

    public sealed class CpuInstruction
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
        /// The function that performs the instructions operation upon execution.
        /// </summary>
        private readonly InstructionExecutor? _instructionExecutor;

        /// <summary>
        /// The function that performs the instructions operation upon execution.
        /// </summary>
        private readonly InstructionExecutorEx? _instructionExecutorEx;

        /// <summary>
        /// An array of all 256 possible instructions. The position in the array represents the instructions opcode.
        /// For example, the opcode 0x00 is the BRK instruction. The opcode 0x60 is the BVC instruction. A lot of
        /// opcodes are not officially defined, but are still accounted for and often implemented.
        /// </summary>
        private static readonly CpuInstruction[] Instructions =
        {
            new CpuInstruction("brk", 0x00, 1, 7, AddressingMode.Implied, Brk),
            new CpuInstruction("ora", 0x01, 2, 6, AddressingMode.IndirectZeroPageWithXOffset, Ora),
            new CpuInstruction("kil", 0x02, 1, 2, AddressingMode.Implied, Kil),
            new CpuInstruction("slo", 0x03, 2, 8, AddressingMode.IndirectZeroPageWithXOffset, Slo),
            new CpuInstruction("dop", 0x04, 2, 3, AddressingMode.ZeroPage, Dop),
            new CpuInstruction("ora", 0x05, 2, 3, AddressingMode.ZeroPage, Ora),
            new CpuInstruction("asl", 0x06, 2, 5, AddressingMode.ZeroPage, Asl),
            new CpuInstruction("slo", 0x07, 2, 5, AddressingMode.ZeroPage, Slo),
            new CpuInstruction("php", 0x08, 1, 3, AddressingMode.Implied, Php),
            new CpuInstruction("ora", 0x09, 2, 2, AddressingMode.Immediate, Ora),
            new CpuInstruction("asl", 0x0a, 1, 2, AddressingMode.Accumulator, Asl),
            new CpuInstruction("???", 0x0b, 1, 2, AddressingMode.Implied, Nop),
            new CpuInstruction("top", 0x0c, 3, 4, AddressingMode.Absolute, Top),
            new CpuInstruction("ora", 0x0d, 3, 4, AddressingMode.Absolute, Ora),
            new CpuInstruction("asl", 0x0e, 3, 6, AddressingMode.Absolute, Asl),
            new CpuInstruction("slo", 0x0f, 3, 6, AddressingMode.Absolute, Slo),
            new CpuInstruction("bpl", 0x10, 2, 2, AddressingMode.Relative, Bpl),
            new CpuInstruction("ora", 0x11, 2, 5, AddressingMode.IndirectZeroPageWithYOffset, Ora),
            new CpuInstruction("kil", 0x12, 1, 2, AddressingMode.Implied, Kil),
            new CpuInstruction("slo", 0x13, 2, 8, AddressingMode.IndirectZeroPageWithYOffset, Slo),
            new CpuInstruction("dop", 0x14, 2, 4, AddressingMode.ZeroPageWithXOffset, Dop),
            new CpuInstruction("ora", 0x15, 2, 4, AddressingMode.ZeroPageWithXOffset, Ora),
            new CpuInstruction("asl", 0x16, 2, 6, AddressingMode.ZeroPageWithXOffset, Asl),
            new CpuInstruction("slo", 0x17, 2, 6, AddressingMode.ZeroPageWithXOffset, Slo),
            new CpuInstruction("clc", 0x18, 1, 2, AddressingMode.Implied, Clc),
            new CpuInstruction("ora", 0x19, 3, 4, AddressingMode.AbsoluteWithYOffset, Ora),
            new CpuInstruction("nop", 0x1a, 1, 2, AddressingMode.Implied, Nop),
            new CpuInstruction("slo", 0x1b, 3, 7, AddressingMode.AbsoluteWithYOffset, Slo),
            new CpuInstruction("top", 0x1c, 3, 4, AddressingMode.AbsoluteWithXOffset, Top),
            new CpuInstruction("ora", 0x1d, 3, 4, AddressingMode.AbsoluteWithXOffset, Ora),
            new CpuInstruction("asl", 0x1e, 3, 7, AddressingMode.AbsoluteWithXOffset, Asl),
            new CpuInstruction("slo", 0x1f, 3, 7, AddressingMode.AbsoluteWithXOffset, Slo),
            new CpuInstruction("jsr", 0x20, 3, 6, AddressingMode.Absolute, Jsr),
            new CpuInstruction("and", 0x21, 2, 6, AddressingMode.IndirectZeroPageWithXOffset, And),
            new CpuInstruction("kil", 0x22, 1, 2, AddressingMode.Implied, Kil),
            new CpuInstruction("rla", 0x23, 2, 8, AddressingMode.IndirectZeroPageWithXOffset, Rla),
            new CpuInstruction("bit", 0x24, 2, 3, AddressingMode.ZeroPage, Bit),
            new CpuInstruction("and", 0x25, 2, 3, AddressingMode.ZeroPage, And),
            new CpuInstruction("rol", 0x26, 2, 5, AddressingMode.ZeroPage, Rol),
            new CpuInstruction("rla", 0x27, 2, 5, AddressingMode.ZeroPage, Rla),
            new CpuInstruction("plp", 0x28, 1, 4, AddressingMode.Implied, Plp),
            new CpuInstruction("and", 0x29, 2, 2, AddressingMode.Immediate, And),
            new CpuInstruction("rol", 0x2a, 1, 2, AddressingMode.Accumulator, Rol),
            new CpuInstruction("???", 0x2b, 1, 2, AddressingMode.Implied, Nop),
            new CpuInstruction("bit", 0x2c, 3, 4, AddressingMode.Absolute, Bit),
            new CpuInstruction("and", 0x2d, 3, 4, AddressingMode.Absolute, And),
            new CpuInstruction("rol", 0x2e, 3, 6, AddressingMode.Absolute, Rol),
            new CpuInstruction("rla", 0x2f, 3, 6, AddressingMode.Absolute, Rla),
            new CpuInstruction("bmi", 0x30, 2, 2, AddressingMode.Relative, Bmi),
            new CpuInstruction("and", 0x31, 2, 5, AddressingMode.IndirectZeroPageWithYOffset, And),
            new CpuInstruction("kil", 0x32, 1, 2, AddressingMode.Implied, Kil),
            new CpuInstruction("rla", 0x33, 2, 8, AddressingMode.IndirectZeroPageWithYOffset, Rla),
            new CpuInstruction("dop", 0x34, 2, 4, AddressingMode.ZeroPageWithXOffset, Dop),
            new CpuInstruction("and", 0x35, 2, 4, AddressingMode.ZeroPageWithXOffset, And),
            new CpuInstruction("rol", 0x36, 2, 6, AddressingMode.ZeroPageWithXOffset, Rol),
            new CpuInstruction("rla", 0x37, 2, 6, AddressingMode.ZeroPageWithXOffset, Rla),
            new CpuInstruction("sec", 0x38, 1, 2, AddressingMode.Implied, Sec),
            new CpuInstruction("and", 0x39, 3, 4, AddressingMode.AbsoluteWithYOffset, And),
            new CpuInstruction("nop", 0x3a, 1, 2, AddressingMode.Implied, Nop),
            new CpuInstruction("rla", 0x3b, 3, 7, AddressingMode.AbsoluteWithYOffset, Rla),
            new CpuInstruction("top", 0x3c, 3, 4, AddressingMode.AbsoluteWithXOffset, Top),
            new CpuInstruction("and", 0x3d, 3, 4, AddressingMode.AbsoluteWithXOffset, And),
            new CpuInstruction("rol", 0x3e, 3, 7, AddressingMode.AbsoluteWithXOffset, Rol),
            new CpuInstruction("rla", 0x3f, 3, 7, AddressingMode.AbsoluteWithXOffset, Rla),
            new CpuInstruction("rti", 0x40, 1, 6, AddressingMode.Implied, Rti),
            new CpuInstruction("eor", 0x41, 2, 6, AddressingMode.IndirectZeroPageWithXOffset, Eor),
            new CpuInstruction("kil", 0x42, 1, 2, AddressingMode.Implied, Kil),
            new CpuInstruction("???", 0x43, 1, 2, AddressingMode.Implied, Nop),
            new CpuInstruction("dop", 0x44, 2, 3, AddressingMode.ZeroPage, Dop),
            new CpuInstruction("eor", 0x45, 2, 3, AddressingMode.ZeroPage, Eor),
            new CpuInstruction("lsr", 0x46, 2, 5, AddressingMode.ZeroPage, Lsr),
            new CpuInstruction("???", 0x47, 1, 2, AddressingMode.Implied, Nop),
            new CpuInstruction("pha", 0x48, 1, 3, AddressingMode.Implied, Pha),
            new CpuInstruction("eor", 0x49, 2, 2, AddressingMode.Immediate, Eor),
            new CpuInstruction("lsr", 0x4a, 1, 2, AddressingMode.Accumulator, Lsr),
            new CpuInstruction("???", 0x4b, 1, 2, AddressingMode.Implied, Nop),
            new CpuInstruction("jmp", 0x4c, 3, 3, AddressingMode.Absolute, Jmp),
            new CpuInstruction("eor", 0x4d, 3, 4, AddressingMode.Absolute, Eor),
            new CpuInstruction("lsr", 0x4e, 3, 6, AddressingMode.Absolute, Lsr),
            new CpuInstruction("???", 0x4f, 1, 2, AddressingMode.Implied, Nop),
            new CpuInstruction("bvc", 0x50, 2, 2, AddressingMode.Relative, Bvc),
            new CpuInstruction("eor", 0x51, 2, 5, AddressingMode.IndirectZeroPageWithYOffset, Eor),
            new CpuInstruction("kil", 0x52, 1, 2, AddressingMode.Implied, Kil),
            new CpuInstruction("???", 0x53, 1, 2, AddressingMode.Implied, Nop),
            new CpuInstruction("dop", 0x54, 2, 4, AddressingMode.ZeroPageWithXOffset, Dop),
            new CpuInstruction("eor", 0x55, 2, 4, AddressingMode.ZeroPageWithXOffset, Eor),
            new CpuInstruction("lsr", 0x56, 2, 6, AddressingMode.ZeroPageWithXOffset, Lsr),
            new CpuInstruction("???", 0x57, 1, 2, AddressingMode.Implied, Nop),
            new CpuInstruction("cli", 0x58, 1, 2, AddressingMode.Implied, Cli),
            new CpuInstruction("eor", 0x59, 3, 4, AddressingMode.AbsoluteWithYOffset, Eor),
            new CpuInstruction("nop", 0x5a, 1, 2, AddressingMode.Implied, Nop),
            new CpuInstruction("???", 0x5b, 1, 2, AddressingMode.Implied, Nop),
            new CpuInstruction("top", 0x5c, 3, 4, AddressingMode.AbsoluteWithXOffset, Top),
            new CpuInstruction("eor", 0x5d, 3, 4, AddressingMode.AbsoluteWithXOffset, Eor),
            new CpuInstruction("lsr", 0x5e, 3, 7, AddressingMode.AbsoluteWithXOffset, Lsr),
            new CpuInstruction("???", 0x5f, 1, 2, AddressingMode.Implied, Nop),
            new CpuInstruction("rts", 0x60, 1, 6, AddressingMode.Implied, Rts),
            new CpuInstruction("adc", 0x61, 2, 6, AddressingMode.IndirectZeroPageWithXOffset, Adc),
            new CpuInstruction("kil", 0x62, 1, 2, AddressingMode.Implied, Kil),
            new CpuInstruction("rra", 0x63, 2, 8, AddressingMode.IndirectZeroPageWithXOffset, Rra),
            new CpuInstruction("dop", 0x64, 2, 3, AddressingMode.ZeroPage, Dop),
            new CpuInstruction("adc", 0x65, 2, 3, AddressingMode.ZeroPage, Adc),
            new CpuInstruction("ror", 0x66, 2, 5, AddressingMode.ZeroPage, Ror),
            new CpuInstruction("rra", 0x67, 2, 5, AddressingMode.ZeroPage, Rra),
            new CpuInstruction("pla", 0x68, 1, 4, AddressingMode.Implied, Pla),
            new CpuInstruction("adc", 0x69, 2, 2, AddressingMode.Immediate, Adc),
            new CpuInstruction("ror", 0x6a, 1, 2, AddressingMode.Accumulator, Ror),
            new CpuInstruction("???", 0x6b, 1, 2, AddressingMode.Implied, Nop),
            new CpuInstruction("jmp", 0x6c, 3, 5, AddressingMode.Indirect, Jmp),
            new CpuInstruction("adc", 0x6d, 3, 4, AddressingMode.Absolute, Adc),
            new CpuInstruction("ror", 0x6e, 3, 6, AddressingMode.Absolute, Ror),
            new CpuInstruction("rra", 0x6f, 3, 6, AddressingMode.Absolute, Rra),
            new CpuInstruction("bvs", 0x70, 2, 2, AddressingMode.Relative, Bvs),
            new CpuInstruction("adc", 0x71, 2, 5, AddressingMode.IndirectZeroPageWithYOffset, Adc),
            new CpuInstruction("kil", 0x72, 1, 2, AddressingMode.Implied, Kil),
            new CpuInstruction("rra", 0x73, 2, 8, AddressingMode.IndirectZeroPageWithYOffset, Rra),
            new CpuInstruction("dop", 0x74, 2, 4, AddressingMode.ZeroPageWithXOffset, Dop),
            new CpuInstruction("adc", 0x75, 2, 4, AddressingMode.ZeroPageWithXOffset, Adc),
            new CpuInstruction("ror", 0x76, 2, 6, AddressingMode.ZeroPageWithXOffset, Ror),
            new CpuInstruction("rra", 0x77, 2, 6, AddressingMode.ZeroPageWithXOffset, Rra),
            new CpuInstruction("sei", 0x78, 1, 2, AddressingMode.Implied, Sei),
            new CpuInstruction("adc", 0x79, 3, 4, AddressingMode.AbsoluteWithYOffset, Adc),
            new CpuInstruction("nop", 0x7a, 1, 2, AddressingMode.Implied, Nop),
            new CpuInstruction("rra", 0x7b, 3, 7, AddressingMode.AbsoluteWithYOffset, Rra),
            new CpuInstruction("top", 0x7c, 3, 4, AddressingMode.AbsoluteWithXOffset, Top),
            new CpuInstruction("adc", 0x7d, 3, 4, AddressingMode.AbsoluteWithXOffset, Adc),
            new CpuInstruction("ror", 0x7e, 3, 7, AddressingMode.AbsoluteWithXOffset, Ror),
            new CpuInstruction("rra", 0x7f, 3, 7, AddressingMode.AbsoluteWithXOffset, Rra),
            new CpuInstruction("dop", 0x80, 2, 2, AddressingMode.Immediate, Dop),
            new CpuInstruction("sta", 0x81, 2, 6, AddressingMode.IndirectZeroPageWithXOffset, Sta),
            new CpuInstruction("dop", 0x82, 2, 2, AddressingMode.Immediate, Dop),
            new CpuInstruction("aax", 0x83, 2, 6, AddressingMode.IndirectZeroPageWithXOffset, Aax),
            new CpuInstruction("sty", 0x84, 2, 3, AddressingMode.ZeroPage, Sty),
            new CpuInstruction("sta", 0x85, 2, 3, AddressingMode.ZeroPage, Sta),
            new CpuInstruction("stx", 0x86, 2, 3, AddressingMode.ZeroPage, Stx),
            new CpuInstruction("aax", 0x87, 2, 3, AddressingMode.ZeroPage, Aax),
            new CpuInstruction("dey", 0x88, 1, 2, AddressingMode.Implied, Dey),
            new CpuInstruction("dop", 0x89, 2, 2, AddressingMode.Immediate, Dop),
            new CpuInstruction("txa", 0x8a, 1, 2, AddressingMode.Implied, Txa),
            new CpuInstruction("???", 0x8b, 1, 2, AddressingMode.Implied, Nop),
            new CpuInstruction("sty", 0x8c, 3, 4, AddressingMode.Absolute, Sty),
            new CpuInstruction("sta", 0x8d, 3, 4, AddressingMode.Absolute, Sta),
            new CpuInstruction("stx", 0x8e, 3, 4, AddressingMode.Absolute, Stx),
            new CpuInstruction("aax", 0x8f, 3, 4, AddressingMode.Absolute, Aax),
            new CpuInstruction("bcc", 0x90, 2, 2, AddressingMode.Relative, Bcc),
            new CpuInstruction("sta", 0x91, 2, 6, AddressingMode.IndirectZeroPageWithYOffset, Sta),
            new CpuInstruction("kil", 0x92, 1, 2, AddressingMode.Implied, Kil),
            new CpuInstruction("???", 0x93, 1, 2, AddressingMode.Implied, Nop),
            new CpuInstruction("sty", 0x94, 2, 4, AddressingMode.ZeroPageWithXOffset, Sty),
            new CpuInstruction("sta", 0x95, 2, 4, AddressingMode.ZeroPageWithXOffset, Sta),
            new CpuInstruction("stx", 0x96, 2, 4, AddressingMode.ZeroPageWithYOffset, Stx),
            new CpuInstruction("aax", 0x97, 2, 4, AddressingMode.ZeroPageWithYOffset, Aax),
            new CpuInstruction("tya", 0x98, 1, 2, AddressingMode.Implied, Tya),
            new CpuInstruction("sta", 0x99, 3, 5, AddressingMode.AbsoluteWithYOffset, Sta),
            new CpuInstruction("txs", 0x9a, 1, 2, AddressingMode.Implied, Txs),
            new CpuInstruction("???", 0x9b, 1, 2, AddressingMode.Implied, Nop),
            new CpuInstruction("???", 0x9c, 1, 2, AddressingMode.Implied, Nop),
            new CpuInstruction("sta", 0x9d, 3, 5, AddressingMode.AbsoluteWithXOffset, Sta),
            new CpuInstruction("???", 0x9e, 1, 2, AddressingMode.Implied, Nop),
            new CpuInstruction("???", 0x9f, 1, 2, AddressingMode.Implied, Nop),
            new CpuInstruction("ldy", 0xa0, 2, 2, AddressingMode.Immediate, Ldy),
            new CpuInstruction("lda", 0xa1, 2, 6, AddressingMode.IndirectZeroPageWithXOffset, Lda),
            new CpuInstruction("ldx", 0xa2, 2, 2, AddressingMode.Immediate, Ldx),
            new CpuInstruction("lax", 0xa3, 2, 6, AddressingMode.IndirectZeroPageWithXOffset, Lax),
            new CpuInstruction("ldy", 0xa4, 2, 3, AddressingMode.ZeroPage, Ldy),
            new CpuInstruction("lda", 0xa5, 2, 3, AddressingMode.ZeroPage, Lda),
            new CpuInstruction("ldx", 0xa6, 2, 3, AddressingMode.ZeroPage, Ldx),
            new CpuInstruction("lax", 0xa7, 2, 3, AddressingMode.ZeroPage, Lax),
            new CpuInstruction("tay", 0xa8, 1, 2, AddressingMode.Implied, Tay),
            new CpuInstruction("lda", 0xa9, 2, 2, AddressingMode.Immediate, Lda),
            new CpuInstruction("tax", 0xaa, 1, 2, AddressingMode.Implied, Tax),
            new CpuInstruction("???", 0xab, 1, 2, AddressingMode.Implied, Nop),
            new CpuInstruction("ldy", 0xac, 3, 4, AddressingMode.Absolute, Ldy),
            new CpuInstruction("lda", 0xad, 3, 4, AddressingMode.Absolute, Lda),
            new CpuInstruction("ldx", 0xae, 3, 4, AddressingMode.Absolute, Ldx),
            new CpuInstruction("lax", 0xaf, 3, 4, AddressingMode.Absolute, Lax),
            new CpuInstruction("bcs", 0xb0, 2, 2, AddressingMode.Relative, Bcs),
            new CpuInstruction("lda", 0xb1, 2, 5, AddressingMode.IndirectZeroPageWithYOffset, Lda),
            new CpuInstruction("kil", 0xb2, 1, 2, AddressingMode.Implied, Kil),
            new CpuInstruction("lax", 0xb3, 2, 5, AddressingMode.IndirectZeroPageWithYOffset, Lax),
            new CpuInstruction("ldy", 0xb4, 2, 4, AddressingMode.ZeroPageWithXOffset, Ldy),
            new CpuInstruction("lda", 0xb5, 2, 4, AddressingMode.ZeroPageWithXOffset, Lda),
            new CpuInstruction("ldx", 0xb6, 2, 4, AddressingMode.ZeroPageWithYOffset, Ldx),
            new CpuInstruction("lax", 0xb7, 2, 4, AddressingMode.ZeroPageWithYOffset, Lax),
            new CpuInstruction("clv", 0xb8, 1, 2, AddressingMode.Implied, Clv),
            new CpuInstruction("lda", 0xb9, 3, 4, AddressingMode.AbsoluteWithYOffset, Lda),
            new CpuInstruction("tsx", 0xba, 1, 2, AddressingMode.Implied, Tsx),
            new CpuInstruction("???", 0xbb, 1, 2, AddressingMode.Implied, Nop),
            new CpuInstruction("ldy", 0xbc, 3, 4, AddressingMode.AbsoluteWithXOffset, Ldy),
            new CpuInstruction("lda", 0xbd, 3, 4, AddressingMode.AbsoluteWithXOffset, Lda),
            new CpuInstruction("ldx", 0xbe, 3, 4, AddressingMode.AbsoluteWithYOffset, Ldx),
            new CpuInstruction("lax", 0xbf, 3, 4, AddressingMode.AbsoluteWithYOffset, Lax),
            new CpuInstruction("cpy", 0xc0, 2, 2, AddressingMode.Immediate, Cpy),
            new CpuInstruction("cmp", 0xc1, 2, 6, AddressingMode.IndirectZeroPageWithXOffset, Cmp),
            new CpuInstruction("dop", 0xc2, 2, 2, AddressingMode.Immediate, Dop),
            new CpuInstruction("???", 0xc3, 1, 2, AddressingMode.Implied, Nop),
            new CpuInstruction("cpy", 0xc4, 2, 3, AddressingMode.ZeroPage, Cpy),
            new CpuInstruction("cmp", 0xc5, 2, 3, AddressingMode.ZeroPage, Cmp),
            new CpuInstruction("dec", 0xc6, 2, 5, AddressingMode.ZeroPage, Dec),
            new CpuInstruction("???", 0xc7, 1, 2, AddressingMode.Implied, Nop),
            new CpuInstruction("iny", 0xc8, 1, 2, AddressingMode.Implied, Iny),
            new CpuInstruction("cmp", 0xc9, 2, 2, AddressingMode.Immediate, Cmp),
            new CpuInstruction("dex", 0xca, 1, 2, AddressingMode.Implied, Dex),
            new CpuInstruction("axs", 0xcb, 2, 2, AddressingMode.Immediate, Axs),
            new CpuInstruction("cpy", 0xcc, 3, 4, AddressingMode.Absolute, Cpy),
            new CpuInstruction("cmp", 0xcd, 3, 4, AddressingMode.Absolute, Cmp),
            new CpuInstruction("dec", 0xce, 3, 6, AddressingMode.Absolute, Dec),
            new CpuInstruction("???", 0xcf, 1, 2, AddressingMode.Implied, Nop),
            new CpuInstruction("bne", 0xd0, 2, 2, AddressingMode.Relative, Bne),
            new CpuInstruction("cmp", 0xd1, 2, 5, AddressingMode.IndirectZeroPageWithYOffset, Cmp),
            new CpuInstruction("kil", 0xd2, 1, 2, AddressingMode.Implied, Kil),
            new CpuInstruction("???", 0xd3, 1, 2, AddressingMode.Implied, Nop),
            new CpuInstruction("dop", 0xd4, 2, 4, AddressingMode.ZeroPageWithXOffset, Dop),
            new CpuInstruction("cmp", 0xd5, 2, 4, AddressingMode.ZeroPageWithXOffset, Cmp),
            new CpuInstruction("dec", 0xd6, 2, 6, AddressingMode.ZeroPageWithXOffset, Dec),
            new CpuInstruction("???", 0xd7, 1, 2, AddressingMode.Implied, Nop),
            new CpuInstruction("cld", 0xd8, 1, 2, AddressingMode.Implied, Cld),
            new CpuInstruction("cmp", 0xd9, 3, 4, AddressingMode.AbsoluteWithYOffset, Cmp),
            new CpuInstruction("nop", 0xda, 1, 2, AddressingMode.Implied, Nop),
            new CpuInstruction("???", 0xdb, 1, 2, AddressingMode.Implied, Nop),
            new CpuInstruction("top", 0xdc, 3, 4, AddressingMode.AbsoluteWithXOffset, Top),
            new CpuInstruction("cmp", 0xdd, 3, 4, AddressingMode.AbsoluteWithXOffset, Cmp),
            new CpuInstruction("dec", 0xde, 3, 7, AddressingMode.AbsoluteWithXOffset, Dec),
            new CpuInstruction("???", 0xdf, 1, 2, AddressingMode.Implied, Nop),
            new CpuInstruction("cpx", 0xe0, 2, 2, AddressingMode.Immediate, Cpx),
            new CpuInstruction("sbc", 0xe1, 2, 6, AddressingMode.IndirectZeroPageWithXOffset, Sbc),
            new CpuInstruction("dop", 0xe2, 2, 2, AddressingMode.Immediate, Dop),
            new CpuInstruction("???", 0xe3, 1, 2, AddressingMode.Implied, Nop),
            new CpuInstruction("cpx", 0xe4, 2, 3, AddressingMode.ZeroPage, Cpx),
            new CpuInstruction("sbc", 0xe5, 2, 3, AddressingMode.ZeroPage, Sbc),
            new CpuInstruction("inc", 0xe6, 2, 5, AddressingMode.ZeroPage, Inc),
            new CpuInstruction("???", 0xe7, 1, 2, AddressingMode.Implied, Nop),
            new CpuInstruction("inx", 0xe8, 1, 2, AddressingMode.Implied, Inx),
            new CpuInstruction("sbc", 0xe9, 2, 2, AddressingMode.Immediate, Sbc),
            new CpuInstruction("nop", 0xea, 1, 2, AddressingMode.Implied, Nop),
            new CpuInstruction("sbc", 0xeb, 2, 2, AddressingMode.Immediate, Sbc),
            new CpuInstruction("cpx", 0xec, 3, 4, AddressingMode.Absolute, Cpx),
            new CpuInstruction("sbc", 0xed, 3, 4, AddressingMode.Absolute, Sbc),
            new CpuInstruction("inc", 0xee, 3, 6, AddressingMode.Absolute, Inc),
            new CpuInstruction("???", 0xef, 1, 2, AddressingMode.Implied, Nop),
            new CpuInstruction("beq", 0xf0, 2, 2, AddressingMode.Relative, Beq),
            new CpuInstruction("sbc", 0xf1, 2, 5, AddressingMode.IndirectZeroPageWithYOffset, Sbc),
            new CpuInstruction("kil", 0xf2, 1, 2, AddressingMode.Implied, Kil),
            new CpuInstruction("???", 0xf3, 1, 2, AddressingMode.Implied, Nop),
            new CpuInstruction("dop", 0xf4, 2, 4, AddressingMode.ZeroPageWithXOffset, Dop),
            new CpuInstruction("sbc", 0xf5, 2, 4, AddressingMode.ZeroPageWithXOffset, Sbc),
            new CpuInstruction("inc", 0xf6, 2, 6, AddressingMode.ZeroPageWithXOffset, Inc),
            new CpuInstruction("???", 0xf7, 1, 2, AddressingMode.Implied, Nop),
            new CpuInstruction("sed", 0xf8, 1, 2, AddressingMode.Implied, Sed),
            new CpuInstruction("sbc", 0xf9, 3, 4, AddressingMode.AbsoluteWithYOffset, Sbc),
            new CpuInstruction("nop", 0xfa, 1, 2, AddressingMode.Implied, Nop),
            new CpuInstruction("???", 0xfb, 1, 2, AddressingMode.Implied, Nop),
            new CpuInstruction("top", 0xfc, 3, 4, AddressingMode.AbsoluteWithXOffset, Top),
            new CpuInstruction("sbc", 0xfd, 3, 4, AddressingMode.AbsoluteWithXOffset, Sbc),
            new CpuInstruction("inc", 0xfe, 3, 7, AddressingMode.AbsoluteWithXOffset, Inc),
            new CpuInstruction("???", 0xff, 1, 2, AddressingMode.Implied, Nop),
        };

        private CpuInstruction(string name, byte opCode, int size, int baseCycles, AddressingMode addressingMode, InstructionExecutor instructionExecutor)
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

        private CpuInstruction(string name, byte opCode, int size, int baseCycles, AddressingMode addressingMode, InstructionExecutorEx instructionExecutorEx)
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
            if (_instructionExecutor is not null)
            {
                return _instructionExecutor(AddressingMode, BaseCycles, bus, cpuState);
            }

            if (_instructionExecutorEx is not null)
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

        public static CpuInstruction GetInstruction(byte opCode) => Instructions[opCode];
    }
}