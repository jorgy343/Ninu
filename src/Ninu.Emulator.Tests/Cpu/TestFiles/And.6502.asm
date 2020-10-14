; #Checkpoint 01
; [a000:a0ff] == 00

; #Checkpoint 02
; [a100:a1ff] == 00 .. ff

.include "..\..\..\Cpu\TestFiles\std.6502.asm"

*= $0000

; Test 1
; AND all numbers 00 through ff against 00 and store the results at $a000 through $a0ff. All
; addresses from $a000 through $a0ff should be 00.
ldx #$00                ; Loop counter.
testOneLoop:
    txa                 ; Move loop counter to A.
    and #$00            ; Perform the test.
    sta $a000,x         ; Store the result in memory.

    inx
    bne testOneLoop

.checkpoint $01

; Test 2
; AND all numbers 00 through ff against ff and store the results at $a100 through $a1ff. All
; addresses from $a100 through $a1ff should be numbers starting at 00 and incrementing by one for
; each memory address. For example, $a100 should be 00, $a101 should be 01, and $a102 should be 02.
ldx #$00                ; Loop counter.
testTwoLoop:
    txa                 ; Move loop counter to A.
    and #$ff            ; Perform the test.
    sta $a100,x         ; Store the result in memory.

    inx
    bne testTwoLoop

.checkpoint $02

.done
.vectors