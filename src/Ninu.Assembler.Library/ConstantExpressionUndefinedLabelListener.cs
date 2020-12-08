using Antlr4.Runtime.Misc;
using Ninu.Assembler.Antlr;

namespace Ninu.Assembler.Library
{
    internal class ConstantExpressionUndefinedLabelListener : BaseListener
    {
        public ConstantExpressionUndefinedLabelListener(AssemblerContext context)
            : base(context)
        {

        }

        public bool HasUndefinedLabel { get; protected set; }

        public void Reset()
        {
            HasUndefinedLabel = false;
        }

        public override void EnterConstantExpressionIdentifier([NotNull] Asm6502Parser.ConstantExpressionIdentifierContext context)
        {
            var name = context.identifier().GetText();

            if (!AssemblerContext.Labels.ContainsKey(name))
            {
                HasUndefinedLabel = true;
            }
        }

        public override void EnterConstantExpressionIdentifierHiLo([NotNull] Asm6502Parser.ConstantExpressionIdentifierHiLoContext context)
        {
            var name = context.identifierHiLo().identifier().GetText();

            if (!AssemblerContext.Labels.ContainsKey(name))
            {
                HasUndefinedLabel = true;
            }
        }
    }
}