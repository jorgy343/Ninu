using Ninu.Assembler.Antlr;
using System;
using System.Collections.Generic;
using static Ninu.Assembler.Antlr.Asm6502Parser;

namespace Ninu.Assembler.Library
{
    public class Listener : Asm6502BaseListener
    {
        private readonly Dictionary<string, int> _constants = new();
        private readonly Dictionary<string, int> _labels = new();

        public override void EnterDefineConstant(DefineConstantContext context)
        {
            var name = context.identifier().GetText();
            var value = EvaluateConstantExpression(context.constantExpression());

            if (_constants.ContainsKey(name))
            {
                // TODO: Error: Cannot reassign a constant value.
            }
            else
            {
                _constants[name] = value;
            }
        }

        public override void EnterAssemblerInstructionOrigin(AssemblerInstructionOriginContext context)
        {
            var result = EvaluateConstantExpression(context.constantExpression());
        }

        public override void EnterLabel(LabelContext context)
        {

        }

        public override void EnterInstruction(InstructionContext context)
        {

        }

        protected int EvaluateConstantExpression(ConstantExpressionContext context)
        {
            return context switch
            {
                ConstantExpressionBracesContext x => EvaluateConstantExpression(x.expression),
                ConstantExpressionOperatorContext x => x.operation.Text switch
                {
                    "+" => EvaluateConstantExpression(x.left) + EvaluateConstantExpression(x.right),
                    "-" => EvaluateConstantExpression(x.left) - EvaluateConstantExpression(x.right),
                    "*" => EvaluateConstantExpression(x.left) * EvaluateConstantExpression(x.right),
                    "/" => EvaluateConstantExpression(x.left) / EvaluateConstantExpression(x.right),
                    "%" => EvaluateConstantExpression(x.left) % EvaluateConstantExpression(x.right),
                    "^" => (int)Math.Pow(EvaluateConstantExpression(x.left), EvaluateConstantExpression(x.right)),
                    _ => throw new InvalidOperationException("Unrecognized operator."),
                },
                ConstantExpressionNumberOrIdentifierContext x => GetNumberFromNumberOrIdentifier(x.numberOrIdentifier()),
                _ => throw new InvalidOperationException("Unrecognized constant expression."),
            };
        }

        protected int GetNumberFromNumberOrIdentifier(NumberOrIdentifierContext context)
        {
            if (context.number() != null)
            {
                return ParseNumber(context.number());
            }
            else if (context.identifier() != null)
            {
                return GetNumberFromIdentifier(context.identifier());
            }
            else if (context.identifierHiLo() != null)
            {
                return GetNumberFromIdentifierHiLo(context.identifierHiLo());
            }
            else
            {
                throw new InvalidOperationException("Unexpected type of identifier.");
            }
        }

        protected int GetNumberFromIdentifierHiLo(IdentifierHiLoContext context)
        {
            var labelName = context.identifier().GetText();

            if (_labels.TryGetValue(labelName, out var label))
            {
                if (context.HI() != null)
                {
                    return (label & 0xff00) >> 16;
                }
                else
                {
                    return label & 0x00ff;
                }
            }
            else
            {
                throw new InvalidOperationException($"Label {labelName} does not exist.");
            }
        }

        protected int GetNumberFromIdentifier(IdentifierContext context)
        {
            var constantName = context.GetText();

            if (_constants.TryGetValue(constantName, out var constant))
            {
                return constant;
            }
            else
            {
                throw new InvalidOperationException($"Constant {constantName} does not exist.");
            }
        }

        protected int ParseNumber(NumberContext context)
        {
            if (context.HEX_NUMBER() != null)
            {
                return Convert.ToInt32(context.GetText()[1..], 16);
            }
            else
            {
                return int.Parse(context.GetText());
            }
        }
    }
}