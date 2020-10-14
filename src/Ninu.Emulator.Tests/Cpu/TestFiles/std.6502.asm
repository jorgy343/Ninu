vectors .macro

    *= $fff0
    rti

    *= $fffa
    nmi .addr $fff0
    reset .addr $0000
    irq .addr $fff0

.endmacro

done .macro

    lda #$a3                ; Show that the test ran to the end.
    sta $ff00
    loop: jmp loop

.endmacro

checkpoint .macro checkpointNumber

    php
    pha

    lda #\checkpointNumber
    sta $ff01

    pla
    plp

.endmacro