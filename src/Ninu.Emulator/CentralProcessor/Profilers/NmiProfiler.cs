namespace Ninu.Emulator.CentralProcessor.Profilers
{
    public class NmiProfiler : INmiProfiler, IInstructionExecutedProfiler
    {
        private bool _inNmi;

        public int CountOfReturnedByRti { get; protected set; }
        public int CountOfReturnedByNotRti { get; protected set; }

        public void NmiPerformed(CpuState cpuState)
        {
            if (_inNmi)
            {
                CountOfReturnedByNotRti++;
            }

            _inNmi = true;
        }

        public void InstructionExecuted(CpuInstruction instruction, CpuState cpuState, int cycles)
        {
            if (_inNmi && instruction.Name == "rti")
            {
                _inNmi = false;
                CountOfReturnedByRti++;
            }
        }
    }
}