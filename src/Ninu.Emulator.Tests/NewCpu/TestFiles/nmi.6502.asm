.include "..\..\..\NewCpu\TestFiles\std.6502.asm"

* = $0000
            ; cycles
ldx #$ab    ; 01 - 02
ldx #$ab    ; 03 - 04
inx         ; 05 - 06
inx         ; 07 - 08
iny         ; 09 - 10
iny         ; 11 - 12

brk         ; 13 - 19
nop ; BRK pushes PC + 1 to the stack so RTI will actually return the instruction just after this NOP.

.done

; NMI handler.
* = $1200
sed ; The D flag should be unset when entering the NMI. Set it to test to ensure that P is loaded correctly after RTI.
rti

* = $fff0
rti

* = $fffa
nmi .addr $1200
reset .addr $0000
irq .addr $fff0