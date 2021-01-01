.include "..\..\..\NewCpu\TestFiles\std.6502.asm"

; Tests all of the transfer instructions. The tests all follow the same pattern. Load a value into
; the source register, load a different value into the destination register, and perform the
; transfer. The values are chosen to set specific flags.

* = $0000
.init

; TAX
lda #$00
ldx #$12    ; Sets N and clears Z.
tax         ; Should clear N and set Z.

lda #$ff
ldx #$00    ; Clears N and sets Z.
tax         ; Should set N and clear Z.

; TAY
lda #$00
ldy #$12    ; Sets N and clears Z.
tay         ; Should clear N and set Z.

lda #$ff
ldy #$00    ; Clears N and sets Z.
tay         ; Should set N and clear Z.

; TSX


; TXA
ldx #$00
lda #$12    ; Sets N and clears Z.
txa         ; Should clear N and set Z.

ldx #$ff
lda #$00    ; Clears N and sets Z.
txa         ; Should set N and clear Z.

; TXS


; TYA
ldy #$00
lda #$12    ; Sets N and clears Z.
tya         ; Should clear N and set Z.

ldy #$ff
lda #$00    ; Clears N and sets Z.
tya         ; Should set N and clear Z.

.done

.vectors