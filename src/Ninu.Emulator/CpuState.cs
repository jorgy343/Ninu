namespace Ninu.Emulator
{
    public class CpuState
    {
        public byte A { get; set; }
        public byte X { get; set; }
        public byte Y { get; set; }
        public byte S { get; set; }
        public ushort PC { get; set; }
        public CpuFlags Flags { get; set; }

        public bool GetFlag(CpuFlags flag) => (Flags & flag) != 0;

        public void SetFlag(CpuFlags flag, bool value)
        {
            if (value)
            {
                Flags |= flag;
            }
            else
            {
                Flags &= ~flag;
            }
        }

        public void SetZeroFlag(byte data) => SetFlag(CpuFlags.Z, data == 0);
        public void SetZeroFlag(ushort data) => SetFlag(CpuFlags.Z, (data & 0x00ff) == 0);

        public void SetNegativeFlag(byte data) => SetFlag(CpuFlags.N, (data & 0x80) != 0);
        public void SetNegativeFlag(ushort data) => SetFlag(CpuFlags.N, (data & 0x0080) != 0);
    }
}