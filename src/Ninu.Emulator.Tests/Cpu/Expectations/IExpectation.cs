using Ninu.Emulator.CentralProcessor;

namespace Ninu.Emulator.Tests.Cpu.Expectations
{
    /// <summary>
    /// Represents an assertion of the memory. This requires testing one or more memory
    /// locations to see if their value(s) are the expected values.
    /// </summary>
    public interface IExpectation
    {
        /// <summary>
        /// Asserts that one or more memory locations are set to their expected value(s).
        /// </summary>
        /// <param name="memory">The memory to test the assertions against.</param>
        /// <param name="flags">The value of the flags register taken from the stack before it was tampered with by the checkpoint code.</param>
        /// <param name="a">The value of the accumulator register taken from the stack before it was tampered with by the checkpoint code.</param>
        /// <param name="x">The current value of the x register taken from the CPU state.</param>
        /// <param name="y">The current value of the y register taken from the CPU state.</param>
        /// <returns><c>true</c> if the assertion passes; otherwise, <c>false</c>.</returns>
        bool AssertExpectation(byte[] memory, CpuFlags flags, byte a, byte x, byte y);
    }
}