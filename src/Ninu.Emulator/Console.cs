using Microsoft.Extensions.Logging;
using Ninu.Emulator.CentralProcessor;
using Ninu.Emulator.GraphicsProcessor;
using System;

namespace Ninu.Emulator
{
    public class Console : IBus
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger _logger;

        [SaveChildren]
        public Cpu Cpu { get; }

        [SaveChildren]
        public Ppu Ppu { get; }

        [SaveChildren("Cartridge")]
        private Cartridge? _cartridge;

        [SaveChildren("InternalRam")]
        public CpuRam InternalRam { get; } = new();

        [SaveChildren("DmaState")]
        public DmaState DmaState { get; } = new();

        [SaveChildren]
        public Controllers Controllers { get; } = new();

        [Save]
        public long TotalCycles { get; set; }

        public Console(ILoggerFactory loggerFactory, ILogger logger)
        {
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            Cpu = new Cpu(this);
            Ppu = new Ppu(loggerFactory, loggerFactory.CreateLogger<Ppu>());
        }

        public void PowerOn()
        {
            Ppu.PowerOn();
            Cpu.PowerOn();
        }

        public void Reset()
        {
            Ppu.Reset();
            Cpu.Reset();
        }

        public void LoadCartridge(Cartridge cartridge)
        {
            _cartridge = cartridge ?? throw new ArgumentNullException(nameof(cartridge));

            Ppu.LoadCartridge(_cartridge);
        }

        public ClockResult Clock()
        {
            if (_cartridge is null)
            {
                return ClockResult.Nothing;
            }

            if (Ppu.Nmi)
            {
                Cpu.Nmi = true;
                Ppu.Nmi = false;
            }

            var ppuResult = Ppu.Clock();

            // The CPU gets clocked every third system clock. This means that the CPU will get clocked on the first
            // system clock. Also, the CPU is suspended during a DMA transfer.
            if (TotalCycles % 3 == 0)
            {
                // Perform a DMA transfer cycle if we are processing a DMA.
                if (DmaState.Processing)
                {
                    if (!DmaState.Synchronized)
                    {
                        if (TotalCycles % 2 == 1)
                        {
                            DmaState.Synchronized = true;
                        }
                    }
                    else
                    {
                        if (TotalCycles % 2 == 0) // Read on even cycles.
                        {
                            var address = (ushort)((DmaState.CpuHighAddress << 8) | DmaState.CurrentByte);
                            DmaState.ReadByte = Read(address);
                        }
                        else // Write on odd cycles.
                        {
                            Ppu.Oam.Write((byte)DmaState.CurrentByte, DmaState.ReadByte);

                            DmaState.CurrentByte++;

                            if (DmaState.CurrentByte == 256)
                            {
                                DmaState.Processing = false;
                            }
                        }
                    }
                }
                else
                {
                    Cpu.Clock();
                }
            }

            TotalCycles++;

            return ppuResult;
        }

        /// <summary>
        /// Clocks the system until the PPU completes a frame. This will leave the PPU in a state
        /// such that the next system clock will be the first PPU clock into the next frame.
        /// </summary>
        public void CompleteFrame()
        {
            if (_cartridge is null)
            {
                return;
            }

            while (Clock() != ClockResult.FrameComplete) ;
        }

        /// <summary>
        /// Routes the read request to the appropriate device or area of memory. If no device or area of memory handles
        /// the given address, 0 is returned.
        /// </summary>
        /// <param name="address">The address that determines where to get the data from.</param>
        /// <returns>The data that was read or 0 if no data was read.</returns>
        public byte Read(ushort address)
        {
            if (_cartridge is not null && _cartridge.CpuRead(address, out var data))
            {
                return data;
            }

            if (InternalRam.CpuRead(address, out data))
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
            _cartridge?.CpuWrite(address, data);
            InternalRam.CpuWrite(address, data);

            Ppu.CpuWrite(address, data);

            Controllers.CpuWrite(address, data);

            // Process a DMA request.
            if (address == 0x4014)
            {
                DmaState.Processing = true;
                DmaState.Synchronized = false;
                DmaState.CurrentByte = 0;
                DmaState.CpuHighAddress = data;
            }
        }
    }
}