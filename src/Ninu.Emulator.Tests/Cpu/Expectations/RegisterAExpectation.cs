using Ninu.Emulator.CentralProcessor;

namespace Ninu.Emulator.Tests.Cpu.Expectations
{
    public class RegisterAExpectation : IExpectation
    {
        private readonly byte _expectedValue;

        public RegisterAExpectation(byte expectedValue)
        {
            _expectedValue = expectedValue;
        }

        public bool AssertExpectation(byte[] memory, CpuFlags flags, byte a, byte x, byte y)
        {
            return a == _expectedValue;
        }
    }
}