namespace Ninu.Emulator.CentralProcessor
{
    public static partial class Operations
    {
        public static class Interrupts
        {
            public static void FetchIrqVectorLowIntoEffectiveAddressLatchLow(Cpu cpu, IBus bus)
            {
                cpu.EffectiveAddressLatchLow = bus.Read(0xfffe);
            }

            public static void FetchIrqVectorHighIntoEffectiveAddressLatchHigh(Cpu cpu, IBus bus)
            {
                cpu.EffectiveAddressLatchHigh = bus.Read(0xffff);
            }

            public static void FetchNmiVectorLowIntoAddressLatchLow(Cpu cpu, IBus bus)
            {
                cpu.AddressLatchLow = bus.Read(0xfffa);
            }

            public static void FetchNmiVectorHighIntoAddressLatchHigh(Cpu cpu, IBus bus)
            {
                cpu.AddressLatchHigh = bus.Read(0xfffb);
            }

            public static void FetchResetVectorLowIntoAddressLatchLow(Cpu cpu, IBus bus)
            {
                cpu.AddressLatchLow = bus.Read(0xfffc);
            }

            public static void FetchResetVectorHighIntoAddressLatchHigh(Cpu cpu, IBus bus)
            {
                cpu.AddressLatchHigh = bus.Read(0xfffd);
            }

            public static void PushPCLowOnStack(Cpu cpu, IBus bus)
            {
                var pcLow = (byte)(cpu.CpuState.PC & 0x00ff);
                bus.Write((ushort)(0x100 + cpu.CpuState.S - 1), pcLow);
            }

            public static void PushPCHighOnStack(Cpu cpu, IBus bus)
            {
                var pcHigh = (byte)(cpu.CpuState.PC >> 8);
                bus.Write((ushort)(0x100 + cpu.CpuState.S), pcHigh);
            }

            public static void SetPCToAddressLatchAndFetchInstruction(Cpu cpu, IBus bus)
            {
                cpu.CpuState.PC = (ushort)(cpu.AddressLatchLow | (cpu.AddressLatchHigh << 8));

                var instruction = bus.Read(cpu.CpuState.PC);
                cpu.ExecuteInstruction(instruction);
            }
        }
    }
}