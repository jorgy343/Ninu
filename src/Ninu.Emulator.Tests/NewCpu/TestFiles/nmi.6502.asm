.include "..\..\..\NewCpu\TestFiles\std.6502.asm"

* = $0000
            ; cycles
ldx #$ff    ; 01 - 02
txs         ; 03 - 04

ldx #$12    ; 05 - 06
inx         ; 07 - 08
inx         ; 09 - 10
inx         ; 11 - 12
inx         ; 13 - 14

.done

; NMI handler.
* = $1200
rti

* = $fffa
nmi .addr $1200
reset .addr $0000
irq .addr $fff0