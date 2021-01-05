using Ninu.Emulator.CentralProcessor.Operations;
using System;

namespace Ninu.Emulator.CentralProcessor
{
    /// <summary>
    /// Represents the operation that will take place during a CPU clock. The primary member to be
    /// concerned about is <see cref="Operation"/>. This defines the primary function of the CPU
    /// cycle. However, there are many other flags on this struct that dictate how the CPU will
    /// handle the cycle. See the documentation on each flag for more details.
    /// </summary>
    public struct NewCpuOperationQueueState
    {
        public NewCpuOperationQueueState(
            CpuOperation operation,
            Action? preAction,
            Action? postAction,
            bool incrementPC,
            bool free)
        {
            Operation = operation ?? throw new ArgumentNullException(nameof(operation));
            PreAction = preAction;
            PostAction = postAction;
            IncrementPC = incrementPC;
            Free = free;
        }

        /// <summary>
        /// The primary operation that will take place during the CPU clock.
        /// </summary>
        public CpuOperation Operation { get; }

        /// <summary>
        /// Optionally, an action that will be executed prior to the <see cref="Operation"/>
        /// execution.
        /// </summary>
        public Action? PreAction { get; }

        /// <summary>
        /// Optionally, an action that will be executed after <see cref="Operation"/> execution.
        /// </summary>
        public Action? PostAction { get; }

        /// <summary>
        /// Indicates if the CPU should increment the PC register during the clock cycle. If this
        /// property is <c>true</c>, PC will be incremented prior to <see cref="PreAction"/> execution
        /// and prior to <see cref="Operation"/> execution. This is the first thing that is done
        /// during a CPU clock cycle.
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