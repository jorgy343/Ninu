namespace Ninu.Visual6502
{
    public class NodeDefinition
    {
        public int W { get; }
        public bool PullUp { get; }
        public string? Name { get; }

        public NodeDefinition(int w, bool pullUp, string? name)
        {
            W = w;
            PullUp = pullUp;
            Name = name;
        }
    }
}