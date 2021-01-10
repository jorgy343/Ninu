.include "..\..\..\NewCpu\TestFiles\std.6502.asm"

; Zero page data.
* = $0000
.byte $17   ; Used to test wrapping of zero page addressing modes.

; Basic zero page data.
* = $0020
zeroPage_30 .byte $30
zeroPage_00 .byte $00
zeroPage_ff .byte $ff

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
lda #$01
adc #$02
and #$01

; ***************
; ***** ADC *****
; ***************
;
; set zero flag
lda #$00
ldx #$00    ; Clear zero flag.
adc #$00    ; 00 + 00 = 00; Set zero flag.

; clear zero flag
lda #$00    ; Set zero flag.
adc #$01    ; 00 + 01 = 01; Clear zero flag.

; set negative flag
lda #$00    ; Clear negative flag.
adc #$f0    ; 00 + f0 = f0; Set negative flag.

; set z, clear n
lda #$f0    ; clear z, set n
adc #$10    ; f0 + 10 = 00; set z, clear n

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
; ***** SBC *****
; ***************
;
; immediate
lda #$a0
sbc #$22

; zero page
lda #$f0
sbc $20

; zero page with x offset
lda #$a0
ldx #$10
lda #$a0
sbc $10,x

; absolute
lda #$a0
sbc $1020

; absolute with x offset
lda #$a0
ldx #$10
sbc $1010,x

; absolute with y offset
lda #$a0
ldy #$10
sbc $1010,y

; indirect zero page with x offset
lda #$a0
ldx $20
sbc ($30,x) ; This will pull the address from $0050 ($20 + $30) which is $1020 and then pull the data from that location.

; indirect zero page with y offset
lda #$a0
ldy #$02
sbc ($80),y ; This will pull the address from $0080 which is $1020 and then add $02 to get the data at address $1022 which will cost 5 cycles.

lda #$a0
ldy #$e0
sbc ($80),y ; This will pull the address from $0080 which is $1020 and then add $e0 to get the data at address $1100 which will cost 6 cycles instead of 5.

; ***************
; ***** AND *****
; ***************
;
; immediate
lda #$a0
and #$22

; zero page
lda #$f0
and $20

; zero page with x offset
lda #$a0
ldx #$10
lda #$a0
and $10,x

; absolute
lda #$a0
and $1020

; absolute with x offset
lda #$a0
ldx #$10
and $1010,x

; absolute with y offset
lda #$a0
ldy #$10
and $1010,y

; indirect zero page with x offset
lda #$a0
ldx $20
and ($30,x) ; This will pull the address from $0050 ($20 + $30) which is $1020 and then pull the data from that location.

; indirect zero page with y offset
lda #$a0
ldy #$02
and ($80),y ; This will pull the address from $0080 which is $1020 and then add $02 to get the data at address $1022 which will cost 5 cycles.

lda #$a0
ldy #$e0
and ($80),y ; This will pull the address from $0080 which is $1020 and then add $e0 to get the data at address $1100 which will cost 6 cycles instead of 5.

; ***************
; ***** EOR *****
; ***************
;
; immediate
lda #$a0
eor #$22

; zero page
lda #$f0
eor $20

; zero page with x offset
lda #$a0
ldx #$10
lda #$a0
eor $10,x

; absolute
lda #$a0
eor $1020

; absolute with x offset
lda #$a0
ldx #$10
eor $1010,x

; absolute with y offset
lda #$a0
ldy #$10
eor $1010,y

; indirect zero page with x offset
lda #$a0
ldx $20
eor ($30,x) ; This will pull the address from $0050 ($20 + $30) which is $1020 and then pull the data from that location.

; indirect zero page with y offset
lda #$a0
ldy #$02
eor ($80),y ; This will pull the address from $0080 which is $1020 and then add $02 to get the data at address $1022 which will cost 5 cycles.

lda #$a0
ldy #$e0
eor ($80),y ; This will pull the address from $0080 which is $1020 and then add $e0 to get the data at address $1100 which will cost 6 cycles instead of 5.

; ***************
; ***** ORA *****
; ***************
;
; immediate
lda #$a0
ora #$22

; zero page
lda #$f0
ora $20

; zero page with x offset
lda #$a0
ldx #$10
lda #$a0
ora $10,x

; absolute
lda #$a0
ora $1020

; absolute with x offset
lda #$a0
ldx #$10
ora $1010,x

; absolute with y offset
lda #$a0
ldy #$10
ora $1010,y

; indirect zero page with x offset
lda #$a0
ldx $20
ora ($30,x) ; This will pull the address from $0050 ($20 + $30) which is $1020 and then pull the data from that location.

; indirect zero page with y offset
lda #$a0
ldy #$02
ora ($80),y ; This will pull the address from $0080 which is $1020 and then add $02 to get the data at address $1022 which will cost 5 cycles.

lda #$a0
ldy #$e0
ora ($80),y ; This will pull the address from $0080 which is $1020 and then add $e0 to get the data at address $1100 which will cost 6 cycles instead of 5.

; ***************
; ***** CMP *****
; ***************
;
; immediate
lda #$a0
cmp #$22

; zero page
lda #$f0
cmp $20

; zero page with x offset
lda #$a0
ldx #$10
lda #$a0
cmp $10,x

; absolute
lda #$a0
cmp $1020

; absolute with x offset
lda #$a0
ldx #$10
cmp $1010,x

; absolute with y offset
lda #$a0
ldy #$10
cmp $1010,y

; indirect zero page with x offset
lda #$a0
ldx $20
cmp ($30,x) ; This will pull the address from $0050 ($20 + $30) which is $1020 and then pull the data from that location.

; indirect zero page with y offset
lda #$a0
ldy #$02
cmp ($80),y ; This will pull the address from $0080 which is $1020 and then add $02 to get the data at address $1022 which will cost 5 cycles.

lda #$a0
ldy #$e0
cmp ($80),y ; This will pull the address from $0080 which is $1020 and then add $e0 to get the data at address $1100 which will cost 6 cycles instead of 5.

; ***************
; ***** CPX *****
; ***************
;
; immediate
lda #$a0
cpx #$22

; zero page
lda #$f0
cpx $20

; absolute
lda #$a0
cpx $1020

; ***************
; ***** CPY *****
; ***************
;
; immediate
lda #$a0
cpy #$22

; zero page
lda #$f0
cpy $20

; absolute
lda #$a0
cpy $1020

.done

* = $fff0
rti

* = $fffa
nmiVector   .addr $fff0
resetVector .addr $c000
irqVector   .addr $fff0