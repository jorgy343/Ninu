using System;

namespace Ninu.Emulator.Tests.Cpu.Expectations
{
    /// <summary>
    /// Asserts that a single memory location is the expected value.
    /// </summary>
    public class SingleMemoryExpectation : IExpectation
    {
        private readonly int _memoryLocation;
        private readonly byte _expectedValue;

        public SingleMemoryExpectation(int memoryLocation, byte expectedValue)
        {
            _memoryLocation = memoryLocation;
            _expectedValue = expectedValue;
        }

        public bool AssertExpectation(byte[] memory, CpuFlags flags, byte a, byte x, byte y)
        {
            if (memory is null)
            {
                throw new ArgumentNullException(nameof(memory));
            }

            if (_memoryLocation < 0 || _memoryLocation >= memory.Length)
            {
                throw new InvalidOperationException($"The memory location specified is outside of the memory.");
            }

            return memory[_memoryLocation] == _expectedValue;
        }
    }
}