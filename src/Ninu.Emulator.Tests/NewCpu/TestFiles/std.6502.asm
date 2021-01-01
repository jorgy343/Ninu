vectors .macro

    * = $fff0
    rti

    * = $fffa
    nmi .addr $fff0
    reset .addr $0000
    irq .addr $fff0

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