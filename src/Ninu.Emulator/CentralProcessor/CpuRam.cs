namespace Ninu.Emulator.CentralProcessor
{
    public class CpuRam : ICpuBusComponent
    {
        // This RAM is mirror four times (total of 8KiB).
        [Save("Ram")]
        private readonly byte[] _ram = new byte[2048];

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