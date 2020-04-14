// ReSharper disable ShiftExpressionRealShiftCountIsZero

using System;
using System.Runtime.CompilerServices;

namespace Ninu.Emulator
{
    public class PpuRegisters
    {
        public byte Control { get; set; }
        public byte Mask { get; set; }
        public byte Status { get; set; }

        public byte OamAddress { get; set; }

        public VRamAddressRegister CurrentAddress;
        public VRamAddressRegister TemporaryAddress;

        public byte FineX { get; set; }
        public bool WriteLatch { get; set; }

        public byte ReadBuffer { get; set; }

        public void WriteControlRegister(byte data)
        {
            // The control register will only store the top 6 bits. The bottom 2 bits
            // are stored in the temporary address register.
            Control = (byte)(data & 0b1111_1100);

            // Bits 10 and 11 of the temporary address register are set to the two
            // lowest bits of the data. These are the name table select bits.
            TemporaryAddress = (ushort)((TemporaryAddress & ~0x0c00) | ((data << 10) & 0x0c00));
        }

        public void WriteMaskRegister(byte data)
        {
            Mask = data;
        }

        public byte ReadStatusRegister()
        {
            // The bottom 5 bits come from the bottom 5 bits of the data buffer.
            var returnValue = (byte)((Status & 0xe0) | (ReadBuffer & 0x1f));

            // Reading the status register clears the vertical blank bit.
            Status = SetBit(Status, VerticalBlankStartedMask, false);

            // Reading the status register resets the write latch.
            WriteLatch = false;

            return returnValue;
        }

        public void WriteOamAddress(byte data)
        {
            OamAddress = data;
        }

        public byte ReadOamData()
        {
            // TODO: Reading during PPU rendering exposes internal OAM accesses when evaluating and loading sprites.

            // TODO: Implement OAM.

            return 0;
        }

        public void WriteOamData(byte data)
        {
            // TODO: Implement OAM.
        }

        public void WriteScroll(byte data)
        {
            if (!WriteLatch) // First write.
            {
                // The data is split into two parts. The first 3 bits are the fine X scroll and
                // the upper 5 bits are the course X scroll.
                FineX = (byte)(data & 0b000_0111);

                // Set course X in the temporary address register without touching the other bits.
                TemporaryAddress = (ushort)((TemporaryAddress & ~0x001f) | ((data >> 3) & 0x001f));

                WriteLatch = true;
            }
            else // Second write.
            {
                // The data is split into two parts again. The first 5 bits are the course Y scroll
                // and the upper 3 bits are the fine Y scroll.

                // First set the course Y scroll in the temporary address register without touching
                // the other bits. Course Y is stored as bits 3-7 in data so we only have to left
                // shift data by 2 to get bits 3-7 into positions 5-9 for the temporary address register.
                TemporaryAddress = (ushort)((TemporaryAddress & ~0x03e0) | ((data << 2) & 0x03e0));

                // Now set the fine Y scroll in the temporary address register without touching the
                // other bits. Fine Y is stored as bits 0-2 in data so we have to left shift data by
                // 12 to get bits 0-2 into positions 12-14.
                TemporaryAddress = (ushort)((TemporaryAddress & ~0x7000) | ((data << 12) & 0x7000));

                WriteLatch = false;
            }
        }

        public void WriteAddress(byte data)
        {
            if (!WriteLatch) // First write.
            {
                // Write data into the high byte position of the temporary address register. Don't
                // touch the low byte of the temporary address register.
                TemporaryAddress = (ushort)((TemporaryAddress & ~0xff00) | ((data << 8) & 0xff00));

                // We do have to set bit 15 of the temporary address register to zero since it
                // technically doesn't exist.
                TemporaryAddress &= 0x7fff; // Mask the bottom 15 bits.

                WriteLatch = true;
            }
            else // Second write.
            {
                // Write data into the low byte position of the temporary address register. Don't
                // touch the high byte of the temporary address register.
                TemporaryAddress = (TemporaryAddress & ~0x00ff) | data;

                // Set the current address register to the value of the temporary address register.
                CurrentAddress = TemporaryAddress;

                WriteLatch = false;
            }
        }

        // Rendering Methods.
        public void IncrementX()
        {
            if (CurrentAddress.CourseX == 31)
            {
                CurrentAddress.CourseX = 0;
                CurrentAddress.NameTableSelectX = (byte)~CurrentAddress.NameTableSelectX; // This simply flips the name table select X bit.
            }
            else
            {
                CurrentAddress.CourseX += 1;
            }
        }

        public void IncrementY()
        {
            if (CurrentAddress.FineY < 7)
            {
                CurrentAddress.FineY++;
            }
            else
            {
                CurrentAddress.FineY = 0;

                if (CurrentAddress.CourseY == 29)
                {
                    CurrentAddress.CourseY = 0;
                    CurrentAddress.NameTableSelectY = (byte)~CurrentAddress.NameTableSelectY; // This simply flips the name table select Y bit.
                }
                else if (CurrentAddress.CourseY == 31)
                {
                    CurrentAddress.CourseY = 0;
                }
                else
                {
                    CurrentAddress.CourseY++;
                }
            }
        }

        public void TransferX()
        {
            CurrentAddress.CourseX = TemporaryAddress.CourseX;
            CurrentAddress.NameTableSelectX = TemporaryAddress.NameTableSelectX;
        }

        public void TransferY()
        {
            CurrentAddress.CourseY = TemporaryAddress.CourseY;
            CurrentAddress.FineY = TemporaryAddress.FineY;
            CurrentAddress.NameTableSelectY = TemporaryAddress.NameTableSelectY;
        }

        public bool RenderingEnabled => RenderBackground || RenderSprites;

        // Control Register
        private const int VramAddressIncrementMask = 1 << 2;
        private const int SpritePatternTableAddressFor8X8Mask = 1 << 3;
        private const int BackgroundPatternTableAddressMask = 1 << 4;
        private const int SpriteSizeMask = 1 << 5;
        private const int PpuMasterSlaveSelectMask = 1 << 6;
        private const int GenerateVerticalBlankingIntervalNmiMask = 1 << 7;

        public bool VramAddressIncrement
        {
            get => GetBit(Control, VramAddressIncrementMask);
            set => Control = SetBit(Control, VramAddressIncrementMask, value);
        }

        public bool SpritePatternTableAddressFor8X8
        {
            get => GetBit(Control, SpritePatternTableAddressFor8X8Mask);
            set => Control = SetBit(Control, SpritePatternTableAddressFor8X8Mask, value);
        }

        public bool BackgroundPatternTableAddress
        {
            get => GetBit(Control, BackgroundPatternTableAddressMask);
            set => Control = SetBit(Control, BackgroundPatternTableAddressMask, value);
        }

        public bool SpriteSize
        {
            get => GetBit(Control, SpriteSizeMask);
            set => Control = SetBit(Control, SpriteSizeMask, value);
        }

        public bool PpuMasterSlaveSelect
        {
            get => GetBit(Control, PpuMasterSlaveSelectMask);
            set => Control = SetBit(Control, PpuMasterSlaveSelectMask, value);
        }

        public bool GenerateVerticalBlankingIntervalNmi
        {
            get => GetBit(Control, GenerateVerticalBlankingIntervalNmiMask);
            set => Control = SetBit(Control, GenerateVerticalBlankingIntervalNmiMask, value);
        }

        // Mask Register
        private const int EnableGrayscaleMask = 1 << 0;
        private const int RenderBackgroundInLeftMost8PixelsOfScreenMask = 1 << 1;
        private const int RenderSpritesInLeftMost8PixelsOfScreenMask = 1 << 2;
        private const int RenderBackgroundMask = 1 << 3;
        private const int RenderSpritesMask = 1 << 4;
        private const int EmphasizeRedMask = 1 << 5;
        private const int EmphasizeGreenMask = 1 << 6;
        private const int EmphasizeBlueMask = 1 << 7;

        public bool EnableGrayscale
        {
            get => GetBit(Mask, EnableGrayscaleMask);
            set => Mask = SetBit(Mask, EnableGrayscaleMask, value);
        }

        public bool RenderBackgroundInLeftMost8PixelsOfScreen
        {
            get => GetBit(Mask, RenderBackgroundInLeftMost8PixelsOfScreenMask);
            set => Mask = SetBit(Mask, RenderBackgroundInLeftMost8PixelsOfScreenMask, value);
        }

        public bool RenderSpritesInLeftMost8PixelsOfScreen
        {
            get => GetBit(Mask, RenderSpritesInLeftMost8PixelsOfScreenMask);
            set => Mask = SetBit(Mask, RenderSpritesInLeftMost8PixelsOfScreenMask, value);
        }

        public bool RenderBackground
        {
            get => GetBit(Mask, RenderBackgroundMask);
            set => Mask = SetBit(Mask, RenderBackgroundMask, value);
        }

        public bool RenderSprites
        {
            get => GetBit(Mask, RenderSpritesMask);
            set => Mask = SetBit(Mask, RenderSpritesMask, value);
        }

        public bool EmphasizeRed
        {
            get => GetBit(Mask, EmphasizeRedMask);
            set => Mask = SetBit(Mask, EmphasizeRedMask, value);
        }

        public bool EmphasizeGreen
        {
            get => GetBit(Mask, EmphasizeGreenMask);
            set => Mask = SetBit(Mask, EmphasizeGreenMask, value);
        }

        public bool EmphasizeBlue
        {
            get => GetBit(Mask, EmphasizeBlueMask);
            set => Mask = SetBit(Mask, EmphasizeBlueMask, value);
        }

        // Status Register
        private const int SpriteOverflowMask = 1 << 5;
        private const int Sprite0HitMask = 1 << 6;
        private const int VerticalBlankStartedMask = 1 << 7;

        public bool SpriteOverflow
        {
            get => GetBit(Status, SpriteOverflowMask);
            set => Status = SetBit(Status, SpriteOverflowMask, value);
        }

        public bool Sprite0Hit
        {
            get => GetBit(Status, Sprite0HitMask);
            set => Status = SetBit(Status, Sprite0HitMask, value);
        }

        public bool VerticalBlankStarted
        {
            get => GetBit(Status, VerticalBlankStartedMask);
            set => Status = SetBit(Status, VerticalBlankStartedMask, value);
        }

        // Helpers
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool GetBit(byte data, int mask) => (data & mask) != 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte SetBit(byte data, int mask, bool value) => (byte)(value ? data | mask : data & ~mask);
    }
}