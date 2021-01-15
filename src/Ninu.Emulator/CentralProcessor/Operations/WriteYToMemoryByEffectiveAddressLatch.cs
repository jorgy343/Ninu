﻿namespace Ninu.Emulator.CentralProcessor.Operations
{
    /// <summary>
    /// Writes the Y register to memory at the address defined by the effective address latches
    /// <see cref="Cpu.EffectiveAddressLatchLow"/> and <see
    /// cref="Cpu.EffectiveAddressLatchHigh"/>.
    /// </summary>
    public class WriteYToMemoryByEffectiveAddressLatch : CpuOperation
    {
        private WriteYToMemoryByEffectiveAddressLatch()
        {

        }

        public static WriteYToMemoryByEffectiveAddressLatch Singleton { get; } = new();

        public override void Execute(Cpu cpu, IBus bus)
        {
            var address = (ushort)(cpu.EffectiveAddressLatchLow | (cpu.EffectiveAddressLatchHigh << 8));
            bus.Write(address, cpu.CpuState.Y);
        }
    }
}