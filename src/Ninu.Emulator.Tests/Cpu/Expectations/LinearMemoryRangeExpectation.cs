using System;

namespace Ninu.Emulator.Tests.Cpu.Expectations
{
    public class LinearMemoryRangeExpectation : IExpectation
    {
        private readonly Range _memoryRange;

        private readonly byte _startValue;
        private readonly byte _endValue;

        public LinearMemoryRangeExpectation(Range memoryRange, byte startValue, byte endValue)
        {
            _memoryRange = memoryRange;

            _startValue = startValue;
            _endValue = endValue;
        }

        public bool AssertExpectation(byte[] memory)
        {
            var currentExpectedValue = _startValue;

            foreach (var memoryByte in memory[_memoryRange])
            {
                if (memoryByte != currentExpectedValue)
                {
                    return false;
                }

                currentExpectedValue++;

                // Add one because the end value is inclusive and will be compared during the next loop.
                if (currentExpectedValue == _endValue + 1)
                {
                    currentExpectedValue = _startValue;
                }
            }

            return true;
        }
    }
}