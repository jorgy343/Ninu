using System;

namespace Ninu.Emulator
{
    public class Console : IBus
    {
        public Cpu Cpu { get; }
        public Ppu Ppu { get; }

        private readonly Cartridge _cartridge;
        private readonly CpuRam _internalRam;

        public long TotalCycles { get; set; }

        public Console(Cartridge cartridge)
        {
            Cpu = new Cpu(this);
            Ppu = new Ppu(cartridge);

            _internalRam = new CpuRam();
            _cartridge = cartridge ?? throw new ArgumentNullException(nameof(cartridge));
        }

        public void Reset()
        {
            Ppu.Reset();
            Cpu.Reset();
        }

        public void Clock()
        {
            Ppu.Clock();

            // The CPU gets clocked every third system clock. This means that the CPU will
            // get clocked on the first system clock.
            if (TotalCycles % 3 == 0)
            {
                Cpu.Clock();
            }

            // TODO: Are we supposed to trigger the nmi after the CPU clocks?
            if (Ppu.CallNmi)
            {
                Cpu.NonMaskableInterrupt();

                Ppu.CallNmi = false;
            }

            TotalCycles++;
        }

        public void CompleteFrame()
        {
            var ppuClockResult = PpuClockResult.NormalCycle;

            while (ppuClockResult != PpuClockResult.FrameComplete)
            {
                ppuClockResult = Ppu.Clock();

                // The CPU gets clocked every third system clock. This means that the CPU will
                // get clocked on the first system clock.
                if (TotalCycles % 3 == 0)
                {
                    Cpu.Clock();
                }

                // TODO: Are we supposed to trigger the nmi after the CPU clocks?
                if (Ppu.CallNmi)
                {
                    Cpu.NonMaskableInterrupt();

                    Ppu.CallNmi = false;
                }

                TotalCycles++;
            }
        }

        /// <summary>
        /// Routes the read request to the appropriate device or area of memory. If no device
        /// or area of memory handles the given address, 0 is returned.
        /// </summary>
        /// <param name="address">The address that determines where to get the data from.</param>
        /// <returns>The data that was read or 0 if no data was read.</returns>
        public byte Read(ushort address)
        {
            if (_cartridge.CpuRead(address, out var data))
            {
                return data;
            }

            if (_internalRam.CpuRead(address, out data))
            {
                return data;
            }

            if (Ppu.CpuRead(address, out data))
            {
                return data;
            }

            // TODO: Go through the rest of the devices.

            return 0;
        }

        public void Write(ushort address, byte data)
        {
            _cartridge.CpuWrite(address, data);
            _internalRam.CpuWrite(address, data);

            Ppu.CpuWrite(address, data);
        }
    }
}