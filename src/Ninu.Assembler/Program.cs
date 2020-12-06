using Ninu.Assembler.Library;

namespace Ninu.Assembler
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var compiler = new Compiler();

            compiler.Assemble();
        }
    }
}