namespace Ninu.Emulator.CentralProcessor
{
    /// <summary>
    /// Represents the operation that will take place during a CPU clock. A struct is being used to
    /// avoid heap allocations. Because we are using a struct, a list of actions is not possible
    /// but instead up to three actions can be specified to be executed during the clock cycle. Any
    /// or all of the actions can be null and the actions are executed in the order of <see
    /// cref="Action1"/>, <see cref="Action2"/>, and <see cref="Action3"/> as you would expect.
    /// </summary>
    public unsafe struct OperationQueueState
    {
        public OperationQueueState(
            delegate*<Cpu, IBus, void> action1,
            delegate*<Cpu, IBus, void> action2,
            delegate*<Cpu, IBus, void> action3,
            bool incrementPC,
            bool free)
        {
            Action1 = action1;
            Action2 = action2;
            Action3 = action3;
            IncrementPC = incrementPC;
            Free = free;
        }

        /// <summary>
        /// The first action to be executed during the clock cycle. This property can be null.
        /// </summary>
        public delegate*<Cpu, IBus, void> Action1 { get; }

        /// <summary>
        /// The second action to be executed during the clock cycle. This property can be null.
        /// </summary>
        public delegate*<Cpu, IBus, void> Action2 { get; }

        /// <summary>
        /// The third action to be executed during the clock cycle. This property can be null.
        /// </summary>
        public delegate*<Cpu, IBus, void> Action3 { get; }

        /// <summary>
        /// Indicates if the CPU should increment the PC register during the clock cycle. If this
        /// property is <c>true</c>, PC will be incremented prior to <see cref="Action1"/>
        /// execution and prior to <see cref="Operation"/> execution. This is the first thing that
        /// is done during a CPU clock cycle.
        /// </summary>
        public bool IncrementPC { get; }

        /// <summary>
        /// If this property is set to <c>true</c>, this operation does not cost the CPU a cycle
        /// and so when this operation is executed by the CPU the next operation in the queue will
        /// also be executed in the same clock cycle. This is useful for situations where an
        /// instruction is unusually pipelined into the first cycle of the next instruction such as
        /// the instructions <c>inx</c>, <c>iny</c>, <c>dex</c>, and <c>dey</c>.
        /// </summary>
        public bool Free { get; }
    }
}