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

                ldx #$ff    ; 01 - 02
                txs         ; 03 - 04

                ldx #$12    ; 05 - 06
                inx         ; 07 - 08
                inx         ; 09 - 10
                inx         ; 11 - 12
                inx         ; 13 - 14

                .done

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

            for (var i = 0; i < 500; i++)
            {
                cycle++;

                if (cycle == 7)
                {
                    simulator.WriteBit("nmi", false);
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