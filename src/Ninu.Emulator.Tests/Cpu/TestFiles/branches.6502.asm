.include "..\..\..\Cpu\TestFiles\std.6502.asm"

; Zero page data.
* = $0000
.byte $00

; Beginning of tests.
* = $c0c0
.init

test1:
    ldx #$01
    dex     ; Set zero flag.

    bne @skip1
    beq @skip1
    inx
    jmp @skip1

    * = $c0e0
    @skip1:

    ldx #$01
    dex     ; Set zero flag.

    bne @skip2
    beq @skip2
    inx
    jmp @skip2

    * = $c110
    @skip2:
    jmp test2

* = $c1c0
test2:
    ldx #$01 ; clear zero flag.

    beq @skip1
    bne @skip1
    inx
    jmp @skip1

    * = $c1e0
    @skip1:

    ldx #$01 ; clear zero flag.

    beq @skip2
    bne @skip2
    inx
    jmp @skip2

    * = $c210
    @skip2:
    jmp test3

* = $c2c0
test3:
    sec     ; set carry flag

    bcc @skip1
    bcs @skip1
    inx
    jmp @skip1

    * = $c2e0
    @skip1:

    sec     ; set carry flag

    bcc @skip2
    bcs @skip2
    inx
    jmp @skip2

    * = $c310
    @skip2:
    jmp test4

* = $c3c0
test4:
    clc     ; clear carry flag

    bcs @skip1
    bcc @skip1
    inx
    jmp @skip1

    * = $c3e0
    @skip1:

    clc     ; clear carry flag

    bcs @skip2
    bcc @skip2
    inx
    jmp @skip2

    * = $c410
    @skip2:
    jmp finish

finish:
.done

* = $fff0
rti

* = $fffa
nmiVector   .addr $fff0
resetVector .addr $c0c0
irqVector   .addr $fff0