namespace Ninu.Emulator.Tests.Cpu.Expectations
{
    public class RegisterXExpectation : IExpectation
    {
        private readonly byte _expectedValue;

        public RegisterXExpectation(byte expectedValue)
        {
            _expectedValue = expectedValue;
        }

        public bool AssertExpectation(byte[] memory, CpuFlags flags, byte a, byte x, byte y)
        {
            return x == _expectedValue;
        }
    }
}