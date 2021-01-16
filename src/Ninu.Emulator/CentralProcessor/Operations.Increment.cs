namespace Ninu.Emulator.CentralProcessor
{
    public static partial class Operations
    {
        public static class Increment
        {
            public static class AddressLow
            {
                public static class ByX
                {
                    public static void WithWrapping(Cpu cpu, IBus bus)
                    {
                        cpu.AddressLatchLow += cpu.CpuState.X;
                    }
                }
            }

            public static class EffectiveaddressLow
            {
                public static class ByX
                {
                    public static void WithWrapping(Cpu cpu, IBus bus)
                    {
                        cpu.EffectiveAddressLatchLow += cpu.CpuState.X;
                    }

                    public static void WithoutWrapping(Cpu cpu, IBus bus)
                    {
                        var address = (ushort)(cpu.EffectiveAddressLatchLow | (cpu.EffectiveAddressLatchHigh << 8));
                        address = (ushort)((address + cpu.CpuState.X) & 0xffff); // Wrap addresses around 64KiB.

                        cpu.EffectiveAddressLatchLow = (byte)(address & 0xff);
                        cpu.EffectiveAddressLatchHigh = (byte)(address >> 8);
                    }
                }

                public static class ByY
                {
                    public static void WithWrapping(Cpu cpu, IBus bus)
                    {
                        cpu.EffectiveAddressLatchLow += cpu.CpuState.Y;
                    }

                    public static void WithoutWrapping(Cpu cpu, IBus bus)
                    {
                        var address = (ushort)(cpu.EffectiveAddressLatchLow | (cpu.EffectiveAddressLatchHigh << 8));
                        address = (ushort)((address + cpu.CpuState.Y) & 0xffff); // Wrap addresses around 64KiB.

                        cpu.EffectiveAddressLatchLow = (byte)(address & 0xff);
                        cpu.EffectiveAddressLatchHigh = (byte)(address >> 8);
                    }
                }
            }

            public static class EffectiveAddressHigh
            {
                public static class ByX
                {
                    public static void OnlyWithCarry(Cpu cpu, IBus bus)
                    {
                        var result = cpu.EffectiveAddressLatchLow + cpu.CpuState.X;
                        var increment = result >> 8; // This will shift the carry bit into the LSB.

                        cpu.EffectiveAddressLatchHigh += (byte)increment;
                    }
                }
            }
        }
    }
}