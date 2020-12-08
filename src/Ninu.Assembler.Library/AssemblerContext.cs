using System.Collections.Generic;

namespace Ninu.Assembler.Library
{
    public class AssemblerContext
    {
        public Dictionary<string, int> Constants { get; } = new();
        public Dictionary<string, int> Labels { get; } = new();
    }
}