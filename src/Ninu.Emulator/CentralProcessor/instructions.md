Some instructions have some very weird cycles. A lot of mathematical and logical insructions don't
save the result into the `A` register until the first cycle of the next instruction completes. Even
weirder is the `AND` instruction which saves the flags register on its last cycle but doesn't save
its result into register `A` until after the first cycle of the next instruction.

Notice the difference between `ADC` and `AND` below. They are both two cycle instructions and both
perform an ALU operation on `A` and the immediate. However, `ADC` stores flags into `P` on the next
instruction's first cycle but `AND` stores flags into `P` on its second cycle.

## ADC (Immediate)

| Cycle | PC | Operation                                                                               |
|:-----:|:--:|-----------------------------------------------------------------------------------------|
|   1   | +1 | Fetch memory at PC > Store in data latch                                                |
|   2   | +1 | Fetch next instruction                                                                  |
|   1   |  % | Set P according to result > Store result in A > Perform first cycle of next instruction |

## AND (Immediate)

| Cycle | PC | Operation                                                   |
|:-----:|:--:|-------------------------------------------------------------|
|   1   | +1 | Fetch memory at PC > Store in data latch                    |
|   2   | +1 | Set P according to result > Fetch next instruction          |
|   1   |  % | Store result in A > Perform first cycle of next instruction |