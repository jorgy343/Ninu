using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using Ninu.Assembler.Antlr;

namespace Ninu.Assembler.Library
{
    public class Compiler
    {
        public void Assemble()
        {
            var input = @"
                const orig = $32 * 4
                origin(orig)

                a:
                and b:lo
                ";

            var lexer = new Asm6502Lexer(new AntlrInputStream(input));
            var parser = new Asm6502Parser(new CommonTokenStream(lexer));

            var context = parser.prog();

            // Walk it and attach our listener
            var walker = new ParseTreeWalker();
            var listener = new Listener();

            walker.Walk(listener, context);
        }
    }
}