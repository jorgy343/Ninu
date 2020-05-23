namespace Ninu.Emulator.Tests.Cpu.Expectations
{
    public interface IExpectation
    {
        bool AssertExpectation(byte[] memory);
    }
}