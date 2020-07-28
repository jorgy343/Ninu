; #Init
; [00f5] := a3
; [120b:120e] := 10 00 fd e2

; #Checkpoint 01
; [a000] == 2e
; [a001:a099] == ae 00 28
;
; [a000] := ff
; [a001:a003] := ae 00 28
; [b000:b0ff] := 00 .. ff

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