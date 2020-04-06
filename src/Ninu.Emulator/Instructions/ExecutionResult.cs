namespace Ninu.Emulator.Instructions
{
    public class ExecutionResult
    {
        public int Cycles { get; }

        public byte A { get; }
        public byte X { get; }
        public byte Y { get; }
        public ushort PC { get; }
        public ushort SP { get; }
        public CpuFlags Flags { get; }

        public bool BusWrite { get; }
        public ushort BusAddress { get; }
        public byte BusData { get; }

        public ExecutionResult(int cycles, byte a, byte x, byte y, ushort pc, ushort sp, CpuFlags flags)
        {
            Cycles = cycles;
            A = a;
            X = x;
            Y = y;
            PC = pc;
            SP = sp;
            Flags = flags;
            BusWrite = false;
            BusAddress = 0;
            BusData = 0;
        }

        public ExecutionResult(int cycles, byte a, byte x, byte y, ushort pc, ushort sp, CpuFlags flags, ushort busAddress, byte busData)
        {
            Cycles = cycles;
            A = a;
            X = x;
            Y = y;
            PC = pc;
            SP = sp;
            Flags = flags;
            BusWrite = true;
            BusAddress = busAddress;
            BusData = busData;
        }
    }
}