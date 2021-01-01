namespace Ninu.Emulator.CentralProcessor.Operations
{
    /// <summary>
    /// Fetches the high byte of the address found using the address latches and stores it in the
    /// effective address latche high. This operation reproduces a bug found when doing indirect
    /// addressing in the 6502. Typically, the high byte of the effective address is read from
    /// memory at the address one more than the what the address latches point at. For example, if
    /// the address latches point to 0x348c, the low byte of the effective address would be taken
    /// from memory at 0x348c and the high byte of the effective address would be taken from memory
    /// at 0x348d. However, if the address latches point to 0x34ff, the low byte of the effective
    /// address would be taken from memory at 0x34ff as expected but the high byte of the effective
    /// address would be taken from memory at 0x3400. When pulling the second byte from memory,
    /// only the low byte of the address is incremented. The high byte (page byte) doesn't change.
    /// </summary>
    public class FetchEffectiveAddressHigh : CpuOperation
    {
        public override void Execute(NewCpu cpu, IBus bus)
        {
            // Increment the low byte and allow it wrap if the value is 0xff.
            var addressLow = (byte)((cpu.AddressLatchLow + 1) & 0xff);
            var addressHigh = cpu.AddressLatchHigh;

            var effectiveAddressHigh = bus.Read((ushort)(addressLow | (addressHigh << 8)));

            cpu.EffectiveAddressLatchHigh = effectiveAddressHigh;
        }
    }
}