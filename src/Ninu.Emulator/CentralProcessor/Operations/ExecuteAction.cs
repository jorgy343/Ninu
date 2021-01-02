using System;

namespace Ninu.Emulator.CentralProcessor.Operations
{
    public class ExecuteAction : CpuOperation
    {
        private readonly Action _execute;

        public ExecuteAction(Action execute)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        }

        public override void Execute(NewCpu cpu, IBus bus)
        {
            _execute();
        }
    }
}