using System.Windows.Media;

namespace Ninu
{
    public static class SystemPalette
    {
        public static Color[] Colors { get; } = new Color[64];

        static SystemPalette()
        {
            Colors[0x00] = Color.FromRgb(84, 84, 84);
            Colors[0x01] = Color.FromRgb(0, 30, 116);
            Colors[0x02] = Color.FromRgb(8, 16, 144);
            Colors[0x03] = Color.FromRgb(48, 0, 136);
            Colors[0x04] = Color.FromRgb(68, 0, 100);
            Colors[0x05] = Color.FromRgb(92, 0, 48);
            Colors[0x06] = Color.FromRgb(84, 4, 0);
            Colors[0x07] = Color.FromRgb(60, 24, 0);
            Colors[0x08] = Color.FromRgb(32, 42, 0);
            Colors[0x09] = Color.FromRgb(8, 58, 0);
            Colors[0x0A] = Color.FromRgb(0, 64, 0);
            Colors[0x0B] = Color.FromRgb(0, 60, 0);
            Colors[0x0C] = Color.FromRgb(0, 50, 60);
            Colors[0x0D] = Color.FromRgb(0, 0, 0);
            Colors[0x0E] = Color.FromRgb(0, 0, 0);
            Colors[0x0F] = Color.FromRgb(0, 0, 0);

            Colors[0x10] = Color.FromRgb(152, 150, 152);
            Colors[0x11] = Color.FromRgb(8, 76, 196);
            Colors[0x12] = Color.FromRgb(48, 50, 236);
            Colors[0x13] = Color.FromRgb(92, 30, 228);
            Colors[0x14] = Color.FromRgb(136, 20, 176);
            Colors[0x15] = Color.FromRgb(160, 20, 100);
            Colors[0x16] = Color.FromRgb(152, 34, 32);
            Colors[0x17] = Color.FromRgb(120, 60, 0);
            Colors[0x18] = Color.FromRgb(84, 90, 0);
            Colors[0x19] = Color.FromRgb(40, 114, 0);
            Colors[0x1A] = Color.FromRgb(8, 124, 0);
            Colors[0x1B] = Color.FromRgb(0, 118, 40);
            Colors[0x1C] = Color.FromRgb(0, 102, 120);
            Colors[0x1D] = Color.FromRgb(0, 0, 0);
            Colors[0x1E] = Color.FromRgb(0, 0, 0);
            Colors[0x1F] = Color.FromRgb(0, 0, 0);

            Colors[0x20] = Color.FromRgb(236, 238, 236);
            Colors[0x21] = Color.FromRgb(76, 154, 236);
            Colors[0x22] = Color.FromRgb(120, 124, 236);
            Colors[0x23] = Color.FromRgb(176, 98, 236);
            Colors[0x24] = Color.FromRgb(228, 84, 236);
            Colors[0x25] = Color.FromRgb(236, 88, 180);
            Colors[0x26] = Color.FromRgb(236, 106, 100);
            Colors[0x27] = Color.FromRgb(212, 136, 32);
            Colors[0x28] = Color.FromRgb(160, 170, 0);
            Colors[0x29] = Color.FromRgb(116, 196, 0);
            Colors[0x2A] = Color.FromRgb(76, 208, 32);
            Colors[0x2B] = Color.FromRgb(56, 204, 108);
            Colors[0x2C] = Color.FromRgb(56, 180, 204);
            Colors[0x2D] = Color.FromRgb(60, 60, 60);
            Colors[0x2E] = Color.FromRgb(0, 0, 0);
            Colors[0x2F] = Color.FromRgb(0, 0, 0);

            Colors[0x30] = Color.FromRgb(236, 238, 236);
            Colors[0x31] = Color.FromRgb(168, 204, 236);
            Colors[0x32] = Color.FromRgb(188, 188, 236);
            Colors[0x33] = Color.FromRgb(212, 178, 236);
            Colors[0x34] = Color.FromRgb(236, 174, 236);
            Colors[0x35] = Color.FromRgb(236, 174, 212);
            Colors[0x36] = Color.FromRgb(236, 180, 176);
            Colors[0x37] = Color.FromRgb(228, 196, 144);
            Colors[0x38] = Color.FromRgb(204, 210, 120);
            Colors[0x39] = Color.FromRgb(180, 222, 120);
            Colors[0x3A] = Color.FromRgb(168, 226, 144);
            Colors[0x3B] = Color.FromRgb(152, 226, 180);
            Colors[0x3C] = Color.FromRgb(160, 214, 228);
            Colors[0x3D] = Color.FromRgb(160, 162, 160);
            Colors[0x3E] = Color.FromRgb(0, 0, 0);
            Colors[0x3F] = Color.FromRgb(0, 0, 0);
        }
    }
}