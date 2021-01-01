.include "..\..\..\NewCpu\TestFiles\std.6502.asm"

* = $0000

; Set registers immediate tests.
lda #$13
ldx #$ab
ldy #$cd

.done

.vectors