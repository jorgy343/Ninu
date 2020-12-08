using Antlr4.Runtime;
using System;
using System.IO;

namespace Ninu.Assembler.Library
{
    public class ErrorListener : BaseErrorListener
    {
        public override void SyntaxError(
            TextWriter output,
            IRecognizer recognizer,
            IToken offendingSymbol,
            int line,
            int charPositionInLine,
            string msg,
            RecognitionException e)
        {
            throw new InvalidOperationException($"line {line}:{charPositionInLine} {msg}");
        }
    }
}