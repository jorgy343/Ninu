using Antlr4.Runtime.Misc;
using Ninu.Base;
using System;
using static Ninu.Assembler.Antlr.Asm6502GrammarParser;

namespace Ninu.Assembler.Library
{
    internal class LabelResolverListener : BaseListener
    {
        private int _position = 0;

        private bool _inMacro = false;

        public LabelResolverListener(AssemblerContext context)
            : base(context)
        {

        }

        public override void EnterDefineConstant(DefineConstantContext context)
        {
            var name = context.identifier().GetText();

            if (!IsValidIdentifierName(name))
            {
                throw new InvalidOperationException($"Invalid identifier name of {name}.");
            }

            var value = context.constantExpression().EvaluateConstantExpression(AssemblerContext);

            if (AssemblerContext.Constants.ContainsKey(name))
            {
                throw new InvalidOperationException("Cannot reassign a constant.");
            }
            else
            {
                AssemblerContext.Constants[name] = value;
            }
        }

        public override void EnterLabel([NotNull] LabelContext context)
        {
            if (_inMacro)
            {
                return;
            }

            var name = context.identifier().GetText();

            if (AssemblerContext.Labels.ContainsKey(name))
            {
                throw new InvalidOperationException($"A label with the name {name} was already found.");
            }

            AssemblerContext.Labels[name] = _position;
        }

        public override void EnterAssemblerInstructionOrigin([NotNull] AssemblerInstructionOriginContext context)
        {
            // The origin command only works on discovered labels.
            var result = context.constantExpression().EvaluateConstantExpression(AssemblerContext);

            _position = result;
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

            _position += instruction.Size;
        }
    }
}