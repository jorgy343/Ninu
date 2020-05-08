*= $0000

; Test 1
ldx #$00                ; Loop counter.
testOneLoop:
    txa                 ; Move loop counter to A.
    and #$00            ; Perform the test.
    sta $a000,x         ; Store the result in memory.

    inx
    bne testOneLoop

; Test 2
ldx #$00                ; Loop counter.
testTwoLoop:
    txa                 ; Move loop counter to A.
    and #$ff            ; Perform the test.
    sta $a100,x         ; Store the result in memory.

    inx
    bne testTwoLoop

; Done
lda #$a3                ; Show that the test ran to the end.
sta $fd00
loop: jmp loop

; Setup the vectors.
*= $ff00
rti

*= $fffa
nmi .addr $ff00
reset .addr $0000
irq .addr $ff00