// ReSharper disable InconsistentNaming
using static Ninu.Emulator.Bits;

namespace Ninu.Emulator
{
    public class PpuRegisters
    {
        /// <summary>
        /// The backing data for the PPUCTRL register. This is a write only register. Generally the
        /// <see cref="WriteControlRegister(byte)"/> method should be used to set the data of this register since
        /// specific logic is required to properly implement this register.
        /// </summary>
        [Save]
        public byte Control { get; set; }

        /// <summary>
        /// The backing data for the PPUMASK register. This is a write only register. Generally the
        /// <see cref="WriteMaskRegister(byte)"/> method should be used to set the data of this register for
        /// consistency.
        /// </summary>
        [Save]
        public byte Mask { get; set; }

        [Save]
        public byte Status { get; set; }

        [SaveChildren]
        public VRamAddressRegister VAddress;

        [SaveChildren]
        public VRamAddressRegister TAddress;

        [Save]
        public byte FineX { get; set; }

        [Save]
        public bool WriteLatch { get; set; }

        [Save]
        public byte ReadBuffer { get; set; }

        public void WriteControlRegister(byte data)
        {
            // The control register will only store the top 6  The bottom 2 bits are stored in the temporary
            // address register.
            Control = (byte)(data & 0b1111_1100);

            // Bits 10 and 11 of the temporary address register are set to the two lowest bits of the data. These are
            // the name table select 
            TAddress.NameTableSelect = GetBits(data, 0, 2);
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
            VerticalBlankStarted = false;

            // Reading the status register resets the write latch.
            WriteLatch = false;

            return returnValue;
        }

        public void WriteScroll(byte data)
        {
            if (!WriteLatch) // First write.
            {
                // The data is split into two parts. The first 3 bits are the fine X scroll and the upper 5 bits are
                // the course X scroll.
                FineX = (byte)(data & 0b000_0111);

                // Set course X in the temporary address register without touching the other 
                TAddress = (ushort)((TAddress & ~0x001f) | ((data >> 3) & 0x001f));

                WriteLatch = true;
            }
            else // Second write.
            {
                // The data is split into two parts again. The first 5 bits are the course Y scroll and the upper 3
                // bits are the fine Y scroll.

                // First set the course Y scroll in the temporary address register without touching the other 
                // Course Y is stored as bits 3-7 in data so we only have to left shift data by 2 to get bits 3-7 into
                // positions 5-9 for the temporary address register.
                TAddress = (ushort)((TAddress & ~0x03e0) | ((data << 2) & 0x03e0));

                // Now set the fine Y scroll in the temporary address register without touching the other  Fine Y
                // is stored as bits 0-2 in data so we have to left shift data by 12 to get bits 0-2 into positions
                // 12-14.
                TAddress = (ushort)((TAddress & ~0x7000) | ((data << 12) & 0x7000));

                WriteLatch = false;
            }
        }

        public void WriteAddress(byte data)
        {
            if (!WriteLatch) // First write.
            {
                // Write data into the high byte position of the temporary address register. Don't touch the low byte
                // of the temporary address register.
                TAddress = (ushort)((TAddress & ~0xff00) | ((data << 8) & 0xff00));

                // We do have to set bit 15 of the temporary address register to zero since it technically doesn't
                // exist.
                TAddress &= 0x7fff; // Mask the bottom 15 

                WriteLatch = true;
            }
            else // Second write.
            {
                // Write data into the low byte position of the temporary address register. Don't touch the high byte
                // of the temporary address register.
                TAddress = (TAddress & ~0x00ff) | data;

                // Set the current address register to the value of the temporary address register.
                VAddress = TAddress;

                WriteLatch = false;
            }
        }

        // Rendering Methods.
        public void IncrementX()
        {
            if (VAddress.CourseX == 31)
            {
                VAddress.CourseX = 0;
                VAddress.NameTableSelectX = (byte)~VAddress.NameTableSelectX; // This simply flips the name table select X bit.
            }
            else
            {
                VAddress.CourseX += 1;
            }
        }

        public void IncrementY()
        {
            if (VAddress.FineY < 7)
            {
                VAddress.FineY++;
            }
            else
            {
                VAddress.FineY = 0;

                if (VAddress.CourseY == 29)
                {
                    VAddress.CourseY = 0;
                    VAddress.NameTableSelectY = (byte)~VAddress.NameTableSelectY; // This simply flips the name table select Y bit.
                }
                else if (VAddress.CourseY == 31)
                {
                    VAddress.CourseY = 0;
                }
                else
                {
                    VAddress.CourseY++;
                }
            }
        }

        public void TransferX()
        {
            VAddress.CourseX = TAddress.CourseX;
            VAddress.NameTableSelectX = TAddress.NameTableSelectX;
        }

        public void TransferY()
        {
            VAddress.CourseY = TAddress.CourseY;
            VAddress.FineY = TAddress.FineY;
            VAddress.NameTableSelectY = TAddress.NameTableSelectY;
        }

        public bool RenderingEnabled => RenderBackground || RenderSprites;

        // Control Register
        public bool VramAddressIncrement
        {
            get => GetBit(Control, 2);
            set => Control = (byte)SetBit(Control, 2, value);
        }

        public bool SpritePatternTableAddressFor8X8
        {
            get => GetBit(Control, 3);
            set => Control = (byte)SetBit(Control, 3, value);
        }

        public bool BackgroundPatternTableAddress
        {
            get => GetBit(Control, 4);
            set => Control = (byte)SetBit(Control, 4, value);
        }

        public bool SpriteSize
        {
            get => GetBit(Control, 5);
            set => Control = (byte)SetBit(Control, 5, value);
        }

        public bool PpuMasterSlaveSelect
        {
            get => GetBit(Control, 6);
            set => Control = (byte)SetBit(Control, 6, value);
        }

        public bool GenerateVerticalBlankingIntervalNmi
        {
            get => GetBit(Control, 7);
            set => Control = (byte)SetBit(Control, 7, value);
        }

        // Mask Register
        public bool EnableGrayscale
        {
            get => GetBit(Mask, 0);
            set => Mask = (byte)SetBit(Mask, 0, value);
        }

        public bool RenderBackgroundInLeftMost8PixelsOfScreen
        {
            get => GetBit(Mask, 1);
            set => Mask = (byte)SetBit(Mask, 1, value);
        }

        public bool RenderSpritesInLeftMost8PixelsOfScreen
        {
            get => GetBit(Mask, 2);
            set => Mask = (byte)SetBit(Mask, 2, value);
        }

        public bool RenderBackground
        {
            get => GetBit(Mask, 3);
            set => Mask = (byte)SetBit(Mask, 3, value);
        }

        public bool RenderSprites
        {
            get => GetBit(Mask, 4);
            set => Mask = (byte)SetBit(Mask, 4, value);
        }

        public bool EmphasizeRed
        {
            get => GetBit(Mask, 5);
            set => Mask = (byte)SetBit(Mask, 5, value);
        }

        public bool EmphasizeGreen
        {
            get => GetBit(Mask, 6);
            set => Mask = (byte)SetBit(Mask, 6, value);
        }

        public bool EmphasizeBlue
        {
            get => GetBit(Mask, 7);
            set => Mask = (byte)SetBit(Mask, 7, value);
        }

        // Status Register
        public bool SpriteOverflow
        {
            get => GetBit(Status, 5);
            set => Status = (byte)SetBit(Status, 5, value);
        }

        public bool Sprite0Hit
        {
            get => GetBit(Status, 6);
            set => Status = (byte)SetBit(Status, 6, value);
        }

        public bool VerticalBlankStarted
        {
            get => GetBit(Status, 7);
            set => Status = (byte)SetBit(Status, 7, value);
        }
    }
}