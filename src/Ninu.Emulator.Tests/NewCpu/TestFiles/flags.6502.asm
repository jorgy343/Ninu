.include "..\..\..\NewCpu\TestFiles\std.6502.asm"

; All of the flag tests will run the clear and set instructions twice each.

* = $0000

; Carry
clc
sec
clc
sec

; Decimal Mode
cld
sed
cld
sed

; Interrupt Disable
cli
sei
cli
sei

.done

.vectors