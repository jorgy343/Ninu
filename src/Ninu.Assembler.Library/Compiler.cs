using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using Ninu.Assembler.Antlr;

namespace Ninu.Assembler.Library
{
    public class Compiler
    {
        public (byte[] Data, AssemblerContext Context) AssembleWithContext(string asm)
        {
            var lexer = new Asm6502Lexer(new AntlrInputStream(asm));
            TokenStreamRewriter
            var parser = new Asm6502GrammarParser(new CommonTokenStream(lexer));

            var errorListener = new ErrorListener();

            parser.RemoveErrorListeners();
            parser.AddErrorListener(errorListener);

            var context = parser.prog();
            var assemblerContext = new AssemblerContext();

            var labelResolverListener = new LabelResolverListener(assemblerContext);
            var assemblerListener = new AssemblerListener(assemblerContext);

            ParseTreeWalker.Default.Walk(labelResolverListener, context);
            ParseTreeWalker.Default.Walk(assemblerListener, context);

            return (assemblerListener.GetCompiledBytes(), assemblerContext);
        }
    }
}