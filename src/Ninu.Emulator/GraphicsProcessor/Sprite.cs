using static Ninu.Emulator.Bits;

namespace Ninu.Emulator.GraphicsProcessor
{
    /// <summary>
    /// Represents a sprite entry in OAM memory.
    /// </summary>
    public class Sprite
    {
        /// <summary>
        /// Gets or sets the sprite's Y coordinate. When rendered, sprites are behind by one scanline and so sprites
        /// can never appear on the first scanline nor can they be partially off the screen at the top of the screen.
        /// Because of this, the actual Y value must have 1 subtracted from it when stored in this object and in
        /// memory.
        /// </summary>
        [Save]
        public byte Y { get; set; }

        /// <summary>
        /// Gets or sets the index of the tile in pattern memory. For 8x8 sprites, the name table that is selected is
        /// controlled by bit 3 in PPUCTRL.
        /// </summary>
        [Save]
        public byte TileIndex { get; set; }

        /// <summary>
        /// Gets or sets the attribute bits of the sprites. See the specific attribute bit methods on this object
        /// further details.
        /// </summary>
        [Save]
        public byte Attributes { get; set; }

        /// <summary>
        /// Gets or sets the sprite's X coordinate. This references the sprites left edge. This means that it is not
        /// possible to have a sprite partially off the left edge of the screen.
        /// </summary>
        [Save]
        public byte X { get; set; }

        public Sprite(byte byte0, byte byte1, byte byte2, byte byte3)
        {
            Y = byte0;
            TileIndex = byte1;
            Attributes = byte2;
            X = byte3;
        }

        /// <summary>
        /// Gets the palette index to use for this sprite. This is a number between 0 and 3 inclusive. Since the first
        /// four palettes are for backgrounds and the last four are for sprites, to get the actual palette index, 4
        /// must be added to the returned palette index.
        /// </summary>
        public byte PaletteIndex
        {
            get => GetBits(Attributes, 0, 2);
            set => Attributes = (byte)SetBits(Attributes, 0, 2, value);
        }

        /// <summary>
        /// Gets or sets the priority of the sprite. False puts the sprite in front of the background while true puts
        /// the sprite behind the background.
        /// </summary>
        public bool Priority
        {
            get => GetBit(Attributes, 5);
            set => Attributes = (byte)SetBit(Attributes, 5, value);
        }

        /// <summary>
        /// Gets or sets whether the sprite will be flipped horizontally. This doesn't change the position or bounding
        /// box of the sprite, it only reverses the pixels.
        /// </summary>
        public bool FlipHorizontal
        {
            get => GetBit(Attributes, 6);
            set => Attributes = (byte)SetBit(Attributes, 6, value);
        }

        /// <summary>
        /// Gets or sets whether the sprite will be flipped vertically. This doesn't change the position of the
        /// bounding box of the sprite, it only reverses the pixels.
        /// </summary>
        public bool FlipVertical
        {
            get => GetBit(Attributes, 7);
            set => Attributes = (byte)SetBit(Attributes, 7, value);
        }

        /// <summary>
        /// Copies the data from the sprite this method is being called on to <paramref name="otherSprite"/>.
        /// </summary>
        /// <param name="otherSprite">The sprite that will receive the copied data.</param>
        public void CopyTo(Sprite otherSprite)
        {
            otherSprite.Y = Y;
            otherSprite.TileIndex = TileIndex;
            otherSprite.Attributes = Attributes;
            otherSprite.X = X;
        }
    }
}