namespace Ninu.Emulator
{
    public struct Color4
    {
        public byte R { get; }
        public byte G { get; }
        public byte B { get; }
        public byte A { get; }

        public Color4(byte r, byte g, byte b, byte a)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }

        public Color4(byte r, byte g, byte b)
            : this(r, g, b, 255)
        {

        }
    }
}