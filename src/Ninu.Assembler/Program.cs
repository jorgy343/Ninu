using Ninu.Assembler.Library;

namespace Ninu.Assembler
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var compiler = new Compiler();

            var asm = @"
                const value = 11101101%%%01011001
                ";

            var (data, context) = compiler.AssembleWithContext(asm);
        }
    }
}