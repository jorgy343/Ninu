using System;

namespace Ninu.Emulator
{
    public class Controllers : ICpuBusComponent
    {
        private readonly byte[] _controllerData = new byte[2];
        private readonly byte[] _controllerDataSnapshot = new byte[2];

        public void SetControllerData(int controller, byte data)
        {
            if (controller < 0 || controller > 1) throw new ArgumentOutOfRangeException(nameof(controller));

            _controllerData[controller] = data;
        }

        public bool CpuRead(ushort address, out byte data)
        {
            data = 0;

            if (address >= 0x4016 && address <= 0x4017)
            {
                // The first controller on address 0x4016 has its least significant bit set to zero.
                data = (byte)((uint)_controllerDataSnapshot[address & 0x0001] >> 7); // Output the most significant bit by logical left shifting the MSB to bit 0.

                _controllerDataSnapshot[address & 0x0001] <<= 1; // Shift the register one bit.

                return true;
            }

            return false;
        }

        public bool CpuWrite(ushort address, byte data)
        {
            if (address >= 0x4016 && address <= 0x4017)
            {
                if ((data & 0x01) != 0) // Only poll the controller if the first bit is set.
                {
                    // The first controller on address 0x4016 has its least significant bit set to zero.
                    _controllerDataSnapshot[address & 0x0001] = _controllerData[address & 0x0001];
                }

                return true;
            }

            return false;
        }
    }
}