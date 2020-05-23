using System;

namespace Ninu.Emulator.Tests.Cpu.Expectations
{
    public class ScalarMemoryRangeExpectation : IExpectation
    {
        private readonly Range _memoryRange;
        private readonly byte _expectedValue;

        public ScalarMemoryRangeExpectation(Range memoryRange, byte expectedValue)
        {
            _memoryRange = memoryRange;
            _expectedValue = expectedValue;
        }

        public bool AssertExpectation(byte[] memory)
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