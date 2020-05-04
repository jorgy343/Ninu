using System;

namespace Ninu.Visual6502
{
    public class TransistorDefinition
    {
        public string Name { get; }
        public int Gate { get; }
        public int C1 { get; }
        public int C2 { get; }

        public TransistorDefinition(string name, int gate, int c1, int c2)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Gate = gate;
            C1 = c1;
            C2 = c2;
        }
    }
}