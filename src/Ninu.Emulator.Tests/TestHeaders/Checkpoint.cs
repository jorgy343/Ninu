using Ninu.Emulator.Tests.Cpu.Expectations;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ninu.Emulator.Tests.TestHeaders
{
    public class Checkpoint
    {
        private readonly IList<IExpectation> _expectations;

        public Checkpoint(byte identifier, IList<IExpectation> expectations)
        {
            Identifier = identifier;
            _expectations = expectations ?? throw new ArgumentNullException(nameof(expectations));
        }

        public byte Identifier { get; }

        public bool AssertExpectations(byte[] memory, CpuFlags flags, byte a, byte x, byte y)
        {
            return _expectations.All(expectation => expectation.AssertExpectation(memory, flags, a, x, y));
        }
    }
}