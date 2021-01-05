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

; ADC and some other instructions appear to be double pipelined. This is just a simple test to
; ensure that the value in the A register is available for the next instruction in time. Probably a
; pointless test that does nothing.
;lda #$01
;adc #$02
;and #$01

; ***************
; ***** ADC *****
; ***************
;
; immediate
lda #$a0
adc #$22

; zero page
lda #$f0
adc $20

; zero page with x offset
lda #$a0
ldx #$10
lda #$a0
adc $10,x

; absolute
lda #$a0
adc $1020

; absolute with x offset
lda #$a0
ldx #$10
adc $1010,x

; absolute with y offset
lda #$a0
ldy #$10
adc $1010,y

; indirect zero page with x offset
lda #$a0
ldx $20
adc ($30,x) ; This will pull the address from $0050 ($20 + $30) which is $1020 and then pull the data from that location.

; indirect zero page with y offset
lda #$a0
ldy #$02
adc ($80),y ; This will pull the address from $0080 which is $1020 and then add $02 to get the data at address $1022 which will cost 5 cycles.

lda #$a0
ldy #$e0
adc ($80),y ; This will pull the address from $0080 which is $1020 and then add $e0 to get the data at address $1100 which will cost 6 cycles instead of 5.

; ***************
; ***** AND *****
; ***************
;
; immediate
lda #$a0
and #$22

;; zero page
;lda #$f0
;and $20
;
;; zero page with x offset
;lda #$a0
;ldx #$10
;lda #$a0
;and $10,x
;
;; absolute
;lda #$a0
;and $1020
;
;; absolute with x offset
;lda #$a0
;ldx #$10
;and $1010,x
;
;; absolute with y offset
;lda #$a0
;ldy #$10
;and $1010,y
;
;; indirect zero page with x offset
;lda #$a0
;ldx $20
;and ($30,x) ; This will pull the address from $0050 ($20 + $30) which is $1020 and then pull the data from that location.
;
;; indirect zero page with y offset
;lda #$a0
;ldy #$02
;and ($80),y ; This will pull the address from $0080 which is $1020 and then add $02 to get the data at address $1022 which will cost 5 cycles.
;
;lda #$a0
;ldy #$e0
;and ($80),y ; This will pull the address from $0080 which is $1020 and then add $e0 to get the data at address $1100 which will cost 6 cycles instead of 5.

.done

* = $fff0
rti

* = $fffa
nmiVector   .addr $fff0
resetVector .addr $c000
irqVector   .addr $fff0