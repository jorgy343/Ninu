# 6502C JIT

## General Idea

Whenever a jump is taken, the destination address is used to perform a lookup into a table of
already JITed functions. If an entry is found, the JITed method is executed. The JITed method will
do this same technique whenever a jump is taken except it will be baked into the JIT method.

The table of JITed methods will be an array 16,384 method pointers. Each element corresponds
one-to-one with a memory location. If an access to get or set a method pointer above the initial
array size is made, the array will be expanded in chunks of 16,384. This will allow for mapped
memory. When a mapper is being used, the absolute address (the address before being mapped) is used
as the value in the lookup. If an element is null, that particular function starting at that memory
location has not yet been JITed and should be JITed. After the JIT, the newly JITed method will be
inserted into the array and executed immediately.

A function begins where a jump lands. A function ends whenever the first unconditional jump is
found. At a later time conditional jumps will be checked to see if they jump backwards inside of
the bounds of the function (i.e. they jump to an address after the entry point and before or
including the jump instruction).

Unconditinal jumps include the following instructions:

- BRK implied
- JMP absolute
- JMP indirect
- JSR absolute
- RTI implied
- RTS implied

If a conditional jump inside of a function leads to a location inside of the function, this will be
handled by the JITed code. If it instead leads to a location outside of the function, the JITed
code will call to external code to hand off execution.

Conditional jumps include the following instructions:

- BCC relative
- BCS relative
- BEQ relative
- BMI relative
- BNE relative
- BPL relative
- BVC relative
- BVS relative

```
entry_point = PC
exit_point = PC

while true
    next_instruction = decode(PC)

    if next_instruction is unconditional jump
        if next_instruction.jump_target >= entry_point and <= PC
            exit_point = PC
            exit infinite while loop

    PC += next_instruction.length
```

## PPU

After every JITed CPU instruction is done executing, the JITed code must call out to an external
routine that handles running the PPU. The PPU is executed four times for every clock cycle the
instruction takes. This can be hard coded by the JITed code as something like:

```
call_ppu(instruction_cycle_execution_count)
```

## NMI

Before every JITed CPU instruction is executed, the JITed code must check for an NMI.