namespace Ninu.Emulator.CentralProcessor
{
    internal static unsafe class ConditionalJumps
    {
        /// <summary>
        /// All of the conditional jump instructions call this method. When a conditional jump is
        /// decoded onto the queue, four operations are queued (see <see cref="Addr_Relative"/>).
        /// If the condition fails and the jump is not being taken, this method will dequeue the
        /// next three operations and queue a new operation that simply increments <c>PC</c> and
        /// fetches the next instruction. If the jump is being taken, then this method sets the
        /// address latch to <c>PC + 1</c>. This way, to find the destination of the jump, just add
        /// the offset from the instruction's operand and add it to the address latch.
        /// </summary>
        /// <param name="takingJump">A boolean that says whether the jump is being taken. Pass <c>true</c> if the branch condition succeeds and the jump is being taken. Otherwise, pass <c>false</c>.</param>
        private static void PerformConditionalJumpOperation(Cpu cpu, bool takingJump)
        {
            if (!takingJump)
            {
                cpu.Queue.Dequeue();
                cpu.Queue.Dequeue();
                cpu.Queue.Dequeue();

                cpu.AddOperation(true, &Operations2.FetchInstruction);
            }

            // Save PC + 1 into address latch because it will get clobbered.
            cpu.AddressLatchLow = (byte)((cpu.CpuState.PC + 1) & 0xff);
            cpu.AddressLatchHigh = (byte)((cpu.CpuState.PC + 1) >> 8);
        }

        internal static void Bcc(Cpu cpu, IBus bus)
        {
            PerformConditionalJumpOperation(cpu, !cpu.CpuState.GetFlag(CpuFlags.C));
        }

        internal static void Bcs(Cpu cpu, IBus bus)
        {
            PerformConditionalJumpOperation(cpu, cpu.CpuState.GetFlag(CpuFlags.C));
        }

        internal static void Beq(Cpu cpu, IBus bus)
        {
            PerformConditionalJumpOperation(cpu, cpu.CpuState.GetFlag(CpuFlags.Z));
        }

        internal static void Bmi(Cpu cpu, IBus bus)
        {
            PerformConditionalJumpOperation(cpu, cpu.CpuState.GetFlag(CpuFlags.N));
        }

        internal static void Bne(Cpu cpu, IBus bus)
        {
            PerformConditionalJumpOperation(cpu, !cpu.CpuState.GetFlag(CpuFlags.Z));
        }

        internal static void Bpl(Cpu cpu, IBus bus)
        {
            PerformConditionalJumpOperation(cpu, !cpu.CpuState.GetFlag(CpuFlags.N));
        }

        internal static void Bvc(Cpu cpu, IBus bus)
        {
            PerformConditionalJumpOperation(cpu, !cpu.CpuState.GetFlag(CpuFlags.V));
        }

        internal static void Bvs(Cpu cpu, IBus bus)
        {
            PerformConditionalJumpOperation(cpu, cpu.CpuState.GetFlag(CpuFlags.V));
        }
    }
}