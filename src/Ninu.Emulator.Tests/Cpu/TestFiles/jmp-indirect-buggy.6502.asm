.include "..\..\..\Cpu\TestFiles\std.6502.asm"

; Test an jmp indirect instruction when an indirect address with a low byte of 0xff is given. In
; this case, when reading the high byte of the target address from memory the low byte of the
; indirect address will be incremented (resulting in 0x00) and the high byte will be left alone.

* = $0000
.init

; If the emulator accounts for the bug, the low byte will be read from 0x12ff and the high byte
; from 0x1200.
jmp ($12ff)

* = $1200
.byte $56 ; High byte of the target address.

* = $12ff
.byte $12 ; Low byte of the target address.

* = $1300
.byte $89 ; If the emulator is correct, this byte won't be read due to the bug.

; The target address should be 0x5612 instead of what the 0x8912 that you would expect if this bug
; didn't exist.
* = $5612
.done

; If the emulator is correct, this shouldn't be hit.
* = $8912
.error

.vectors