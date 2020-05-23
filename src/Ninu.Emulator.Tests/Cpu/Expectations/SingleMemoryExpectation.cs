using System;

namespace Ninu.Emulator.Tests.Cpu.Expectations
{
    public class SingleMemoryExpectation : IExpectation
    {
        private readonly int _memoryLocation;
        private readonly byte _expectedValue;

        public SingleMemoryExpectation(int memoryLocation, byte expectedValue)
        {
            _memoryLocation = memoryLocation;
            _expectedValue = expectedValue;
        }

        public bool AssertExpectation(byte[] memory)
        {
            if (memory == null)
            {
                throw new ArgumentNullException(nameof(memory));
            }

            if (_memoryLocation >= memory.Length)
            {
                throw new InvalidOperationException($"The memory location specified is outside of the memory.");
            }

            return memory[_memoryLocation] == _expectedValue;
        }
    }
}