using System;

namespace Ninu.Emulator.GraphicsProcessor
{
    public class Oam
    {
        [SaveChildren]
        public Sprite[] Sprites { get; }

        public Oam(int spriteCount)
        {
            if (spriteCount < 0) throw new ArgumentOutOfRangeException(nameof(spriteCount));

            Sprites = new Sprite[spriteCount];

            for (var i = 0; i < spriteCount; i++)
            {
                Sprites[i] = new Sprite(0, 0, 0, 0);
            }
        }

        public byte this[(byte SpriteIndex, byte SpriteDataIndex) index]
        {
            get => Read((byte)((index.SpriteIndex * 4 + index.SpriteDataIndex) % (Sprites.Length * 4)));
            set => Write((byte)((index.SpriteIndex * 4 + index.SpriteDataIndex) % (Sprites.Length * 4)), value);
        }

        public void ResetAllData(byte data)
        {
            foreach (var sprite in Sprites)
            {
                sprite.Y = data;
                sprite.TileIndex = data;
                sprite.Attributes = data;
                sprite.X = data;
            }
        }

        public byte Read(byte address)
        {
            var sprite = Sprites[address / 4]; // Integer division to round down.

            return (address % 4) switch
            {
                0 => sprite.Y,
                1 => sprite.TileIndex,
                2 => sprite.Attributes,
                3 => sprite.X,
                _ => throw new InvalidOperationException(), // This isn't possible.
            };
        }

        public void Write(byte address, byte data)
        {
            var sprite = Sprites[address / 4]; // Integer division to round down.

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