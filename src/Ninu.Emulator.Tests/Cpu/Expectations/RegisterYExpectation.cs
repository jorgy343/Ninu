namespace Ninu.Emulator.Tests.Cpu.Expectations
{
    public class RegisterYExpectation : IExpectation
    {
        private readonly byte _expectedValue;

        public RegisterYExpectation(byte expectedValue)
        {
            _expectedValue = expectedValue;
        }

        public bool AssertExpectation(byte[] memory, CpuFlags flags, byte a, byte x, byte y)
        {
            return y == _expectedValue;
        }
    }
}