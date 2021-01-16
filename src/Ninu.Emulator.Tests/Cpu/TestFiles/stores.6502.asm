.include "..\..\..\Cpu\TestFiles\std.6502.asm"

; These tests do not currently purposefully test flags. All tests that have offset addressing modes
; are designed to wrap around to a new page to test buggy wrapping behavior where applicable.

* = $0000
.byte $00 ; Tell the assembler to start assembling at $0000.

; sta indirect zero page with x offset - test 1
* = $72
.addr $2345

; sta indirect zero page with y offset
* = $d0
.addr $44ff

; sta indirect zero page with x offset - test 2
* = $ff
.addr $3456

; Start program at 0x1000 so we can freely store data in zero page.
* = $1000
.init

; Data setup.
lda #$a1
ldx #$a2
ldy #$a3

; sta
sta $d0     ; zero page
sta $d0,x   ; zero page with x offset
sta $a0d0   ; absolute
sta $a0d0,x ; absolute with x offset
sta $a0d0,y ; absolute with y offset
sta ($d0,x) ; indirect zero page with x offset - test 1
sta ($5d,x) ; indirect zero page with x offset - test 2
sta ($d0),y ; indirect zero page with y offset

; stx
stx $b0     ; zero page
stx $b0,y   ; zero page with x offset
stx $b0e0   ; absolute

; sty
sty $c0     ; zero page
sty $c0,x   ; zero page with x offset
sty $c0f0   ; absolute

.done

* = $fff0
rti

* = $fffa
nmiVector   .addr $fff0
resetVector .addr $1000
irqVector   .addr $fff0