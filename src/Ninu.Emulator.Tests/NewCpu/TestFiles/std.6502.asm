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