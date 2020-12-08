using Xunit;

namespace Ninu.Assembler.Library.Tests
{
    public class CompilerAddressingModesTests
    {
        private readonly Compiler _compiler = new();

        [Fact]
        public void UsingLabelsBeforeDeclaration_ZeroPageLabel_ResultsInAbsoluteMode()
        {
            // Arrange
            var asm = $@"
                lda label
                origin(20) ; Make sure the label is within the ZP.
                label:
                ";

            // Act
            var (data, context) = _compiler.AssembleWithContext(asm);

            // Assert
            Assert.Equal(20, context.Labels["label"]);

            Assert.Equal(3, data.Length);

            Assert.Equal(0xad, data[0]); // opcode
            Assert.Equal(20, data[1]); // Low byte of label address.
            Assert.Equal(0, data[2]); // High byte of label address.
        }
    }
}