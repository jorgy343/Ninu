using Ninu.Emulator.Tests.Cpu.Expectations;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ninu.Emulator.Tests.TestHeaders
{
    public class Checkpoint
    {
        private readonly List<IExpectation> _expectations;

        public int Number { get; }

        public Checkpoint(int number, List<IExpectation> expectations)
        {
            Number = number;
            _expectations = expectations ?? throw new ArgumentNullException(nameof(expectations));
        }

        public bool AssertExpectations(byte[] memory)
        {
            return _expectations.All(expectation => expectation.AssertExpectation(memory));
        }
    }
}