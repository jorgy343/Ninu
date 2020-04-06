namespace Ninu.Emulator
{
    public class NameTableRam : IPpuBusComponent
    {
        private readonly byte[][] _tables =
        {
            new byte[1024],
            new byte[1024],
        };

        public bool PpuRead(ushort address, out byte data)
        {
            if (address >= 0x2000 && address <= 0x3eff)
            {
                data = 0;
                return true;
            }

            data = 0;
            return false;
        }

        public bool PpuWrite(ushort address, byte data)
        {
            if (address >= 0x2000 && address <= 0x3eff)
            {
                return true;
            }

            return false;
        }
    }
}