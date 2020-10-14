using System;

namespace Ninu.Emulator.Tests.Cpu.Expectations
{
    /// <summary>
    /// Asserts that a range of contiguous is set to the single expected value. All memory locations
    /// in the range must be equal to the expected value.
    /// </summary>
    public class ScalarMemoryRangeExpectation : IExpectation
    {
        private readonly Range _memoryRange;
        private readonly byte _expectedValue;

        public ScalarMemoryRangeExpectation(Range memoryRange, byte expectedValue)
        {
            _memoryRange = memoryRange;
            _expectedValue = expectedValue;
        }

        public bool AssertExpectation(byte[] memory, CpuFlags flags, byte a, byte x, byte y)
        {
            foreach (var memoryByte in memory[_memoryRange])
            {
                if (memoryByte != _expectedValue)
                {
                    return false;
                }
            }

            return true;
        }
    }
}