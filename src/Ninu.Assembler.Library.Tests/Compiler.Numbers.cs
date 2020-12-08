using Xunit;

namespace Ninu.Assembler.Library.Tests
{
    public class CompilerNumbersTests
    {
        private readonly Compiler _compiler = new();

        [Theory]
        [InlineData(0, "0")]
        [InlineData(5, "5")]
        [InlineData(10, "10")]
        [InlineData(15, "15")]
        [InlineData(255, "255")]
        [InlineData(1000, "1000")]
        public void DecimalNumbers(int expectedDecimalValue, string decimalValue)
        {
            // Arrange
            var asm = $@"
                const value = {decimalValue}
                ";

            // Act
            var (_, context) = _compiler.AssembleWithContext(asm);

            // Assert
            Assert.Equal(expectedDecimalValue, context.Constants["value"]);
        }

        [Theory]
        [InlineData(0, "0")]
        [InlineData(5, "5")]
        [InlineData(10, "A")]
        [InlineData(16, "10")]
        [InlineData(15, "F")]
        [InlineData(21, "15")]
        [InlineData(255, "FF")]
        [InlineData(597, "255")]
        [InlineData(4096, "1000")]
        public void HexNumbers(int expectedDecimalValue, string hexValue)
        {
            // Arrange
            var asm = $@"
                const value = ${hexValue}
                ";

            // Act
            var (_, context) = _compiler.AssembleWithContext(asm);

            // Assert
            Assert.Equal(expectedDecimalValue, context.Constants["value"]);
        }

        [Theory]
        [InlineData(0, "0")]
        [InlineData(1, "1")]
        [InlineData(301, "100101101")]
        public void BinaryNumbers(int expectedDecimalValue, string binaryValue)
        {
            // Arrange
            var asm = $@"
                const value = %{binaryValue}
                ";

            // Act
            var (_, context) = _compiler.AssembleWithContext(asm);

            // Assert
            Assert.Equal(expectedDecimalValue, context.Constants["value"]);
        }

        [Theory]
        [InlineData(59, "%11101101 % %01011001")]
        [InlineData(59, "%11101101%%01011001")]
        public void BinaryNumbersandModuloOperator(int expectedDecimalValue, string expression)
        {
            // Arrange
            var asm = $@"
                const value = {expression}
                ";

            // Act
            var (_, context) = _compiler.AssembleWithContext(asm);

            // Assert
            Assert.Equal(expectedDecimalValue, context.Constants["value"]);
        }

        [Theory]
        [InlineData(689, "3 + 2 * 7 ** 3")]
        [InlineData(1, "3 - 686 / 7 ** 3")]
        [InlineData(24, "8 * 5 + 5 % 3 - (10 - 4) ** 2 / 2")]
        public void OrderOfOperations(int expectedDecimalValue, string expression)
        {
            // Arrange
            var asm = $@"
                const value = {expression}
                ";

            // Act
            var (_, context) = _compiler.AssembleWithContext(asm);

            // Assert
            Assert.Equal(expectedDecimalValue, context.Constants["value"]);
        }
    }
}