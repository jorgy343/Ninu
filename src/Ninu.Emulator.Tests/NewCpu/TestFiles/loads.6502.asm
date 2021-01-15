.include "..\..\..\NewCpu\TestFiles\std.6502.asm"

; Zero page data.
* = $0000
.byte $17   ; Used to test wrapping of zero page addressing modes.

* = $0020
.byte $30   ; Basic zero page data.
.byte $00
.byte $ff

* = $0050
.byte $77   ; Used to test wrapping around the entire address space for absolute with x/y offset.

* = $0080
.addr $1020 ; Used for indirect zero page with x/y offset addressing mode.

; Absolute address data (above zero page).
* = $1000
.byte $00   ; Not used for anything right now.

* = $1020
.byte $40
.byte $00
.byte $ff

* = $1100   ; Used to test crossing page boundary in absolute with x/y offset addressing modes (causes an aditional cycle for reads).
.byte $27

; Beginning of tests.
* = $c000
.init

; ***************
; ***** LDA *****
; ***************
;
; immediate
lda #$33
lda #$00
lda #$ff

; zero page
lda $20
lda $21
lda $22

; zero page with x offset
ldx #$10
lda $10,x

ldx #$11
lda $10,x

ldx #$12
lda $10,x

ldx #$01
lda $ff,x ; This should wrap around to address $0000 instead of accessing $0100.

; absolute
lda $1020
lda $1021
lda $1022

; absolute with x offset
ldx #$10
lda $1010,x

ldx #$11
lda $1010,x

ldx #$12
lda $1010,x

ldx #$01
lda $10ff,x ; This will cause the read to cross page boundaries thus causing an additional cycle.

ldx #$55
lda $ffff,x ; This will cause the read to wrap around the entire address space back to $0050.

; absolute with y offset
ldy #$10
lda $1010,y

ldy #$11
lda $1010,y

ldy #$12
lda $1010,y

ldy #$01
lda $10ff,y ; This will cause the read to cross page boundaries thus causing an additional cycle.

ldy #$55
lda $ffff,y ; This will cause the read to wrap around the entire address space back to $0050.

; indirect zero page with x offset
; NOTE: Flags and the wrapping behavior are not fully tested.
ldx $20
lda ($30,x) ; This will pull the address from $0050 ($20 + $30) which is $1020 and then pull the data from that location.

; indirect zero page with y offset
; NOTE: Flags and the wrapping behavior are not fully tested. However, 5 cycles vs 6 cycles are tested.
ldy #$02
lda ($80),y ; This will pull the address from $0080 which is $1020 and then add $02 to get the data at address $1022 which will cost 5 cycles.

ldy #$e0
lda ($80),y ; This will pull the address from $0080 which is $1020 and then add $e0 to get the data at address $1100 which will cost 6 cycles instead of 5.

; ***************
; ***** LDX *****
; ***************
;
; immediate
ldx #$33
ldx #$00
ldx #$ff

; zero page
ldx $20
ldx $21
ldx $22

; zero page with y offset
ldy #$10
ldx $10,y

ldy #$11
ldx $10,y

ldy #$12
ldx $10,y

ldy #$01
ldx $ff,y ; This should wrap around to address $0000 instead of accessing $0100.

; absolute
ldx $1020
ldx $1021
ldx $1022

; absolute with y offset
ldy #$10
ldx $1010,y

ldy #$11
ldx $1010,y

ldy #$12
ldx $1010,y

ldy #$01
ldx $10ff,y ; This will cause the read to cross page boundaries thus causing an additional cycle.

ldy #$55
ldx $ffff,y ; This will cause the read to wrap around the entire address space back to $0050.

; ***************
; ***** LDY *****
; ***************
;
; immediate
ldy #$33
ldy #$00
ldy #$ff

; zero page
ldy $20
ldy $21
ldy $22

; zero page with x offset
ldx #$10
ldy $10,x

ldx #$11
ldy $10,x

ldx #$12
ldy $10,x

ldx #$01
ldy $ff,x ; This should wrap around to address $0000 instead of accessing $0100.

; absolute
ldy $1020
ldy $1021
ldy $1022

; absolute with x offset
ldx #$10
ldy $1010,x

ldx #$11
ldy $1010,x

ldx #$12
ldy $1010,x

ldx #$01
ldy $10ff,x ; This will cause the read to cross page boundaries thus causing an additional cycle.

ldx #$55
ldy $ffff,x ; This will cause the read to wrap around the entire address space back to $0050.

; Done.
.done

* = $fff0
rti

* = $fffa
nmiVector   .addr $fff0
resetVector .addr $c000
irqVector   .addr $fff0