# Markdown File

All numbers in this document are hex numbers.

Certain memory locations are used for specific purposes in unit tests. These are defined below.

```
0000      = Program start.

a000:efff = Test results.

fd00      = A value of a3 signifies that the test ran to completion.
fd01      = The checkpoint value.
ff00      = Vector for NMI and IRQ. This is an rti instruction.

fffa:fffb = NMI vector
fffc:fffd = Reset vector
fffe:ffff = IRQ vector
```

All of the memory ranges above are determined by the assembly file so they are all configurable on a per test basis
except for the vector pointers which are defined by the 6502.

## Memory Definitions

A memory location is defined as a 4 character hex value wrapped in square brackets. A memory location can be specified
to point to a single byte (`[120b]`) or a contiguous range of memory addresses (`[120b:1250]`).

Memory can be specified to hold certain values before the test is run and when specific checkpoints get hit. Define a
memory location and use the `:=` operator to assign a value to that memory location. Both a single memory location and
a range be defined to hold a single byte. If a memory range is set to a single value, each byte in the memory range is
set to that byte.

```
[5000]      := 24
[5000:50ff] := 24
```

A linear pattern can be assigned to a memory range. The format is either a constent number between 0 and 255 inclusive
or a pattern in the form `i + jn` where `i` is the starting value, `j` is the increment value, and `n` is always the
character 'n'. Both `i` and `j` must be an integer value between 0 and 255 inclusive. Both `i` and `j` are required to
appear in the pattern even if either one of those is zero (though a value of 0 for `j` effectively makes this a
constent assigned to all memory locations in the range.. The result is always a byte and the resulting number is
wrapped around to zero upon an overflow.

```
[5000:50ff] := 5 + 2n
```

In the above example, [5000] would be assigned 5, [5001] would be assigned 7, [5002] would be assigned 9, and so on.

A list of values can also be assigned to a memory range. In this case each each value is listed and separated by a
single space. A value must be listed for each memory location in the memory range.

```
[5000:5003] := 10 00 fd e2
```

## Sections

A section is defined by a group of lines that start with ';'. A section is terminted when a line without a ';' as the
first character is found. The ';' must be the very first character on the line. A section begins when one of the
following patterns are found.

```
; #Init
; #Checkpoint nn
```

Where `nn` is a two character hex number.

## Parsing

The following psuedo code can be used to parse out the sections.

```
for each line in file
    if regex matches
        ^; #Init$
    then
        run block as checkpoint

    if regex matches
        ^; #Checkpoint (?<CheckpointNumber>[0-9a-fA-F]{2})$
    then
        run block as checkpoint
```