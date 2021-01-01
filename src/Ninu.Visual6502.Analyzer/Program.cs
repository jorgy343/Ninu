using Ninu.Base;
using Patcher6502;
using static System.Console;

namespace Ninu.Visual6502.Analyzer
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var asm = @"
                vectors .macro

                    * = $fff0
                    rti

                    * = $fffa
                    nmi .addr $fff0
                    reset .addr $0000
                    irq .addr $fff0

                .endmacro

                init .macro

                    ; Set the interrupt disable bit to prevent interrupts.
                    sei

                    ; Clear the decimal bit. Decimal mode is disabled in the NES so this should cause CPU emulators
                    ; to mimic that behavior.
                    cld

                    ; Load 0xff into the stack register.
                    ldx #$ff
                    txs

                .endmacro

                done .macro

                    ; Show that the test ran to the end by storing 0xa3 in 0xff00.
                    lda #$a3
                    sta $ff00
                    doneLoop: jmp doneLoop

                .endmacro

                error .macro

                    ; Show that the test ran to the end by storing 0xa3 in 0xff00.
                    lda #$c9
                    sta $ff00
                    doneLoop: jmp doneLoop

                .endmacro

                * = $0000
                ldx #$ab
                inx

                ; If the emulator accounts for the bug, the low byte will be read from 0x12ff and the high byte
                ; from 0x1200.
                jmp ($12ff)

                * = $1200
                .byte $56 ; High byte of the target address.

                * = $12ff
                .byte $12 ; Low byte of the target address.

                * = $1300
                .byte $89 ; If the emulator is correct, this byte won't be read due to the bug.

                ; The target address should be 0x5612 instead of what the 0x8912 that you would expect if this bug
                ; didn't exist.
                * = $5612
                .done

                ; If the emulator is correct, this shouldn't be hit.
                * = $8912
                .error

                .vectors
            ";

            var assembler = new PatchAssembler();
            var simulationMemory = assembler.Assemble(0, null, asm);

            var memory = new ArrayMemory(simulationMemory);

            var simulator = new Simulator(memory);

            void WriteDataLine(int cycle)
            {
                Write($"{cycle:00000} {simulator.ReadAddressBus():x4} {simulator.ReadBits8("db"):x2} {simulator.ReadPC():x4} ");
                Write($"{simulator.ReadA():x2} {simulator.ReadX():x2} {simulator.ReadY():x2} {simulator.ReadS():x2} ");
                Write($"{simulator.ReadBits8("ir"):x2}  {simulator.ReadBit("sync")}   {simulator.ReadBit("rw")} ");
                Write($"{simulator.ReadPString()} ");

                WriteLine();
            }

            WriteLine("cycle  ab  db  pc  a  x  y  s  ir sync rw   p");

            var cycle = 1;

            simulator.Init(() => WriteDataLine(cycle++));
            WriteLine("--------------");

            cycle = 1;

            simulator.RunStartProgram(() => WriteDataLine(cycle++));
            WriteLine("--------------");

            cycle = 1;

            for (var i = 0; i < 500; i++)
            {
                simulator.Clock();
                WriteDataLine(cycle++);

                if (memory[0xff00] == 0xa3)
                {
                    break;
                }
            }
        }
    }
}