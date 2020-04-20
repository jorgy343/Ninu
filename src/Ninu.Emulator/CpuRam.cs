namespace Ninu.Emulator
{
    public class CpuRam : ICpuBusComponent, IPersistable
    {
        // This RAM is mirror four times (total of 8KiB).
        private byte[] _ram = new byte[2048];

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

        public void SaveState(SaveStateContext context)
        {
            context.AddToState("CpuRam.Ram", _ram);
        }

        public void LoadState(SaveStateContext context)
        {
            _ram = context.GetFromState<byte[]>("CpuRam.Ram");
        }
    }
}