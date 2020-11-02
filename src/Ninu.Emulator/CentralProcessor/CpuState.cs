namespace Ninu.Emulator.CentralProcessor
{
    public class CpuState
    {
        [Save]
        public byte A { get; set; }

        [Save]
        public byte X { get; set; }

        [Save]
        public byte Y { get; set; }

        [Save]
        public byte S { get; set; }

        [Save]
        public CpuFlags P { get; set; }

        [Save]
        public ushort PC { get; set; }

        public bool GetFlag(CpuFlags flag) => (P & flag) != 0;

        public void SetFlag(CpuFlags flag, bool value)
        {
            if (value)
            {
                P |= flag;
            }
            else
            {
                P &= ~flag;
            }
        }

        public void SetZeroFlag(byte data) => SetFlag(CpuFlags.Z, data == 0);
        public void SetZeroFlag(ushort data) => SetFlag(CpuFlags.Z, (data & 0x00ff) == 0);

        public void SetNegativeFlag(byte data) => SetFlag(CpuFlags.N, (data & 0x80) != 0);
        public void SetNegativeFlag(ushort data) => SetFlag(CpuFlags.N, (data & 0x0080) != 0);
    }
}