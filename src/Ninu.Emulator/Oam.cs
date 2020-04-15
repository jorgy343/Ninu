// ReSharper disable ConditionIsAlwaysTrueOrFalse
using System;

namespace Ninu.Emulator
{
    public class Oam
    {
        private readonly byte[] _data = new byte[256];

        public Sprite8x8 Get8x8Sprite(int index)
        {
            if (index < 0 || index > 63) throw new ArgumentOutOfRangeException(nameof(index));

            var byteIndex = index * 4;

            return new Sprite8x8(_data[byteIndex + 0], _data[byteIndex + 1], _data[byteIndex + 2], _data[byteIndex + 3]);
        }

        public byte CpuRead(byte address)
        {
            return _data[address];
        }

        public void CpuWrite(byte address, byte data)
        {
            _data[address] = data;
        }
    }
}