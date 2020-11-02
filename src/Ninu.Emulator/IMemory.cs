using System.Collections.Generic;

namespace Ninu.Emulator
{
    public interface IMemory : IEnumerable<byte>
    {
        byte this[ushort address] { get; set; }

        ushort Size { get; }
    }
}