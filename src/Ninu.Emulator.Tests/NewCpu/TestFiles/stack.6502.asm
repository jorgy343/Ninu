.include "..\..\..\NewCpu\TestFiles\std.6502.asm"

* = $0000
.byte $00

; Beginning of tests.
* = $c000
.init

; Basic test of pha and pla.
lda #$37
pha

lda #$00
pla

; Basic test of php and plp.
lda #$00
php

lda #$ff
plp

.done

* = $fff0
rti

* = $fffa
nmiVector   .addr $fff0
resetVector .addr $c000
irqVector   .addr $fff0