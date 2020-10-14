namespace Ninu.Emulator.Tests.Cpu.Expectations
{
    public class RegisterPExpectation : IExpectation
    {
        private readonly byte _expectedValue;

        public RegisterPExpectation(byte expectedValue)
        {
            _expectedValue = expectedValue;
        }

        public RegisterPExpectation(CpuFlags expectedValue)
        {
            _expectedValue = (byte)expectedValue;
        }

        public bool AssertExpectation(byte[] memory, CpuFlags flags, byte a, byte x, byte y)
        {
            return y == _expectedValue;
        }
    }
}