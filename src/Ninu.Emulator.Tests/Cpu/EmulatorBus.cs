using System;

namespace Ninu.Emulator.Tests.Cpu
{
    public class EmulatorBus : IBus
    {
        private readonly byte[] _memory;

        public EmulatorBus(byte[] memory)
        {
            _memory = memory ?? throw new ArgumentNullException(nameof(memory));
        }

        public byte Read(ushort address) => _memory[address];
        public void Write(ushort address, byte data) => _memory[address] = data;
    }
}