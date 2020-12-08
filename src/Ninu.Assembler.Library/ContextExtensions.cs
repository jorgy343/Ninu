using Antlr4.Runtime.Tree;
using Ninu.Base;
using System;
using static Ninu.Assembler.Antlr.Asm6502Parser;

namespace Ninu.Assembler.Library
{
    internal static class ContextExtensions
    {
        public static bool ContainsUndefinedLabel(this ConstantExpressionContext context, AssemblerContext assemblerContext)
        {
            var listener = new ConstantExpressionUndefinedLabelListener(assemblerContext);

            ParseTreeWalker.Default.Walk(listener, context);

            return listener.HasUndefinedLabel;
        }

        public static int EvaluateConstantExpression(this ConstantExpressionContext context, AssemblerContext assemblerContext)
        {
            return context switch
            {
                ConstantExpressionBracesContext x => EvaluateConstantExpression(x.expression, assemblerContext),
                ConstantExpressionBinaryOperatorContext x => x.operation.Text switch
                {
                    "+" => EvaluateConstantExpression(x.left, assemblerContext) + EvaluateConstantExpression(x.right, assemblerContext),
                    "-" => EvaluateConstantExpression(x.left, assemblerContext) - EvaluateConstantExpression(x.right, assemblerContext),
                    "*" => EvaluateConstantExpression(x.left, assemblerContext) * EvaluateConstantExpression(x.right, assemblerContext),
                    "/" => EvaluateConstantExpression(x.left, assemblerContext) / EvaluateConstantExpression(x.right, assemblerContext),
                    "%" => EvaluateConstantExpression(x.left, assemblerContext) % EvaluateConstantExpression(x.right, assemblerContext),
                    "**" => (int)Math.Pow(EvaluateConstantExpression(x.left, assemblerContext), EvaluateConstantExpression(x.right, assemblerContext)),
                    "<<" => EvaluateConstantExpression(x.left, assemblerContext) << EvaluateConstantExpression(x.right, assemblerContext),
                    ">>" => EvaluateConstantExpression(x.left, assemblerContext) >> EvaluateConstantExpression(x.right, assemblerContext),
                    "&" => EvaluateConstantExpression(x.left, assemblerContext) & EvaluateConstantExpression(x.right, assemblerContext),
                    "^" => EvaluateConstantExpression(x.left, assemblerContext) ^ EvaluateConstantExpression(x.right, assemblerContext),
                    "|" => EvaluateConstantExpression(x.left, assemblerContext) | EvaluateConstantExpression(x.right, assemblerContext),
                    _ => throw new InvalidOperationException("Unrecognized operator."),
                },
                ConstantExpressionNumberContext x => x.number().ParseNumber(),
                ConstantExpressionIdentifierHiLoContext x => x.identifierHiLo().ParseNumber(assemblerContext),
                ConstantExpressionIdentifierContext x => x.identifier().ParseNumber(assemblerContext),
                _ => throw new InvalidOperationException("Unrecognized constant expression."),
            };
        }

        public static int ParseNumber(this IdentifierHiLoContext context, AssemblerContext assemblerContext)
        {
            var labelName = context.identifier().GetText();

            if (assemblerContext.Labels.TryGetValue(labelName, out var label))
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
            else if (assemblerContext.Constants.ContainsKey(labelName))
            {
                throw new InvalidOperationException($"Cannot apply the hi or lo operation on a constant {labelName}.");
            }
            else
            {
                throw new InvalidOperationException($"Label {labelName} does not exist.");
            }
        }

        public static int ParseNumber(this IdentifierContext context, AssemblerContext assemblerContext)
        {
            var name = context.GetText();

            if (assemblerContext.Constants.TryGetValue(name, out var constant))
            {
                return constant;
            }
            else if (assemblerContext.Labels.TryGetValue(name, out var label))
            {
                return label;
            }
            else
            {
                throw new InvalidOperationException($"A constant or label {name} does not exist.");
            }
        }

        public static int ParseNumber(this NumberContext context)
        {
            if (context.HEX_NUMBER() != null)
            {
                return Convert.ToInt32(context.GetText()[1..], 16);
            }
            else if (context.BINARY_NUMBER() != null)
            {
                return Convert.ToInt32(context.GetText()[1..], 2);
            }
            else
            {
                return int.Parse(context.GetText());
            }
        }

        public static bool IsBranchInstruction(this InstructionContext context)
        {
            switch (context.opcode().GetText().ToLowerInvariant())
            {
                case "bcc":
                case "bcs":
                case "beq":
                case "bmi":
                case "bne":
                case "bpl":
                case "bvc":
                case "bvs":
                    return true;

                default:
                    return false;
            }
        }

        public static AddressingMode GetAddressingMode(this InstructionContext context, AssemblerContext assemblerContext)
        {
            switch (context.addressMode())
            {
                case AddressModeRelativeOrAbsoluteOrZeroPageContext x:
                    if (context.IsBranchInstruction())
                    {
                        return AddressingMode.Relative;
                    }

                    if (x.constantExpression().ContainsUndefinedLabel(assemblerContext))
                    {
                        return AddressingMode.Absolute;
                    }

                    //var value = x.constantExpression().EvaluateConstantExpression(assemblerContext);
                    //return (ushort)value > 255 ? AddressingMode.Absolute : AddressingMode.ZeroPage;
                    return AddressingMode.Absolute;

                case AddressModeAbsoluteOrZeroPageWithXContext x:
                    if (x.constantExpression().ContainsUndefinedLabel(assemblerContext))
                    {
                        return AddressingMode.AbsoluteWithXOffset;
                    }

                    //value = x.constantExpression().EvaluateConstantExpression(assemblerContext);
                    //return (ushort)value > 255 ? AddressingMode.AbsoluteWithXOffset : AddressingMode.ZeroPageWithXOffset;
                    return AddressingMode.AbsoluteWithXOffset;

                case AddressModeAbsoluteOrZeroPageWithYContext x:
                    if (x.constantExpression().ContainsUndefinedLabel(assemblerContext))
                    {
                        return AddressingMode.AbsoluteWithYOffset;
                    }

                    //value = x.constantExpression().EvaluateConstantExpression(assemblerContext);
                    //return (ushort)value > 255 ? AddressingMode.AbsoluteWithYOffset : AddressingMode.ZeroPageWithYOffset;
                    return AddressingMode.AbsoluteWithYOffset;
            }

            return context.addressMode() switch
            {
                null => AddressingMode.Implied,
                AddressModeImmediateContext => AddressingMode.Immediate,
                AddressModeAccumulatorContext => AddressingMode.Accumulator,
                AddressModeZeroPageContext => AddressingMode.ZeroPage,
                AddressModeAbsoluteContext => AddressingMode.Absolute,
                AddressModeZeroPageWithXContext => AddressingMode.ZeroPageWithXOffset,
                AddressModeAbsoluteWithXContext => AddressingMode.AbsoluteWithXOffset,
                AddressModeZeroPageWithYContext => AddressingMode.ZeroPageWithYOffset,
                AddressModeAbsoluteWithYContext => AddressingMode.AbsoluteWithYOffset,
                AddressModeIndirectContext => AddressingMode.Indirect,
                AddressModeIndirectWithXContext => AddressingMode.IndirectZeroPageWithXOffset,
                AddressModeIndirectWithYContext => AddressingMode.IndirectZeroPageWithYOffset,
                _ => throw new InvalidOperationException("Unrecognized adress mode."),
            };
        }
    }
}