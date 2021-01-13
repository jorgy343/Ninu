.include "..\..\..\NewCpu\TestFiles\std.6502.asm"

* = $0000
.init

; Random instructions.
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

; BIT
bit $00
bit $1000



; ASL A
lda #$91
asl a

; ASL absolute
lda #$93
sta $5000

asl $5000
lda $5000

; ASL absolute with x offset
ldx #$01
lda #$95
sta $5000,x

asl $5000,x
lda $5000,x

; ASL zero page
lda #$97
sta $fe

asl $fe
lda $fe

; ASL zero page with x offset
ldx #$01
lda #$97
sta $fe,x

asl $fe,x
lda $fe,x



; LSR A
lda #$91
lsr a

; LSR absolute
lda #$93
sta $5000

lsr $5000
lda $5000

; LSR absolute with x offset
ldx #$01
lda #$95
sta $5000,x

lsr $5000,x
lda $5000,x

; LSR zero page
lda #$97
sta $fe

lsr $fe
lda $fe

; LSR zero page with x offset
ldx #$01
lda #$97
sta $fe,x

lsr $fe,x
lda $fe,x



; DEC absolute
lda #$93
sta $5000

dec $5000
lda $5000

; DEC absolute with x offset
ldx #$01
lda #$95
sta $5000,x

dec $5000,x
lda $5000,x

; DEC zero page
lda #$97
sta $fe

dec $fe
lda $fe

; DEC zero page with x offset
ldx #$01
lda #$97
sta $fe,x

dec $fe,x
lda $fe,x



; INC absolute
lda #$93
sta $5000

inc $5000
lda $5000

; INC absolute with x offset
ldx #$01
lda #$95
sta $5000,x

inc $5000,x
lda $5000,x

; INC zero page
lda #$97
sta $fe

inc $fe
lda $fe

; INC zero page with x offset
ldx #$01
lda #$97
sta $fe,x

inc $fe,x
lda $fe,x



; JSR and RTS
ldx #$02
jsrSubroutine: jsr subroutine
jmp jsrSubroutine

subroutine:
    dex
    beq continue

    rts

continue:

; BRK and RTI
brk
nop ; BRK pushes PC + 1 to the stack so RTI will actually return the instruction just after this NOP.

.done

* = $2000
.byte $34

.vectors