namespace Ninu.Emulator.CentralProcessor.Operations.Interrupts
{
    public class SetPCToAddressLatchAndFetchInstruction : CpuOperation
    {
        private SetPCToAddressLatchAndFetchInstruction()
        {

        }

        public static SetPCToAddressLatchAndFetchInstruction Singleton { get; } = new();

        public override void Execute(Cpu cpu, IBus bus)
        {
            cpu.CpuState.PC = (ushort)(cpu.AddressLatchLow | (cpu.AddressLatchHigh << 8));

            var instruction = bus.Read(cpu.CpuState.PC);
            cpu.ExecuteInstruction(instruction);
        }
    }
}
