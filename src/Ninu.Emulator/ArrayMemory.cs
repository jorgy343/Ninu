using System.Collections;
using System.Collections.Generic;

namespace Ninu.Emulator
{
    public class ArrayMemory : IMemory
    {
        private readonly byte[] _memory;

        public ArrayMemory(ushort size)
        {
            _memory = new byte[size];
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