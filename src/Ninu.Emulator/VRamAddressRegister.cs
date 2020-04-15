// ReSharper disable ShiftExpressionRealShiftCountIsZero

namespace Ninu.Emulator
{
    public struct VRamAddressRegister
    {
        public ushort Data;

        public VRamAddressRegister(ushort data)
        {
            Data = data;
        }

        public byte CourseX
        {
            get => (byte)Bits.GetBits(Data, 5, 0);
            set => Data = (ushort)Bits.SetBits(Data, value, 5, 0);
        }

        public byte CourseY
        {
            get => (byte)Bits.GetBits(Data, 5, 5);
            set => Data = (ushort)Bits.SetBits(Data, value, 5, 5);
        }

        public byte NameTableSelect
        {
            get => (byte)Bits.GetBits(Data, 2, 10);
            set => Data = (ushort)Bits.SetBits(Data, value, 2, 10);
        }

        public byte NameTableSelectX
        {
            get => (byte)Bits.GetBits(Data, 1, 10);
            set => Data = (ushort)Bits.SetBits(Data, value, 1, 10);
        }

        public byte NameTableSelectY
        {
            get => (byte)Bits.GetBits(Data, 1, 11);
            set => Data = (ushort)Bits.SetBits(Data, value, 1, 11);
        }

        public byte FineY
        {
            get => (byte)Bits.GetBits(Data, 3, 12);
            set => Data = (ushort)Bits.SetBits(Data, value, 3, 12);
        }

        public static VRamAddressRegister operator ~(VRamAddressRegister value) => new VRamAddressRegister((ushort)~value.Data);

        public static VRamAddressRegister operator &(VRamAddressRegister left, VRamAddressRegister right) => new VRamAddressRegister((ushort)(left.Data & right.Data));
        public static VRamAddressRegister operator |(VRamAddressRegister left, VRamAddressRegister right) => new VRamAddressRegister((ushort)(left.Data | right.Data));
        public static VRamAddressRegister operator ^(VRamAddressRegister left, VRamAddressRegister right) => new VRamAddressRegister((ushort)(left.Data ^ right.Data));

        public static VRamAddressRegister operator &(byte left, VRamAddressRegister right) => new VRamAddressRegister((ushort)(left & right.Data));
        public static VRamAddressRegister operator |(byte left, VRamAddressRegister right) => new VRamAddressRegister((ushort)(left | right.Data));
        public static VRamAddressRegister operator ^(byte left, VRamAddressRegister right) => new VRamAddressRegister((ushort)(left ^ right.Data));

        public static VRamAddressRegister operator &(VRamAddressRegister left, byte right) => new VRamAddressRegister((ushort)(left.Data & right));
        public static VRamAddressRegister operator |(VRamAddressRegister left, byte right) => new VRamAddressRegister((ushort)(left.Data | right));
        public static VRamAddressRegister operator ^(VRamAddressRegister left, byte right) => new VRamAddressRegister((ushort)(left.Data ^ right));

        public static VRamAddressRegister operator &(ushort left, VRamAddressRegister right) => new VRamAddressRegister((ushort)(left & right.Data));
        public static VRamAddressRegister operator |(ushort left, VRamAddressRegister right) => new VRamAddressRegister((ushort)(left | right.Data));
        public static VRamAddressRegister operator ^(ushort left, VRamAddressRegister right) => new VRamAddressRegister((ushort)(left ^ right.Data));

        public static VRamAddressRegister operator &(VRamAddressRegister left, ushort right) => new VRamAddressRegister((ushort)(left.Data & right));
        public static VRamAddressRegister operator |(VRamAddressRegister left, ushort right) => new VRamAddressRegister((ushort)(left.Data | right));
        public static VRamAddressRegister operator ^(VRamAddressRegister left, ushort right) => new VRamAddressRegister((ushort)(left.Data ^ right));

        public static VRamAddressRegister operator &(int left, VRamAddressRegister right) => new VRamAddressRegister((ushort)(left & right.Data));
        public static VRamAddressRegister operator |(int left, VRamAddressRegister right) => new VRamAddressRegister((ushort)(left | right.Data));
        public static VRamAddressRegister operator ^(int left, VRamAddressRegister right) => new VRamAddressRegister((ushort)(left ^ right.Data));

        public static VRamAddressRegister operator &(VRamAddressRegister left, int right) => new VRamAddressRegister((ushort)(left.Data & right));
        public static VRamAddressRegister operator |(VRamAddressRegister left, int right) => new VRamAddressRegister((ushort)(left.Data | right));
        public static VRamAddressRegister operator ^(VRamAddressRegister left, int right) => new VRamAddressRegister((ushort)(left.Data ^ right));

        public static implicit operator ushort(VRamAddressRegister register) => register.Data;
        public static implicit operator int(VRamAddressRegister register) => register.Data;

        public static implicit operator VRamAddressRegister(ushort data) => new VRamAddressRegister(data);
        public static implicit operator VRamAddressRegister(int data) => new VRamAddressRegister((ushort)data);
    }
}