namespace Ninu.Emulator
{
    /// <summary>
    /// Represents an index into a specific color palette. Each palette has four colors, the fourth
    /// of which is always the background color (transparent) color.
    /// </summary>
    public enum PaletteColor : byte
    {
        Color0 = 0,
        Color1 = 1,
        Color2 = 2,
        Color3 = 3,
        Transparent = Color0,
    }
}