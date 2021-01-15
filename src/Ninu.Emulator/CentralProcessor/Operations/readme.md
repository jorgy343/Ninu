This namespace contains all of the CPUs micro operations. The concept used to implement the logic
of this emulated CPU is similar to how actual processors will decode instructions into uops (micro
operations) and then execute the uops one by one. This allows for great reuse of code and also
allows each operation to represent exactly one CPU cycle.

There are some exceptions to the one operation is equal to one CPU cycle. Some instructions
actually finish executing during the first cycle of the next instruction. Examples include `inx`
and `iny` which don't update the `x` and `y` registers or the flags register until the third cycle
even though both of these instructions are 2 cycle instructions.

It should be noted that the actual 6502 processor used a PLA ROM for instruction decoding rather
than uops. This allowed the circuitry of the 6502 to be more compact and ultimately cheaper to
manufacture.

## Naming

Operations that fetch data and store it somewhere will be named with the pattern
`FetchXXXByYYYIntoZZZ`. `XXX` is where the data is being fetched from which is often `Memory`.
`YYY` is what is being used to find that data. If data is being read from memory, this is what is
used to reference into the memory such as `PC`. `ZZZ` is where the data is being stored such as the
CPU's data latch.

## Incrementing of PC

The PC register is not incremented by any of the operations. Whether an operation increments PC is
sometimes a bit arbitrary. The same operation in one instruction might need the PC incremented
while another instruction requires the PC to remain constant. In order to facilitate as much reuse
of the operations as possible, whether the PC is incremented or not is specified during the
decoding phase.

If the decoding phase specifies that an operation is to increment PC, the incrementation is done
prior to executing the operation. This is handled by the logic within the CPU.

## NMI

NMI is handled in a strange way. The NMI flag is checked just prior to an instruction fetch. If the
NMI flag is true and the NMI flag was not set during this cycle, the instruction fetch isn't
performed and the NMI routine is put on the queue. If the NMI flag was set during the same cycle as
the instruction fetch, the fetch goes forward and the next instruction is executed. The next
instruction's instruction fetch will then pick up the NMI and execute it instead of loading the
next instruction.