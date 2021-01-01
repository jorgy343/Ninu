.include "..\..\..\NewCpu\TestFiles\std.6502.asm"

* = $0000
.init

jmp (indirectAddress)

* = $1234
indirectAddress .addr targetAddress

* = $5678
targetAddress:
.done

.vectors