namespace Ninu.Emulator.CentralProcessor
{
    public static partial class Operations
    {
        public static class WriteA
        {
            public static class ToMemory
            {
                public static void ByEffectiveAddress(Cpu cpu, IBus bus)
                {
                    var address = (ushort)(cpu.EffectiveAddressLatchLow | (cpu.EffectiveAddressLatchHigh << 8));
                    bus.Write(address, cpu.CpuState.A);
                }
            }
        }

        public static class WriteX
        {
            public static class ToMemory
            {
                public static void ByEffectiveAddress(Cpu cpu, IBus bus)
                {
                    var address = (ushort)(cpu.EffectiveAddressLatchLow | (cpu.EffectiveAddressLatchHigh << 8));
                    bus.Write(address, cpu.CpuState.X);
                }
            }
        }

        public static class WriteY
        {
            public static class ToMemory
            {
                public static void ByEffectiveAddress(Cpu cpu, IBus bus)
                {
                    var address = (ushort)(cpu.EffectiveAddressLatchLow | (cpu.EffectiveAddressLatchHigh << 8));
                    bus.Write(address, cpu.CpuState.Y);
                }
            }
        }

        public static class WriteData
        {
            public static class ToMemory
            {
                public static void ByEffectiveAddress(Cpu cpu, IBus bus)
                {
                    var address = (ushort)(cpu.EffectiveAddressLatchLow | (cpu.EffectiveAddressLatchHigh << 8));
                    bus.Write(address, cpu.DataLatch);
                }
            }
        }
    }
}