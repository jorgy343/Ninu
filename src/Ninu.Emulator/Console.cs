using System;

namespace Ninu.Emulator
{
    public class Console : IBus
    {
        public Cpu Cpu { get; }
        public Ppu Ppu { get; }

        private readonly Cartridge _cartridge;
        private readonly CpuRam _internalRam;

        /// <summary>
        /// Determines if the DMA is processing and the CPU is suspended.
        /// </summary>
        private bool _dmaProcessing;

        /// <summary>
        /// Determines if the DMA is properly synchronized to the CPU clock. If this is false, a dummy cycle will take
        /// place.
        /// </summary>
        private bool _dmaSynchronized;

        /// <summary>
        /// The current byte that needs to be read during the DMA process. At the start of a DMA process, this will be
        /// zero and it counts up for every byte copied.
        /// </summary>
        private int _dmaCurrentByte;

        /// <summary>
        /// This is the page from which data will be copied from the CPU bus. The page is the high byte of the CPU
        /// address. Data will then be read from 0xXX00 to 0xXXff during the DMA transfer.
        /// </summary>
        private byte _dmaCpuHighAddress;

        /// <summary>
        /// This stores the byte of data that was read during the read cycle of the DMA process.
        /// </summary>
        private byte _dmaReadByte;

        public Controllers Controllers { get; } = new Controllers();

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

            // The CPU gets clocked every third system clock. This means that the CPU will get clocked on the first
            // system clock. Also, the CPU is suspended during a DMA transfer.
            if (TotalCycles % 3 == 0)
            {
                // Perform a DMA transfer cycle if we are processing a DMA.
                if (_dmaProcessing)
                {
                    if (!_dmaSynchronized)
                    {
                        if (TotalCycles % 2 == 1)
                        {
                            _dmaSynchronized = true;
                        }
                    }
                    else
                    {
                        if (TotalCycles % 2 == 0) // Read on even cycles.
                        {
                            var address = (ushort)((_dmaCpuHighAddress << 8) | _dmaCurrentByte);
                            _dmaReadByte = Read(address);
                        }
                        else // Write on odd cycles.
                        {
                            Ppu.Oam.Write((byte)_dmaCurrentByte, _dmaReadByte);

                            _dmaCurrentByte++;

                            if (_dmaCurrentByte == 256)
                            {
                                _dmaProcessing = false;
                            }
                        }
                    }
                }
                else
                {
                    Cpu.Clock();
                }
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

                // The CPU gets clocked every third system clock. This means that the CPU will get clocked on the first
                // system clock.
                if (TotalCycles % 3 == 0)
                {
                    // Perform a DMA transfer cycle if we are processing a DMA.
                    if (_dmaProcessing)
                    {
                        if (!_dmaSynchronized)
                        {
                            if (TotalCycles % 2 == 1)
                            {
                                _dmaSynchronized = true;
                            }
                        }
                        else
                        {
                            if (TotalCycles % 2 == 0) // Read on even cycles.
                            {
                                var address = (ushort)((_dmaCpuHighAddress << 8) | _dmaCurrentByte);
                                _dmaReadByte = Read(address);
                            }
                            else // Write on odd cycles.
                            {
                                Ppu.Oam.Write((byte)_dmaCurrentByte, _dmaReadByte);

                                _dmaCurrentByte++;

                                if (_dmaCurrentByte == 256)
                                {
                                    _dmaProcessing = false;
                                }
                            }
                        }
                    }
                    else
                    {
                        Cpu.Clock();
                    }
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
        /// Routes the read request to the appropriate device or area of memory. If no device or area of memory handles
        /// the given address, 0 is returned.
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

            if (Controllers.CpuRead(address, out data))
            {
                return data;
            }

            return 0;
        }

        public void Write(ushort address, byte data)
        {
            _cartridge.CpuWrite(address, data);
            _internalRam.CpuWrite(address, data);

            Ppu.CpuWrite(address, data);

            Controllers.CpuWrite(address, data);

            // Process a DMA request.
            if (address == 0x4014)
            {
                _dmaProcessing = true;
                _dmaSynchronized = false;
                _dmaCurrentByte = 0;
                _dmaCpuHighAddress = data;
            }
        }
    }
}