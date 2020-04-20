using System;

namespace Ninu.Emulator
{
    public class Oam : IPersistable
    {
        public Sprite8x8[] Sprites { get; private set; }

        public Oam(int spriteCount)
        {
            if (spriteCount < 0) throw new ArgumentOutOfRangeException(nameof(spriteCount));

            Sprites = new Sprite8x8[spriteCount];

            for (var i = 0; i < spriteCount; i++)
            {
                Sprites[i] = new Sprite8x8(0, 0, 0, 0);
            }
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

        public void Write(byte address, byte data)
        {
            var sprite = Sprites[address / 4];

            switch (address % 4)
            {
                case 0:
                    sprite.Y = (byte)(data + 1); // TODO: Adding one here is kind of a hack way of getting everything to work for right now.
                    break;

                case 1:
                    sprite.TileIndex = data;
                    break;

                case 2:
                    sprite.Attributes = data;
                    break;

                case 3:
                    sprite.X = (byte)(data + 1); // TODO: This seems to fix an issue where sprites appear to be shifted one to the left?
                    break;

                default:
                    throw new InvalidOperationException(); // This isn't possible.
            }
        }

        public void SaveState(SaveStateContext context)
        {
            context.AddToState("Oam.Sprites", Sprites);
        }

        public void LoadState(SaveStateContext context)
        {
            Sprites = context.GetFromState<Sprite8x8[]>("Oam.Sprites");
        }
    }
}