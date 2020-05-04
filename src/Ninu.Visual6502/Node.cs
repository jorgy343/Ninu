using System.Collections.Generic;

namespace Ninu.Visual6502
{
    public class Node
    {
        public int Number { get; }
        public string? Name { get; }

        public bool State { get; set; }
        public bool PullUp { get; set; }
        public bool PullDown { get; set; }
        public bool Floating { get; set; } = true;

        public List<Transistor> Gates { get; } = new List<Transistor>();
        public List<Transistor> C1C2S { get; } = new List<Transistor>();

        public Node(int number, bool pullUp, string? name)
        {
            Number = number;
            PullUp = pullUp;
            Name = name;
        }

        public override string ToString()
        {
            return $"{Number}: {State}: {Name}";
        }
    }
}