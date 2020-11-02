using Ninu.Emulator.CentralProcessor;
using System;
using System.Collections.Generic;

namespace Ninu.Emulator.Tests.Cpu.Expectations
{
    public class CollectionMemoryRangeExpectation : IExpectation
    {
        private readonly Range _memoryRange;
        private readonly IList<byte> _expectedValues;

        public CollectionMemoryRangeExpectation(Range memoryRange, IList<byte> expectedValues)
        {
            _memoryRange = memoryRange;
            _expectedValues = expectedValues ?? throw new ArgumentNullException(nameof(expectedValues));

            if (expectedValues.Count == 0)
            {
                throw new ArgumentException($"The {nameof(expectedValues)} argument must have at least one item.", nameof(expectedValues));
            }
        }

        public bool AssertExpectation(byte[] memory, CpuFlags flags, byte a, byte x, byte y)
        {
            var index = 0;

            foreach (var memoryByte in memory[_memoryRange])
            {
                if (memoryByte != _expectedValues[index])
                {
                    return false;
                }

                index++;

                if (index >= _expectedValues.Count)
                {
                    index = 0;
                }
            }

            return true;
        }
    }
}