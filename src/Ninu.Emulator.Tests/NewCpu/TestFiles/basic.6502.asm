.include "..\..\..\NewCpu\TestFiles\std.6502.asm"

* = $0000

; Random simple instructions
nop
nop

; Set registers immediate tests.
lda #$13
ldx #$ab
ldy #$cd

; Test incrementing and decrementing. The X and Y registers are set to something from above.
inx
inx
dex
dex

iny
iny
dey
dey

; Increment when at 0xff and then decrement at 0x00
ldx #$ff
inx ; Brings x to 0x00. Tests setting the Z flag.
dex ; Brings x to 0xff. Tests clearing the Z flag and setting the N flag.
inx ; Brings x to 0x00. Tests setting the Z flag and clearing the N flag.

ldy #$ff
iny ; Brings y to 0x00. Tests setting the Z flag.
dey ; Brings y to 0xff. Tests clearing the Z flag and setting the N flag.
iny ; Brings y to 0x00. Tests setting the Z flag and clearing the N flag.

.done

.vectors