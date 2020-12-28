namespace Ninu.Emulator.CentralProcessor
{
    /// <summary>
    /// Represents the Nintendo's internal RAM. The physical size of the RAM is 2KiB which is
    /// mirrored four times in the address space one after another. This creates a total of 8KiB of
    /// address space that can be used to access the internal RAM. The address space for RAM is
    /// 0x0000 through 0x3fff inclusive.
    /// </summary>
    public class CpuRam : ICpuBusComponent
    {
        // This RAM is mirrored four times (total of 8KiB virtual RAM), but the actual RAM is only
        // 2KiB.
        [Save("Ram")]
        private readonly byte[] _ram = new byte[2048];

        /// <summary>
        /// When <paramref name="address"/> is greater than or equal to 0 and less than 0x2000
        /// (8KiB), a read is performed from the internal RAM contents. The internal RAM is only
        /// 2KiB in size. The address space is mirrored four times giving a virtual address space
        /// of 8KiB. Because of the mirroring, a read from address 0x0800 will actually read the
        /// internal RAM content at address 0x0000.
        /// </summary>
        /// <param name="address">The address to read from the internal RAM.</param>
        /// <param name="data">If the address was in range, contains the data read from the internal RAM.</param>
        /// <returns><c>true</c> if the address was within range; otherwise, <c>false</c>.</returns>
        public bool CpuRead(ushort address, out byte data)
        {
            if (address <= 0x1fff)
            {
                data = _ram[address & 0x07ff]; // Handle the mirroring for 2KiB of memory.
                return true;
            }

            data = 0;
            return false;
        }

        /// <summary>
        /// When <paramref name="address"/> is greater than or equal to 0 and less than 0x2000
        /// (8KiB), a write is performed to the internal RAM contents. The internal RAM is only
        /// 2KiB in size. The address space is mirrored four times giving a virtual address space
        /// of 8KiB. Because of the mirroring, a write to address 0x0800 will actually write to the
        /// internal RAM content at address 0x0000.
        /// </summary>
        /// <param name="address">The address to write to the internal RAM.</param>
        /// <param name="data">The data to write to the internal RAM if the address is within range.</param>
        /// <returns><c>true</c> if the address was within range; otherwise, <c>false</c>.</returns>
        public bool CpuWrite(ushort address, byte data)
        {
            if (address <= 0x1fff)
            {
                _ram[address & 0x07ff] = data; // Handle the mirroring for 2KiB of memory.
                return true;
            }

            return false;
        }
    }
}