namespace Ninu.Emulator
{
    public static class SystemPalette
    {
        public static Color4[] Colors { get; } = new Color4[64];

        static SystemPalette()
        {
            Colors[0x00] = new Color4(84, 84, 84);
            Colors[0x01] = new Color4(0, 30, 116);
            Colors[0x02] = new Color4(8, 16, 144);
            Colors[0x03] = new Color4(48, 0, 136);
            Colors[0x04] = new Color4(68, 0, 100);
            Colors[0x05] = new Color4(92, 0, 48);
            Colors[0x06] = new Color4(84, 4, 0);
            Colors[0x07] = new Color4(60, 24, 0);
            Colors[0x08] = new Color4(32, 42, 0);
            Colors[0x09] = new Color4(8, 58, 0);
            Colors[0x0A] = new Color4(0, 64, 0);
            Colors[0x0B] = new Color4(0, 60, 0);
            Colors[0x0C] = new Color4(0, 50, 60);
            Colors[0x0D] = new Color4(0, 0, 0);
            Colors[0x0E] = new Color4(0, 0, 0);
            Colors[0x0F] = new Color4(0, 0, 0);

            Colors[0x10] = new Color4(152, 150, 152);
            Colors[0x11] = new Color4(8, 76, 196);
            Colors[0x12] = new Color4(48, 50, 236);
            Colors[0x13] = new Color4(92, 30, 228);
            Colors[0x14] = new Color4(136, 20, 176);
            Colors[0x15] = new Color4(160, 20, 100);
            Colors[0x16] = new Color4(152, 34, 32);
            Colors[0x17] = new Color4(120, 60, 0);
            Colors[0x18] = new Color4(84, 90, 0);
            Colors[0x19] = new Color4(40, 114, 0);
            Colors[0x1A] = new Color4(8, 124, 0);
            Colors[0x1B] = new Color4(0, 118, 40);
            Colors[0x1C] = new Color4(0, 102, 120);
            Colors[0x1D] = new Color4(0, 0, 0);
            Colors[0x1E] = new Color4(0, 0, 0);
            Colors[0x1F] = new Color4(0, 0, 0);

            Colors[0x20] = new Color4(236, 238, 236);
            Colors[0x21] = new Color4(76, 154, 236);
            Colors[0x22] = new Color4(120, 124, 236);
            Colors[0x23] = new Color4(176, 98, 236);
            Colors[0x24] = new Color4(228, 84, 236);
            Colors[0x25] = new Color4(236, 88, 180);
            Colors[0x26] = new Color4(236, 106, 100);
            Colors[0x27] = new Color4(212, 136, 32);
            Colors[0x28] = new Color4(160, 170, 0);
            Colors[0x29] = new Color4(116, 196, 0);
            Colors[0x2A] = new Color4(76, 208, 32);
            Colors[0x2B] = new Color4(56, 204, 108);
            Colors[0x2C] = new Color4(56, 180, 204);
            Colors[0x2D] = new Color4(60, 60, 60);
            Colors[0x2E] = new Color4(0, 0, 0);
            Colors[0x2F] = new Color4(0, 0, 0);

            Colors[0x30] = new Color4(236, 238, 236);
            Colors[0x31] = new Color4(168, 204, 236);
            Colors[0x32] = new Color4(188, 188, 236);
            Colors[0x33] = new Color4(212, 178, 236);
            Colors[0x34] = new Color4(236, 174, 236);
            Colors[0x35] = new Color4(236, 174, 212);
            Colors[0x36] = new Color4(236, 180, 176);
            Colors[0x37] = new Color4(228, 196, 144);
            Colors[0x38] = new Color4(204, 210, 120);
            Colors[0x39] = new Color4(180, 222, 120);
            Colors[0x3A] = new Color4(168, 226, 144);
            Colors[0x3B] = new Color4(152, 226, 180);
            Colors[0x3C] = new Color4(160, 214, 228);
            Colors[0x3D] = new Color4(160, 162, 160);
            Colors[0x3E] = new Color4(0, 0, 0);
            Colors[0x3F] = new Color4(0, 0, 0);
        }
    }
}