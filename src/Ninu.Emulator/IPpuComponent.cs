namespace Ninu.Emulator
{
    /// <summary>
    /// Represents a component on the PPU bus that is capable of handling reading and writing from
    /// memory addresses.
    /// </summary>
    public interface IPpuBusComponent
    {
        /// <summary>
        /// Attempts to handle a read operation on the bus. If the address is within the
        /// component's addressable range, the read is performed and this method returns
        /// true. If the address is outside of the addressable range, this method returns
        /// false and the next component on the bus gets a chance to handle the read.
        /// </summary>
        /// <param name="address">The address that the read is being requested for.</param>
        /// <param name="data">If the <paramref name="address"/> is within the addressable range of the component, this will be the data read; otherwise, it is zero.</param>
        /// <returns>true if the component handled the read; otherwise, false.</returns>
        public bool PpuRead(ushort address, out byte data);

        /// <summary>
        /// Attempts to handle a write operation on the bus. If the address is within
        /// the component's addressable range, the write is performed and this method
        /// returns true. if the address is outside of the addressable range, this
        /// method returns false and the next component on the bus gets a change to
        /// handle the write.
        /// </summary>
        /// <param name="address">The address that the write is being requested for.</param>
        /// <param name="data">If the <paramref name="address"/> is within the addressable range of the component, this data will be written.</param>
        /// <returns>true if the component handled the write; otherwise, false.</returns>
        public bool PpuWrite(ushort address, byte data);
    }
}