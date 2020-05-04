using Patcher6502;
using Xunit;

namespace Ninu.Visual6502.Tests
{
    public class BasicTests
    {
        /// <summary>
        /// Tests the state of the nodes and transistors match the JavaScript version of Visual 6502.
        /// </summary>
        [Fact]
        public void Initialization()
        {
            var simulator = new Simulator();

            simulator.Init();

            // These hashes were taken from the JavaScript version of the Visual 6502.
            Assert.Equal(650693342, simulator.ComputeNodeHash());
            Assert.Equal(699598249, simulator.ComputeTransistorHash());
        }

        /// <summary>
        /// Tests that loading the A, X, and Y registers works as expected.
        /// </summary>
        [Fact]
        public void LoadRegisters()
        {
            var simulator = new Simulator();

            var assembler = new PatchAssembler();

            var asm = @"
                .org $0000

                lda #$31
                ldx #$32
                ldy #$33

                loop: jmp loop

                .org $f000
                rti

                .org $fffa
                nmi .addr $f000

                .org $fffc
                reset .addr $0000

                .org $fffe
                irq .addr $f000
            ".Replace(".org", "* =");

            var data = assembler.Assemble(0, null, asm);

            simulator.SetMemory(data);

            simulator.Init();
            simulator.RunStartProgram();

            simulator.ExecuteCycles(20);

            Assert.Equal(0x31, simulator.ReadA());
            Assert.Equal(0x32, simulator.ReadX());
            Assert.Equal(0x33, simulator.ReadY());
        }
    }
}