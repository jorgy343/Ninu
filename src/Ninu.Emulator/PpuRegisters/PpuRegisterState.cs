namespace Ninu.Emulator.PpuRegisters
{
    public sealed class PpuRegisterState
    {
        public ushort VramAddress { get; set; }

        /// <summary>
        /// When this is set to false, a write to PPUADDR ($2006) will write to the high 8 bits of
        /// <see cref="VramAddress"/>. When it is set to true, the low 8 bits of <see cref="VramAddress"/>
        /// are written to.
        /// </summary>
        public bool VramAddressLatch { get; set; }

        /// <summary>
        /// This buffer is only used when reading from the PPUDATA register ($2007). This buffer is only updated
        /// after a read is performed on the PPUDATA register. In order to get the correct data after writing an
        /// address to the PPUADDR ($2006) address, one has to read from the PPUDATA register once to force the
        /// data buffer to update and then read it again to get the actual data. Reading the PPUDATA register
        /// multiple times without modifying the PPUADDR register will continually update the data buffer as
        /// expected.
        /// </summary>
        public byte DataBuffer { get; set; }

        public ControlRegister Control { get; } = new ControlRegister();
        public MaskRegister Mask { get; } = new MaskRegister();
        public StatusRegister Status { get; } = new StatusRegister();

        public void WriteVramAddressByte(byte addressByte)
        {
            if (VramAddressLatch)
            {
                VramAddress = (ushort)((0xff00 & VramAddress) | addressByte);
            }
            else
            {
                VramAddress = (ushort)((0x00ff & VramAddress) | (addressByte << 8));
            }

            VramAddressLatch = !VramAddressLatch;
        }

        /// <summary>
        /// Increments <see cref="VramAddress"/> either by 1 byte or 32 bytes depending on
        /// <see cref="ControlRegister.VramAddressIncrement"/>.
        /// </summary>
        public void PostVramReadWrite()
        {
            VramAddress += Control.VramAddressIncrement == VramAddressIncrement.Add1GoingAcross ? (ushort)1 : (ushort)32;

            // TODO: Assumption: We probably have to wrap the address.
            VramAddress &= 0x3fff;
        }
    }
}