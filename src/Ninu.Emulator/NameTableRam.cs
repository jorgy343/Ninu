// ReSharper disable ConditionIsAlwaysTrueOrFalse

namespace Ninu.Emulator
{
    public class NameTableRam
    {
        private readonly byte[][] _tables =
        {
            new byte[1024],
            new byte[1024],
        };

        public bool PpuRead(NameTableMirrorMode mirrorMode, ushort address, out byte data)
        {
            if (address >= 0x2000 && address <= 0x3eff)
            {
                data = 0;

                address -= 0x2000;

                if (mirrorMode == NameTableMirrorMode.Horizontal)
                {
                    if (address >= 0x0000 && address <= 0x03ff) // First 1KiB maps to the first name table.
                    {
                        data = _tables[0][address & 0x03ff];
                    }
                    else if (address >= 0x0400 && address <= 0x07ff) // Second 1KiB maps back to the first name table.
                    {
                        data = _tables[0][address & 0x03ff];
                    }
                    else if (address >= 0x0800 && address <= 0x0bff) // Third 1KiB maps to the second name table.
                    {
                        data = _tables[1][address & 0x03ff];
                    }
                    else if (address >= 0x0c00 && address <= 0x0fff) // Fourth 1KiB maps back to the second name table.
                    {
                        data = _tables[1][address & 0x03ff];
                    }
                }
                else if (mirrorMode == NameTableMirrorMode.Vertical)
                {
                    if (address >= 0x0000 && address <= 0x03ff) // First 1KiB maps to the first name table.
                    {
                        data = _tables[0][address & 0x03ff];
                    }
                    else if (address >= 0x0400 && address <= 0x07ff) // Second 1KiB maps to the second name table.
                    {
                        data = _tables[1][address & 0x03ff];
                    }
                    else if (address >= 0x0800 && address <= 0x0bff) // Third 1KiB maps back to the first name table.
                    {
                        data = _tables[0][address & 0x03ff];
                    }
                    else if (address >= 0x0c00 && address <= 0x0fff) // Fourth 1KiB maps back to the second name table.
                    {
                        data = _tables[1][address & 0x03ff];
                    }
                }

                return true;
            }

            data = 0;
            return false;
        }

        public bool PpuWrite(NameTableMirrorMode mirrorMode, ushort address, byte data)
        {
            if (address >= 0x2000 && address <= 0x3eff)
            {
                address -= 0x2000;

                if (mirrorMode == NameTableMirrorMode.Horizontal)
                {
                    if (address >= 0x0000 && address <= 0x03ff) // First 1KiB maps to the first name table.
                    {
                        _tables[0][address & 0x03ff] = data;
                    }
                    else if (address >= 0x0400 && address <= 0x07ff) // Second 1KiB maps back to the first name table.
                    {
                        _tables[0][address & 0x03ff] = data;
                    }
                    else if (address >= 0x0800 && address <= 0x0bff) // Third 1KiB maps to the second name table.
                    {
                        _tables[1][address & 0x03ff] = data;
                    }
                    else if (address >= 0x0c00 && address <= 0x0fff) // Fourth 1KiB maps back to the second name table.
                    {
                        _tables[1][address & 0x03ff] = data;
                    }
                }
                else if (mirrorMode == NameTableMirrorMode.Vertical)
                {
                    if (address >= 0x0000 && address <= 0x03ff) // First 1KiB maps to the first name table.
                    {
                        _tables[0][address & 0x03ff] = data;
                    }
                    else if (address >= 0x0400 && address <= 0x07ff) // Second 1KiB maps to the second name table.
                    {
                        _tables[1][address & 0x03ff] = data;
                    }
                    else if (address >= 0x0800 && address <= 0x0bff) // Third 1KiB maps back to the first name table.
                    {
                        _tables[0][address & 0x03ff] = data;
                    }
                    else if (address >= 0x0c00 && address <= 0x0fff) // Fourth 1KiB maps back to the second name table.
                    {
                        _tables[1][address & 0x03ff] = data;
                    }
                }

                return true;
            }

            return false;
        }
    }
}