﻿using Microsoft.Extensions.Logging;
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
        private readonly Cartridge _cartridge;

        [SaveChildren("InternalRam")]
        private readonly CpuRam _internalRam;

        [SaveChildren("DmaState")]
        private readonly DmaState _dmaState = new DmaState();

        [SaveChildren]
        public Controllers Controllers { get; } = new Controllers();

        [Save]
        public long TotalCycles { get; set; }

        public Console(Cartridge cartridge, ILoggerFactory loggerFactory, ILogger logger)
        {
            if (cartridge == null) throw new ArgumentNullException(nameof(cartridge));

            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            Cpu = new Cpu(this);
            Ppu = new Ppu(cartridge, loggerFactory, loggerFactory.CreateLogger<Ppu>());

            _internalRam = new CpuRam();
            _cartridge = cartridge;
        }

        public void Reset()
        {
            Ppu.Reset();
            Cpu.Reset();
        }

        public PpuClockResult Clock()
        {
            var ppuResult = Ppu.Clock();

            // The CPU gets clocked every third system clock. This means that the CPU will get clocked on the first
            // system clock. Also, the CPU is suspended during a DMA transfer.
            if (TotalCycles % 3 == 0)
            {
                // Perform a DMA transfer cycle if we are processing a DMA.
                if (_dmaState.Processing)
                {
                    if (!_dmaState.Synchronized)
                    {
                        if (TotalCycles % 2 == 1)
                        {
                            _dmaState.Synchronized = true;
                        }
                    }
                    else
                    {
                        if (TotalCycles % 2 == 0) // Read on even cycles.
                        {
                            var address = (ushort)((_dmaState.CpuHighAddress << 8) | _dmaState.CurrentByte);
                            _dmaState.ReadByte = Read(address);
                        }
                        else // Write on odd cycles.
                        {
                            Ppu.Oam.Write((byte)_dmaState.CurrentByte, _dmaState.ReadByte);

                            _dmaState.CurrentByte++;

                            if (_dmaState.CurrentByte == 256)
                            {
                                _dmaState.Processing = false;
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

            return ppuResult;
        }

        public void CompleteFrame()
        {
            while (Clock() != PpuClockResult.FrameComplete) ;
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
                _dmaState.Processing = true;
                _dmaState.Synchronized = false;
                _dmaState.CurrentByte = 0;
                _dmaState.CpuHighAddress = data;
            }
        }
    }
}