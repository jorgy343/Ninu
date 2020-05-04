using System;

namespace Ninu.Visual6502
{
    public class Transistor
    {
        public bool On { get; set; }

        public string Name { get; }
        public Node Gate { get; }
        public Node C1 { get; }
        public Node C2 { get; }

        public Transistor(string name, Node gate, Node c1, Node c2)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Gate = gate ?? throw new ArgumentNullException(nameof(gate));
            C1 = c1 ?? throw new ArgumentNullException(nameof(c1));
            C2 = c2 ?? throw new ArgumentNullException(nameof(c2));
        }
    }
}