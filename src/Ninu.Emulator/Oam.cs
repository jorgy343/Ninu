// ReSharper disable ConditionIsAlwaysTrueOrFalse
using System;

namespace Ninu.Emulator
{
    public class Oam
    {
        public Sprite8x8[] Sprites { get; } = new Sprite8x8[64];

        public Oam()
        {
            for (var i = 0; i < 64; i++)
            {
                Sprites[i] = new Sprite8x8(0, 0, 0, 0);
            }
        }

        public byte CpuRead(byte address)
        {
            var sprite = Sprites[address / 4];

            return (address % 4) switch
            {
                0 => sprite.Y,
                1 => sprite.TileIndex,
                2 => sprite.Attributes,
                3 => sprite.X,
                _ => throw new InvalidOperationException(), // This isn't possible.
            };
        }

        public void CpuWrite(byte address, byte data)
        {
            var sprite = Sprites[address / 4];

            switch (address % 4)
            {
                case 0:
                    sprite.Y = data;
                    break;

                case 1:
                    sprite.TileIndex = data;
                    break;

                case 2:
                    sprite.Attributes = data;
                    break;

                case 3:
                    sprite.X = data;
                    break;

                default:
                    throw new InvalidOperationException(); // This isn't possible.
            }
        }
    }
}