using System.Collections.Generic;

namespace Ninu.Base
{
    public interface IMemory : IEnumerable<byte>
    {
        byte this[ushort address] { get; set; }

        ushort Size { get; }
    }
}