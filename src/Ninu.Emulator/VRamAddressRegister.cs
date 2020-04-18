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
            get => (byte)Bits.GetBits(Data, 0, 5);
            set => Data = (ushort)Bits.SetBits(Data, 0, 5, value);
        }

        public byte CourseY
        {
            get => (byte)Bits.GetBits(Data, 5, 5);
            set => Data = (ushort)Bits.SetBits(Data, 5, 5, value);
        }

        public byte NameTableSelect
        {
            get => (byte)Bits.GetBits(Data, 10, 2);
            set => Data = (ushort)Bits.SetBits(Data, 10, 2, value);
        }

        public byte NameTableSelectX
        {
            get => (byte)Bits.GetBits(Data, 10, 1);
            set => Data = (ushort)Bits.SetBits(Data, 10, 1, value);
        }

        public byte NameTableSelectY
        {
            get => (byte)Bits.GetBits(Data, 11, 1);
            set => Data = (ushort)Bits.SetBits(Data, 11, 1, value);
        }

        public byte FineY
        {
            get => (byte)Bits.GetBits(Data, 12, 3);
            set => Data = (ushort)Bits.SetBits(Data, 12, 3, value);
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