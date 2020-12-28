using Ninu.Assembler.Antlr;
using System;

namespace Ninu.Assembler.Library
{
    internal abstract class BaseListener : Asm6502GrammarBaseListener
    {
        public BaseListener(AssemblerContext context)
        {
            AssemblerContext = context ?? throw new ArgumentNullException(nameof(context));
        }

        protected AssemblerContext AssemblerContext { get; }

        protected bool IsValidIdentifierName(string identifier)
        {
            switch (identifier.ToLowerInvariant())
            {
                // Registers
                case "a":
                case "x":
                case "y":
                // Opcodes
                // ...
                // Keywords
                case "origin":
                case "include":
                case "include_binary":
                case "const":
                case "hi":
                case "lo":
                    return false;

                default:
                    return true;
            }
        }
    }
}