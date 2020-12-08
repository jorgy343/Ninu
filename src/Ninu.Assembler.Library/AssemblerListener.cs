using Antlr4.Runtime.Misc;
using Ninu.Base;
using System;
using System.IO;
using static Ninu.Assembler.Antlr.Asm6502Parser;

namespace Ninu.Assembler.Library
{
    internal class AssemblerListener : BaseListener, IDisposable
    {
        private readonly MemoryStream _memory;
        private readonly BinaryWriter _writer;

        public AssemblerListener(AssemblerContext context)
            : base(context)
        {
            _memory = new();
            _writer = new(_memory);
        }

        public void Dispose()
        {
            _writer.Dispose();
            _memory.Dispose();
        }

        public byte[] GetCompiledBytes()
        {
            _writer.Flush();

            return _memory.ToArray();
        }

        public override void EnterAssemblerInstructionOrigin([NotNull] AssemblerInstructionOriginContext context)
        {
            // The origin command only works on discovered labels.
            var result = context.constantExpression().EvaluateConstantExpression(AssemblerContext);

            _memory.Seek(result, SeekOrigin.Begin);
        }

        public override void EnterInstruction([NotNull] InstructionContext context)
        {
            var name = context.opcode().GetText().ToLowerInvariant();
            var addressingMode = context.GetAddressingMode(AssemblerContext);

            var instruction = Instruction.GetByNameAndAddressingMode(name, addressingMode);

            if (instruction == null)
            {
                throw new InvalidOperationException($"An instruction with name {name} and addressing mode {addressingMode} does not exist.");
            }

            _writer.Write(instruction.OpCode);

            if (addressingMode != AddressingMode.Implied && addressingMode != AddressingMode.Accumulator)
            {
                WriteAddressingMode(context.addressMode(), addressingMode);
            }
        }

        protected void WriteAddressingMode(AddressModeContext context, AddressingMode addressingMode)
        {
            var value = context switch
            {
                AddressModeImmediateContext x => x.constantExpression().EvaluateConstantExpression(AssemblerContext),
                AddressModeZeroPageContext x => x.constantExpression().EvaluateConstantExpression(AssemblerContext),
                AddressModeAbsoluteContext x => x.constantExpression().EvaluateConstantExpression(AssemblerContext),
                AddressModeRelativeOrAbsoluteOrZeroPageContext x => x.constantExpression().EvaluateConstantExpression(AssemblerContext),
                AddressModeZeroPageWithXContext x => x.constantExpression().EvaluateConstantExpression(AssemblerContext),
                AddressModeAbsoluteWithXContext x => x.constantExpression().EvaluateConstantExpression(AssemblerContext),
                AddressModeAbsoluteOrZeroPageWithXContext x => x.constantExpression().EvaluateConstantExpression(AssemblerContext),
                AddressModeZeroPageWithYContext x => x.constantExpression().EvaluateConstantExpression(AssemblerContext),
                AddressModeAbsoluteWithYContext x => x.constantExpression().EvaluateConstantExpression(AssemblerContext),
                AddressModeAbsoluteOrZeroPageWithYContext x => x.constantExpression().EvaluateConstantExpression(AssemblerContext),
                AddressModeIndirectContext x => x.constantExpression().EvaluateConstantExpression(AssemblerContext),
                AddressModeIndirectWithXContext x => x.constantExpression().EvaluateConstantExpression(AssemblerContext),
                AddressModeIndirectWithYContext x => x.constantExpression().EvaluateConstantExpression(AssemblerContext),
                _ => 0,
            };

            switch (addressingMode)
            {
                case AddressingMode.Immediate:
                case AddressingMode.ZeroPage:
                case AddressingMode.ZeroPageWithXOffset:
                case AddressingMode.ZeroPageWithYOffset:
                case AddressingMode.IndirectZeroPageWithXOffset:
                case AddressingMode.IndirectZeroPageWithYOffset:
                case AddressingMode.Relative:
                    _writer.Write((byte)(value & 0xff));
                    break;

                case AddressingMode.Absolute:
                case AddressingMode.AbsoluteWithXOffset:
                case AddressingMode.AbsoluteWithYOffset:
                case AddressingMode.Indirect:
                    _writer.Write((ushort)(value & 0xffff));
                    break;

                default:
                    throw new InvalidOperationException("Unrecognized addressing mode.");
            }
        }
    }
}