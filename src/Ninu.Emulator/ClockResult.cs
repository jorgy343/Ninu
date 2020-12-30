using System;

namespace Ninu.Emulator
{
    [Flags]
    public enum ClockResult
    {
        Nothing = 0x00,
        NormalPpuCycleComplete = 0x01,
        VBlankInterruptComplete = 0x02,
        FrameComplete = 0x04,

        /// <summary>
        /// Used when the clock that finished executing finished out the last cycle of the
        /// currently executing CPU instruction. The next clock that clocks the CPU will begin
        /// executing the next CPU instruction. Since the CPU clocks every third system clock, the
        /// next two system clocks after this status won't modify the CPU state.
        /// </summary>
        InstructionComplete = 0x08,
    }
}