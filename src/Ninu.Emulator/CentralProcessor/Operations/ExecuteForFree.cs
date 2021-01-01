using System;

namespace Ninu.Emulator.CentralProcessor.Operations
{
    /// <summary>
    /// Executes an action that represents what the instruction does. This does not cost the CPU a
    /// cycle and so when this operation is executed by the CPU the next operation in the queue
    /// will also be executed in the same clock cycle. This is useful for the instructions that
    /// actually execute during the first cycle of the next instruction such as <c>inx</c> and
    /// <c>iny</c>.
    /// </summary>
    public class ExecuteForFree : CpuOperation
    {
        private readonly Action _execute;

        public ExecuteForFree(Action execute)
            : base(true)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        }

        public override void Execute(NewCpu cpu, IBus bus)
        {
            _execute();
        }
    }
}