using System;
using System.Diagnostics;

namespace Ninu.Emulator.GraphicsProcessor
{
    /// <summary>
    /// This is the state machine that handles sprite evalulation which occurs during every visible
    /// scanline including the prerender scanline. It replicates the important bits of the state
    /// machine that exists in the 2c02.
    /// </summary>
    public class SpriteEvalulationStateMachine
    {
        private readonly Oam _primaryOam;
        private readonly Oam _secondaryOam;

        [Save("State")]
        private State _state = State.ReadWriteNextSpriteYCoordinate;

        [Save("ReadData")]
        private byte _readData;

        [Save("SpriteIndex")]
        private byte _spriteIndex; // This represents n when accessing primary OAM in the form oam[n][m].

        [Save("SpriteByteIndex")]
        private byte _spriteByteIndex; // This represents m when accessing primary OAM in the form oam[n][m].

        // Represents the index into secondary OAM that the next in range sprite will be written
        // to. This starts at 0 which means the next in range sprite will be written to
        // secondaryOam[n]. This is incremented only after the tile index, attributes, and x
        // coordinates are read and written to secondary oam.
        [Save("SecondarySpriteIndex")]
        private byte _secondarySpriteIndex;

        public SpriteEvalulationStateMachine(Oam primaryOam, Oam secondaryOam)
        {
            _primaryOam = primaryOam ?? throw new ArgumentNullException(nameof(primaryOam));
            _secondaryOam = secondaryOam ?? throw new ArgumentNullException(nameof(secondaryOam));
        }

        /// <summary>
        /// Resets the state machine to it's initial state where it will be ready to read the first
        /// byte of primary OAM.
        /// </summary>
        public void Reset()
        {
            _state = State.ReadWriteNextSpriteYCoordinate;
            _readData = 0;

            _spriteIndex = 0;
            _spriteByteIndex = 0;

            _secondarySpriteIndex = 0;
        }

        /// <summary>
        /// Increments the state machine by a single pixel clock.
        /// </summary>
        /// <param name="scanline">The scanline that this tick is taking place on.</param>
        /// <param name="cycle">The cycle of the scanline that this tick is taking place on. This must be a value between 65 and 256 inclusively.</param>
        /// <returns><c>true</c> if the overflow flag needs to be set; otherwise, <c>false</c>.</returns>
        public (bool Sprite0HitPossible, bool SetOverflowFlag) Tick(int scanline, int cycle, bool spriteSizeIs8x8)
        {
            if (cycle < 65 || cycle > 256)
            {
                throw new ArgumentOutOfRangeException($"The argument for {nameof(cycle)} must be between 65 and 256 inclusively.");
            }

            // Reads are performed on odd cycles and writes are performed on even cycles. During
            // some states, the writes might be supressed.

            switch (_state)
            {
                // This is the initial state of the state machine. Initially, this stage will read
                // the first byte of primary OAM which is the sprite's y coordinate. However, this
                // state will be run at least 8 times and at most 64 times. It stops running once
                // we find 8 sprites that are within the scanline.
                case State.ReadWriteNextSpriteYCoordinate: // Step 1
                    if ((cycle & 0x01) == 1) // Odd cycle, read.
                    {
                        Debug.Assert(_spriteByteIndex == 0); // _spriteByteIndex should always be zero here.

                        _readData = _primaryOam[(_spriteIndex, _spriteByteIndex)];
                    }
                    else // Even cycle, write.
                    {
                        Debug.Assert(_spriteByteIndex == 0); // _spriteByteIndex should always be zero here.

                        // Write the y coordinate to secondary OAM even if this sprite is not in
                        // range.
                        _secondaryOam[(_secondarySpriteIndex, _spriteByteIndex)] = _readData;

                        // If this sprite is in range, copy the next three bytes of this sprite.
                        // Check for y being less than 0xef since anything in the 0xef to 0xff
                        // range is not displayed.
                        if (_readData < 0xef && _readData <= scanline && _readData + (spriteSizeIs8x8 ? 7 : 15) >= scanline)
                        {
                            _spriteByteIndex++; // The next byte read and written will be the tile index.

                            // We need read and write the next three bytes from primary oam to
                            // secondary oam.
                            _state = State.ReadWriteNextThreeBytes;

                            // If this is the first sprite in OAM, inform the caller that a sprite
                            // 0 hit is possible.
                            if (_spriteIndex == 0)
                            {
                                return (Sprite0HitPossible: true, SetOverflowFlag: false);
                            }
                        }
                        else
                        {
                            _spriteIndex = (byte)((_spriteIndex + 1) & 0x3f); // This is a 6-bit number. Wrap accordingly.

                            // If we have gone through all 64 sprites in primary OAM and the index
                            // wraped back to zero, go to step 4.
                            if (_spriteIndex == 0)
                            {
                                _state = State.Done;
                            }
                        }
                    }

                    break;

                // This runs three times after the previous state finds a sprite within the
                // scanline. It reads and writes the tile index, attributes, and x coordinate of
                // the sprite that is in range.
                case State.ReadWriteNextThreeBytes: // Step 2
                    if ((cycle & 0x01) == 1) // Odd cycle, read.
                    {
                        _readData = _primaryOam[(_spriteIndex, _spriteByteIndex)];
                    }
                    else // Even cycle, write.
                    {
                        _secondaryOam[(_secondarySpriteIndex, _spriteByteIndex)] = _readData;

                        // Read the next byte of data during the next pass through this stage.
                        _spriteByteIndex++;

                        // If we have read and written the tile index, attributes, and x
                        // coordinate; reset the sprite byte index and increment both the sprite
                        // index and the secondary sprite index. Note that we already incremented
                        // _spriteByteIndex so this variable will be 4 if we have read the three
                        // bytes.
                        if (_spriteByteIndex == 4)
                        {
                            _spriteByteIndex = 0;

                            _spriteIndex = (byte)((_spriteIndex + 1) & 0x3f); // This is a 6-bit number. Wrap accordingly.
                            _secondarySpriteIndex++;

                            // If we have gone through all 64 sprites in primary OAM and the index
                            // wraped back to zero, go to step 4.
                            if (_spriteIndex == 0)
                            {
                                _state = State.Done;
                            }
                            else
                            {
                                // If we have not yet found 8 sprites, go back to step 1 to find
                                // the next in range sprite. Note that we already incremented
                                // _secondarySpriteIndex so this variable will be 7 or less if we
                                // have not found all 8 sprites.
                                if (_secondarySpriteIndex < 8)
                                {
                                    _state = State.ReadWriteNextSpriteYCoordinate;
                                }
                                else
                                {
                                    // Otherwise, we have found 8 sprites. Find out if we have a
                                    // sprite overflow.
                                    _state = State.FindSpriteOverflow;
                                }
                            }
                        }
                    }

                    break;

                case State.FindSpriteOverflow: // step 3.
                    if ((cycle & 0x01) == 1) // Odd cycle, read.
                    {
                        // During the sprite overflow phase, _spriteByteIndex is incremented in a
                        // buggy way just as it was in the original 2c02. Without the bug, this
                        // would always read the y coordinate of the sprite in primary OAM. But
                        // with the bug, it may read the tile index, attributes, or x coordinate
                        // and interpret that as the y coordinate depending on the value of
                        // _spriteByteIndex.
                        _readData = _primaryOam[(_spriteIndex, _spriteByteIndex)];
                    }
                    else // Even cycle, write.
                    {
                        // If this sprite is in range, set the overflow flag. Check for y being
                        // less than 0xef since anything in the 0xef to 0xff range is not
                        // displayed.
                        if (_readData < 0xef && _readData <= scanline && _readData + (spriteSizeIs8x8 ? 7 : 15) >= scanline)
                        {
                            // Technically the 2c02 does some incrementing of stuff but it doesn't
                            // matter since it doesn't affect secondary OAM or any other external
                            // facing memory. As such we'll just go to step 4.
                            _state = State.Done;

                            // Indicate that we need to set the overflow flag.
                            return (Sprite0HitPossible: false, SetOverflowFlag: true);
                        }
                        else
                        {
                            // Increment m and n. This is the buggy part of the hardware. m should
                            // not have been incremented.
                            _spriteIndex = (byte)((_spriteIndex + 1) & 0x7f); // This is a 7-bit number. Wrap accordingly.
                            _secondarySpriteIndex++;

                            // If m overflows, reset it back to zero.
                            if (_secondarySpriteIndex == 4)
                            {
                                _secondarySpriteIndex = 0;
                            }

                            // If we have gone through all 64 sprites in primary OAM and the index
                            // wraped back to zero, go to step 4.
                            if (_spriteIndex == 0)
                            {
                                _state = State.Done;
                            }
                        }
                    }

                    break;

                // This step just reads the y coordinate from the current sprite in primary OAM and
                // fails to write this data to secondary OAM since writes are now supressed.
                case State.Done: // Step 4
                    break;
            }

            return (Sprite0HitPossible: false, SetOverflowFlag: false);
        }

        public enum State
        {
            ReadWriteNextSpriteYCoordinate,
            ReadWriteNextThreeBytes,
            FindSpriteOverflow,
            Done,
        }
    }
}