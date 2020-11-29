using System;
using System.Collections;
using System.Collections.Generic;

namespace Ninu.Base
{
    public class ArrayMemory : IMemory
    {
        private readonly byte[] _memory;

        public ArrayMemory(int size)
        {
            if (size < 0 || size > 65536)
            {
                throw new ArgumentOutOfRangeException(nameof(size), $"The argument for parameter {nameof(size)} must be greater than or equal to 0 and less than or equal to 65536.");
            }

            _memory = new byte[size];
        }

        public ArrayMemory(byte[] memory)
        {
            if (memory.Length > 65536)
            {
                throw new ArgumentOutOfRangeException(nameof(memory), $"The argument for parameter {nameof(memory)} must be less than or equal to 65536.");
            }

            _memory = new byte[memory.Length];

            Array.Copy(memory, _memory, memory.Length);
        }

        public byte this[ushort address]
        {
            get => _memory[address];
            set => _memory[address] = value;
        }

        public ushort Size => (ushort)_memory.Length;

        public IEnumerator<byte> GetEnumerator()
        {
            foreach (var b in _memory)
            {
                yield return b;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => _memory.GetEnumerator();
    }
}