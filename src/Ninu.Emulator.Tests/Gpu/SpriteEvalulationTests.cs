using Ninu.Emulator.GraphicsProcessor;
using Xunit;

namespace Ninu.Emulator.Tests.Gpu
{
    public class SpriteEvalulationTests
    {
        private readonly Oam _primaryOam = new(64);
        private readonly Oam _secondaryOam = new(8);

        private readonly SpriteEvalulationStateMachine _spriteEvalulator;

        private readonly Sprite _defaultSprite = new(0xff, 0xff, 0xff, 0xff);

        public SpriteEvalulationTests()
        {
            _spriteEvalulator = new(_primaryOam, _secondaryOam);

            for (var i = 0; i < _primaryOam.Sprites.Length; i++)
            {
                var sprite = _primaryOam.Sprites[i];

                sprite.X = (byte)(i + 1); // So we can easily determine which sprite we are working with.
                sprite.Y = 0xff; // Make all sprites out of range by default.
            }
        }

        [Fact]
        public void NoSpritesInRange()
        {
            // Arrange
            // Do nothing since all sprites out out of range by default.

            // Act/Assert
            for (var scanline = -1; scanline <= 239; scanline++)
            {
                _secondaryOam.ResetAllData(0xff); // Secondary OAM clear happens just before evalulation.
                _spriteEvalulator.Reset();

                for (var cycle = 65; cycle <= 256; cycle++)
                {
                    var (sprite0HitPossible, setOverflowFlag) = _spriteEvalulator.Tick(scanline, cycle, true);

                    Assert.False(sprite0HitPossible);
                    Assert.False(setOverflowFlag);
                }

                AssertSprite(_defaultSprite, _secondaryOam.Sprites[0]);
                AssertSprite(_defaultSprite, _secondaryOam.Sprites[1]);
                AssertSprite(_defaultSprite, _secondaryOam.Sprites[2]);
                AssertSprite(_defaultSprite, _secondaryOam.Sprites[3]);
                AssertSprite(_defaultSprite, _secondaryOam.Sprites[4]);
                AssertSprite(_defaultSprite, _secondaryOam.Sprites[5]);
                AssertSprite(_defaultSprite, _secondaryOam.Sprites[6]);
                AssertSprite(_defaultSprite, _secondaryOam.Sprites[7]);
            }
        }

        [Fact]
        public void FirstSpriteInRange_Scanline0()
        {
            // Arrange
            var sprite0 = new Sprite(0, 0, 0, 117);

            sprite0.CopyTo(_primaryOam.Sprites[0]);

            // Act
            _secondaryOam.ResetAllData(0xff); // Secondary OAM clear happens just before evalulation.

            var sprite0HitPossibleAny = false;
            var setOverflowFlagAny = false;

            for (var cycle = 65; cycle <= 256; cycle++)
            {
                var (sprite0HitPossible, setOverflowFlag) = _spriteEvalulator.Tick(0, cycle, true);

                if (sprite0HitPossible)
                {
                    sprite0HitPossibleAny = true;
                }

                if (setOverflowFlag)
                {
                    setOverflowFlagAny = true;
                }
            }

            // Assert
            Assert.True(sprite0HitPossibleAny);
            Assert.False(setOverflowFlagAny);

            AssertSprite(sprite0, _secondaryOam.Sprites[0]);
            AssertSprite(_defaultSprite, _secondaryOam.Sprites[1]);
            AssertSprite(_defaultSprite, _secondaryOam.Sprites[2]);
            AssertSprite(_defaultSprite, _secondaryOam.Sprites[3]);
            AssertSprite(_defaultSprite, _secondaryOam.Sprites[4]);
            AssertSprite(_defaultSprite, _secondaryOam.Sprites[5]);
            AssertSprite(_defaultSprite, _secondaryOam.Sprites[6]);
            AssertSprite(_defaultSprite, _secondaryOam.Sprites[7]);
        }

        [Fact]
        public void FirstSpriteInRange_Scanline239()
        {
            // Arrange
            var sprite0 = new Sprite(239, 0, 0, 117);

            sprite0.CopyTo(_primaryOam.Sprites[0]);

            // Act
            _secondaryOam.ResetAllData(0xff); // Secondary OAM clear happens just before evalulation.

            var sprite0HitPossibleAny = false;
            var setOverflowFlagAny = false;

            for (var cycle = 65; cycle <= 256; cycle++)
            {
                var (sprite0HitPossible, setOverflowFlag) = _spriteEvalulator.Tick(239, cycle, true);

                if (sprite0HitPossible)
                {
                    sprite0HitPossibleAny = true;
                }

                if (setOverflowFlag)
                {
                    setOverflowFlagAny = true;
                }
            }

            // Assert
            Assert.True(sprite0HitPossibleAny);
            Assert.False(setOverflowFlagAny);

            AssertSprite(sprite0, _secondaryOam.Sprites[0]);
            AssertSprite(_defaultSprite, _secondaryOam.Sprites[1]);
            AssertSprite(_defaultSprite, _secondaryOam.Sprites[2]);
            AssertSprite(_defaultSprite, _secondaryOam.Sprites[3]);
            AssertSprite(_defaultSprite, _secondaryOam.Sprites[4]);
            AssertSprite(_defaultSprite, _secondaryOam.Sprites[5]);
            AssertSprite(_defaultSprite, _secondaryOam.Sprites[6]);
            AssertSprite(_defaultSprite, _secondaryOam.Sprites[7]);
        }

        [Fact]
        public void SevenSpritesInRange()
        {
            // Arrange
            var spriteOutOfRange0 = new Sprite(22, 0, 0, 50);
            var spriteOutOfRange1 = new Sprite(31, 0, 0, 51);

            var sprite0 = new Sprite(24, 0, 0, 117);
            var sprite1 = new Sprite(25, 0, 0, 118);
            var sprite2 = new Sprite(26, 0, 0, 119);
            var sprite3 = new Sprite(27, 0, 0, 120);
            var sprite4 = new Sprite(28, 0, 0, 121);
            var sprite5 = new Sprite(29, 0, 0, 122);
            var sprite6 = new Sprite(30, 0, 0, 123);

            spriteOutOfRange0.CopyTo(_primaryOam.Sprites[2]);
            spriteOutOfRange1.CopyTo(_primaryOam.Sprites[3]);

            sprite0.CopyTo(_primaryOam.Sprites[11]);
            sprite1.CopyTo(_primaryOam.Sprites[12]);
            sprite2.CopyTo(_primaryOam.Sprites[13]);
            sprite3.CopyTo(_primaryOam.Sprites[14]);
            sprite4.CopyTo(_primaryOam.Sprites[15]);
            sprite5.CopyTo(_primaryOam.Sprites[16]);
            sprite6.CopyTo(_primaryOam.Sprites[17]);

            // Act
            _secondaryOam.ResetAllData(0xff); // Secondary OAM clear happens just before evalulation.

            for (var cycle = 65; cycle <= 256; cycle++)
            {
                var (sprite0HitPossible, setOverflowFlag) = _spriteEvalulator.Tick(30, cycle, true);

                Assert.False(sprite0HitPossible);
                Assert.False(setOverflowFlag);
            }

            // Assert
            AssertSprite(sprite0, _secondaryOam.Sprites[0]);
            AssertSprite(sprite1, _secondaryOam.Sprites[1]);
            AssertSprite(sprite2, _secondaryOam.Sprites[2]);
            AssertSprite(sprite3, _secondaryOam.Sprites[3]);
            AssertSprite(sprite4, _secondaryOam.Sprites[4]);
            AssertSprite(sprite5, _secondaryOam.Sprites[5]);
            AssertSprite(sprite6, _secondaryOam.Sprites[6]);
            AssertSprite(_defaultSprite, _secondaryOam.Sprites[7]);
        }

        [Fact]
        public void EightSpritesInRange()
        {
            // Arrange
            var spriteOutOfRange0 = new Sprite(22, 0, 0, 50);
            var spriteOutOfRange1 = new Sprite(31, 0, 0, 51);

            var sprite0 = new Sprite(23, 0, 0, 117);
            var sprite1 = new Sprite(24, 0, 0, 118);
            var sprite2 = new Sprite(25, 0, 0, 119);
            var sprite3 = new Sprite(26, 0, 0, 120);
            var sprite4 = new Sprite(27, 0, 0, 121);
            var sprite5 = new Sprite(28, 0, 0, 122);
            var sprite6 = new Sprite(29, 0, 0, 123);
            var sprite7 = new Sprite(30, 0, 0, 124);

            spriteOutOfRange0.CopyTo(_primaryOam.Sprites[2]);
            spriteOutOfRange1.CopyTo(_primaryOam.Sprites[3]);

            sprite0.CopyTo(_primaryOam.Sprites[11]);
            sprite1.CopyTo(_primaryOam.Sprites[12]);
            sprite2.CopyTo(_primaryOam.Sprites[13]);
            sprite3.CopyTo(_primaryOam.Sprites[14]);
            sprite4.CopyTo(_primaryOam.Sprites[15]);
            sprite5.CopyTo(_primaryOam.Sprites[16]);
            sprite6.CopyTo(_primaryOam.Sprites[17]);
            sprite7.CopyTo(_primaryOam.Sprites[18]);

            // Act
            _secondaryOam.ResetAllData(0xff); // Secondary OAM clear happens just before evalulation.

            for (var cycle = 65; cycle <= 256; cycle++)
            {
                var (sprite0HitPossible, setOverflowFlag) = _spriteEvalulator.Tick(30, cycle, true);

                Assert.False(sprite0HitPossible);
                Assert.False(setOverflowFlag);
            }

            // Assert
            AssertSprite(sprite0, _secondaryOam.Sprites[0]);
            AssertSprite(sprite1, _secondaryOam.Sprites[1]);
            AssertSprite(sprite2, _secondaryOam.Sprites[2]);
            AssertSprite(sprite3, _secondaryOam.Sprites[3]);
            AssertSprite(sprite4, _secondaryOam.Sprites[4]);
            AssertSprite(sprite5, _secondaryOam.Sprites[5]);
            AssertSprite(sprite6, _secondaryOam.Sprites[6]);
            AssertSprite(sprite7, _secondaryOam.Sprites[7]);
        }

        private void AssertSprite(Sprite expected, Sprite actual)
        {
            Assert.Equal(expected.Y, actual.Y);
            Assert.Equal(expected.TileIndex, actual.TileIndex);
            Assert.Equal(expected.Attributes, actual.Attributes);
            Assert.Equal(expected.X, actual.X);
        }
    }
}