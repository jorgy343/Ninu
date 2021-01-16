namespace Ninu.Emulator.CentralProcessor
{
    public static partial class Operations2
    {
        public static class ReadMemory
        {
            public static class ByPC
            {
                public static void IntoData(Cpu cpu, IBus bus)
                {
                    var data = bus.Read(cpu.CpuState.PC);
                    cpu.DataLatch = data;
                }

                public static void IntoAddressLow(Cpu cpu, IBus bus)
                {
                    var data = bus.Read(cpu.CpuState.PC);
                    cpu.AddressLatchLow = data;
                }

                public static void IntoAddressHigh(Cpu cpu, IBus bus)
                {
                    var data = bus.Read(cpu.CpuState.PC);
                    cpu.AddressLatchHigh = data;
                }

                public static void IntoEffectiveAddressLow(Cpu cpu, IBus bus)
                {
                    var data = bus.Read(cpu.CpuState.PC);
                    cpu.EffectiveAddressLatchLow = data;
                }

                public static void IntoEffectiveAddressHigh(Cpu cpu, IBus bus)
                {
                    var data = bus.Read(cpu.CpuState.PC);
                    cpu.EffectiveAddressLatchHigh = data;
                }
            }

            public static class ByS
            {
                public static void IntoData(Cpu cpu, IBus bus)
                {
                    var address = (ushort)(0x100 + cpu.CpuState.S); // The stack occupies page 0x01.
                    var data = bus.Read(address);

                    cpu.DataLatch = data;
                }
            }

            public static class ByAddress
            {
                /// <summary>
                /// Fetches the low byte of the address found using the address latches and stores
                /// it in the effective address latche low.
                /// </summary>
                public static void IntoEffectiveAddressLow(Cpu cpu, IBus bus)
                {
                    var address = (ushort)(cpu.AddressLatchLow | (cpu.AddressLatchHigh << 8));
                    var data = bus.Read(address);

                    cpu.EffectiveAddressLatchLow = data;
                }

                public static class IntoEffectiveAddressHigh
                {
                    /// <summary>
                    /// Fetches the high byte of the address found using the address latches and stores
                    /// it in the effective address latche high. This operation reproduces a bug found
                    /// when doing indirect addressing in the 6502. Typically, the high byte of the
                    /// effective address is read from memory at the address one more than the what the
                    /// address latches point at. For example, if the address latches point to 0x348c,
                    /// the low byte of the effective address would be read from memory at 0x348c and
                    /// the high byte of the effective address would be read from memory at 0x348d.
                    /// However, if the address latches point to 0x34ff, the low byte of the effective
                    /// address would be read from memory at 0x34ff as expected but the high byte of
                    /// the effective address would be read from memory at 0x3400. When reading the
                    /// second byte from memory, only the low byte of the address is incremented. The
                    /// high byte (page byte) doesn't change.
                    /// </summary>
                    public static void WithWrapping(Cpu cpu, IBus bus)
                    {
                        // Increment the low byte and allow it towrap if the value is 0xff.
                        var addressLow = (byte)((cpu.AddressLatchLow + 1) & 0xff);
                        var addressHigh = cpu.AddressLatchHigh;

                        var address = (ushort)(addressLow | (addressHigh << 8));

                        var data = bus.Read(address);
                        cpu.EffectiveAddressLatchHigh = data;
                    }
                }
            }

            public static class ByEffectiveAddress
            {
                public static void IntoData(Cpu cpu, IBus bus)
                {
                    var address = (ushort)(cpu.EffectiveAddressLatchLow | (cpu.EffectiveAddressLatchHigh << 8));
                    var data = bus.Read(address);

                    cpu.DataLatch = data;
                }
            }
        }
    }
}