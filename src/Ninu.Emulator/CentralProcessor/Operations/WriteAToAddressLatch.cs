namespace Ninu.Emulator.CentralProcessor.Operations
{
    /// <summary>
    /// Writes the A register to memory at the address defined by the address latches <see
    /// cref="NewCpu.AddressLatchLow"/> and <see cref="NewCpu.AddressLatchHigh"/>.
    /// </summary>
    public class WriteAToAddressLatch : CpuOperation
    {
        private WriteAToAddressLatch()
        {

        }

        public static WriteAToAddressLatch Singleton { get; } = new();

        public override void Execute(NewCpu cpu, IBus bus)
        {
            var address = (ushort)(cpu.AddressLatchLow | (cpu.AddressLatchHigh << 8));

            bus.Write(address, cpu.CpuState.A);
        }
    }
}