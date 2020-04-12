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
            get => (byte)((Data & 0x001f) >> 0);
            set => Data = (ushort)((Data & ~0x001f) | ((value << 0) & 0x001f));
        }

        public byte CourseY
        {
            get => (byte)((Data & 0x03e0) >> 5);
            set => Data = (ushort)((Data & ~0x03e0) | ((value << 5) & 0x03e0));
        }

        public byte NameTableSelect
        {
            get => (byte)((Data & 0x0c00) >> 10);
            set => Data = (ushort)((Data & ~0x0c00) | ((value << 10) & 0x0c00));
        }

        public byte NameTableSelectX
        {
            get => (byte)((Data & 0x0400) >> 10);
            set => Data = (ushort)((Data & ~0x0400) | ((value << 10) & 0x0400));
        }

        public byte NameTableSelectY
        {
            get => (byte)((Data & 0x0800) >> 11);
            set => Data = (ushort)((Data & ~0x0800) | ((value << 11) & 0x0800));
        }

        public byte FineY
        {
            get => (byte)((Data & 0x7000) >> 12);
            set => Data = (ushort)((Data & ~0x7000) | ((value << 12) & 0x7000));
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