using System;

namespace Ninu.Emulator
{
    public struct Color4 : IEquatable<Color4>
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

        public void Deconstruct(out byte r, out byte g, out byte b, out byte a)
        {
            r = R;
            g = G;
            b = B;
            a = A;
        }

        public bool Equals(Color4 other) => R == other.R && G == other.G && B == other.B && A == other.A;
        public override bool Equals(object? obj) => obj is Color4 other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(R, G, B, A);

        public static bool operator ==(Color4 left, Color4 right) => left.Equals(right);
        public static bool operator !=(Color4 left, Color4 right) => !left.Equals(right);

        public override string ToString() => $"{nameof(R)}: {R}, {nameof(G)}: {G}, {nameof(B)}: {B}, {nameof(A)}: {A}";
    }
}