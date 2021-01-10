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
                    sed
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

; Zero page data.
* = $0000
.byte $17   ; Used to test wrapping of zero page addressing modes.

* = $0020
.byte $30   ; Basic zero page data.
.byte $00
.byte $ff

* = $0050
.byte $77   ; Used to test wrapping around the entire address space for absolute with x/y offset.

* = $0080
.addr $1020 ; Used for indirect zero page with x/y offset addressing mode.

; Absolute address data (above zero page).
* = $1000
.byte $00   ; Not used for anything right now.

* = $1020
.byte $40
.byte $00
.byte $ff

* = $1100   ; Used to test crossing page boundary in absolute with x/y offset addressing modes (causes an aditional cycle for reads).
.byte $27

; Beginning of tests.
* = $c000
lda #$f1
ldx #$00
;nop
and #$f1

.done

* = $fff0
rti

* = $fffa
nmiVector   .addr $fff0
resetVector .addr $c000
irqVector   .addr $fff0
            ";

            var assembler = new PatchAssembler();
            var simulationMemory = assembler.Assemble(0, null, asm);

            var memory = new ArrayMemory(simulationMemory);

            var simulator = new Simulator(memory);

            void WriteDataLine(int cycle)
            {
                Write($"{cycle:00000} {simulator.ReadAddressBus():x4} {simulator.ReadBits8("db"):x2} {simulator.ReadPC():x4} ");
                Write($"{simulator.ReadA():x2} {simulator.ReadX():x2} {simulator.ReadY():x2} {simulator.ReadS():x2} ");
                Write($"{simulator.ReadBits8("ir"):x2}  {simulator.ReadBit("sync")}   {simulator.ReadBit("rw")}   ");
                Write($"{simulator.ReadBit("nmi")}  {simulator.ReadPString()} ");

                WriteLine();
            }

            WriteLine("cycle  ab  db  pc  a  x  y  s  ir sync rw nmi    p");

            var cycle = 1;

            simulator.Init(() => WriteDataLine(cycle++));
            WriteLine("--------------");

            cycle = 1;

            simulator.RunStartProgram(() => WriteDataLine(cycle++));
            WriteLine("--------------");

            cycle = 0;

            for (var i = 0; i < 1000; i++)
            {
                cycle++;

                if (cycle == 5)
                {
                    //simulator.WriteBit("nmi", false);
                }

                simulator.Clock();
                WriteDataLine(cycle);

                if (memory[0xff00] == 0xa3)
                {
                    break;
                }
            }
        }
    }
}