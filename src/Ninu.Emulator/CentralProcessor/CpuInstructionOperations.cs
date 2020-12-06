using Ninu.Base;
using System;
using System.Collections.Generic;

namespace Ninu.Emulator.CentralProcessor
{
    public delegate int InstructionExecutor(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState);
    public delegate int InstructionExecutorEx(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState, byte? dataIn, out byte result);

    /// <summary>
    /// Exposes all of the CPU's instructions as static methods.
    /// </summary>
    public static class CpuInstructionOperations
    {
        private static readonly Dictionary<Instruction, (InstructionExecutor? Executor, InstructionExecutorEx? ExecutorEx)> _instructionExecutors;

        private static (InstructionExecutor? Executor, InstructionExecutorEx? ExecutorEx) CreateTuple(InstructionExecutor executor) =>
            (executor, null);

        private static (InstructionExecutor? Executor, InstructionExecutorEx? ExecutorEx) CreateTuple(InstructionExecutorEx executorEx) =>
            (null, executorEx);

        static CpuInstructionOperations()
        {
            _instructionExecutors = new(256)
            {
                [Instruction.GetByOpCode(0x00)] = CreateTuple(Brk),
                [Instruction.GetByOpCode(0x01)] = CreateTuple(Ora),
                [Instruction.GetByOpCode(0x02)] = CreateTuple(Kil),
                [Instruction.GetByOpCode(0x03)] = CreateTuple(Slo),
                [Instruction.GetByOpCode(0x04)] = CreateTuple(Dop),
                [Instruction.GetByOpCode(0x05)] = CreateTuple(Ora),
                [Instruction.GetByOpCode(0x06)] = CreateTuple(Asl),
                [Instruction.GetByOpCode(0x07)] = CreateTuple(Slo),
                [Instruction.GetByOpCode(0x08)] = CreateTuple(Php),
                [Instruction.GetByOpCode(0x09)] = CreateTuple(Ora),
                [Instruction.GetByOpCode(0x0a)] = CreateTuple(Asl),
                [Instruction.GetByOpCode(0x0b)] = CreateTuple(Nop),
                [Instruction.GetByOpCode(0x0c)] = CreateTuple(Top),
                [Instruction.GetByOpCode(0x0d)] = CreateTuple(Ora),
                [Instruction.GetByOpCode(0x0e)] = CreateTuple(Asl),
                [Instruction.GetByOpCode(0x0f)] = CreateTuple(Slo),
                [Instruction.GetByOpCode(0x10)] = CreateTuple(Bpl),
                [Instruction.GetByOpCode(0x11)] = CreateTuple(Ora),
                [Instruction.GetByOpCode(0x12)] = CreateTuple(Kil),
                [Instruction.GetByOpCode(0x13)] = CreateTuple(Slo),
                [Instruction.GetByOpCode(0x14)] = CreateTuple(Dop),
                [Instruction.GetByOpCode(0x15)] = CreateTuple(Ora),
                [Instruction.GetByOpCode(0x16)] = CreateTuple(Asl),
                [Instruction.GetByOpCode(0x17)] = CreateTuple(Slo),
                [Instruction.GetByOpCode(0x18)] = CreateTuple(Clc),
                [Instruction.GetByOpCode(0x19)] = CreateTuple(Ora),
                [Instruction.GetByOpCode(0x1a)] = CreateTuple(Nop),
                [Instruction.GetByOpCode(0x1b)] = CreateTuple(Slo),
                [Instruction.GetByOpCode(0x1c)] = CreateTuple(Top),
                [Instruction.GetByOpCode(0x1d)] = CreateTuple(Ora),
                [Instruction.GetByOpCode(0x1e)] = CreateTuple(Asl),
                [Instruction.GetByOpCode(0x1f)] = CreateTuple(Slo),
                [Instruction.GetByOpCode(0x20)] = CreateTuple(Jsr),
                [Instruction.GetByOpCode(0x21)] = CreateTuple(And),
                [Instruction.GetByOpCode(0x22)] = CreateTuple(Kil),
                [Instruction.GetByOpCode(0x23)] = CreateTuple(Rla),
                [Instruction.GetByOpCode(0x24)] = CreateTuple(Bit),
                [Instruction.GetByOpCode(0x25)] = CreateTuple(And),
                [Instruction.GetByOpCode(0x26)] = CreateTuple(Rol),
                [Instruction.GetByOpCode(0x27)] = CreateTuple(Rla),
                [Instruction.GetByOpCode(0x28)] = CreateTuple(Plp),
                [Instruction.GetByOpCode(0x29)] = CreateTuple(And),
                [Instruction.GetByOpCode(0x2a)] = CreateTuple(Rol),
                [Instruction.GetByOpCode(0x2b)] = CreateTuple(Nop),
                [Instruction.GetByOpCode(0x2c)] = CreateTuple(Bit),
                [Instruction.GetByOpCode(0x2d)] = CreateTuple(And),
                [Instruction.GetByOpCode(0x2e)] = CreateTuple(Rol),
                [Instruction.GetByOpCode(0x2f)] = CreateTuple(Rla),
                [Instruction.GetByOpCode(0x30)] = CreateTuple(Bmi),
                [Instruction.GetByOpCode(0x31)] = CreateTuple(And),
                [Instruction.GetByOpCode(0x32)] = CreateTuple(Kil),
                [Instruction.GetByOpCode(0x33)] = CreateTuple(Rla),
                [Instruction.GetByOpCode(0x34)] = CreateTuple(Dop),
                [Instruction.GetByOpCode(0x35)] = CreateTuple(And),
                [Instruction.GetByOpCode(0x36)] = CreateTuple(Rol),
                [Instruction.GetByOpCode(0x37)] = CreateTuple(Rla),
                [Instruction.GetByOpCode(0x38)] = CreateTuple(Sec),
                [Instruction.GetByOpCode(0x39)] = CreateTuple(And),
                [Instruction.GetByOpCode(0x3a)] = CreateTuple(Nop),
                [Instruction.GetByOpCode(0x3b)] = CreateTuple(Rla),
                [Instruction.GetByOpCode(0x3c)] = CreateTuple(Top),
                [Instruction.GetByOpCode(0x3d)] = CreateTuple(And),
                [Instruction.GetByOpCode(0x3e)] = CreateTuple(Rol),
                [Instruction.GetByOpCode(0x3f)] = CreateTuple(Rla),
                [Instruction.GetByOpCode(0x40)] = CreateTuple(Rti),
                [Instruction.GetByOpCode(0x41)] = CreateTuple(Eor),
                [Instruction.GetByOpCode(0x42)] = CreateTuple(Kil),
                [Instruction.GetByOpCode(0x43)] = CreateTuple(Nop),
                [Instruction.GetByOpCode(0x44)] = CreateTuple(Dop),
                [Instruction.GetByOpCode(0x45)] = CreateTuple(Eor),
                [Instruction.GetByOpCode(0x46)] = CreateTuple(Lsr),
                [Instruction.GetByOpCode(0x47)] = CreateTuple(Nop),
                [Instruction.GetByOpCode(0x48)] = CreateTuple(Pha),
                [Instruction.GetByOpCode(0x49)] = CreateTuple(Eor),
                [Instruction.GetByOpCode(0x4a)] = CreateTuple(Lsr),
                [Instruction.GetByOpCode(0x4b)] = CreateTuple(Nop),
                [Instruction.GetByOpCode(0x4c)] = CreateTuple(Jmp),
                [Instruction.GetByOpCode(0x4d)] = CreateTuple(Eor),
                [Instruction.GetByOpCode(0x4e)] = CreateTuple(Lsr),
                [Instruction.GetByOpCode(0x4f)] = CreateTuple(Nop),
                [Instruction.GetByOpCode(0x50)] = CreateTuple(Bvc),
                [Instruction.GetByOpCode(0x51)] = CreateTuple(Eor),
                [Instruction.GetByOpCode(0x52)] = CreateTuple(Kil),
                [Instruction.GetByOpCode(0x53)] = CreateTuple(Nop),
                [Instruction.GetByOpCode(0x54)] = CreateTuple(Dop),
                [Instruction.GetByOpCode(0x55)] = CreateTuple(Eor),
                [Instruction.GetByOpCode(0x56)] = CreateTuple(Lsr),
                [Instruction.GetByOpCode(0x57)] = CreateTuple(Nop),
                [Instruction.GetByOpCode(0x58)] = CreateTuple(Cli),
                [Instruction.GetByOpCode(0x59)] = CreateTuple(Eor),
                [Instruction.GetByOpCode(0x5a)] = CreateTuple(Nop),
                [Instruction.GetByOpCode(0x5b)] = CreateTuple(Nop),
                [Instruction.GetByOpCode(0x5c)] = CreateTuple(Top),
                [Instruction.GetByOpCode(0x5d)] = CreateTuple(Eor),
                [Instruction.GetByOpCode(0x5e)] = CreateTuple(Lsr),
                [Instruction.GetByOpCode(0x5f)] = CreateTuple(Nop),
                [Instruction.GetByOpCode(0x60)] = CreateTuple(Rts),
                [Instruction.GetByOpCode(0x61)] = CreateTuple(Adc),
                [Instruction.GetByOpCode(0x62)] = CreateTuple(Kil),
                [Instruction.GetByOpCode(0x63)] = CreateTuple(Rra),
                [Instruction.GetByOpCode(0x64)] = CreateTuple(Dop),
                [Instruction.GetByOpCode(0x65)] = CreateTuple(Adc),
                [Instruction.GetByOpCode(0x66)] = CreateTuple(Ror),
                [Instruction.GetByOpCode(0x67)] = CreateTuple(Rra),
                [Instruction.GetByOpCode(0x68)] = CreateTuple(Pla),
                [Instruction.GetByOpCode(0x69)] = CreateTuple(Adc),
                [Instruction.GetByOpCode(0x6a)] = CreateTuple(Ror),
                [Instruction.GetByOpCode(0x6b)] = CreateTuple(Nop),
                [Instruction.GetByOpCode(0x6c)] = CreateTuple(Jmp),
                [Instruction.GetByOpCode(0x6d)] = CreateTuple(Adc),
                [Instruction.GetByOpCode(0x6e)] = CreateTuple(Ror),
                [Instruction.GetByOpCode(0x6f)] = CreateTuple(Rra),
                [Instruction.GetByOpCode(0x70)] = CreateTuple(Bvs),
                [Instruction.GetByOpCode(0x71)] = CreateTuple(Adc),
                [Instruction.GetByOpCode(0x72)] = CreateTuple(Kil),
                [Instruction.GetByOpCode(0x73)] = CreateTuple(Rra),
                [Instruction.GetByOpCode(0x74)] = CreateTuple(Dop),
                [Instruction.GetByOpCode(0x75)] = CreateTuple(Adc),
                [Instruction.GetByOpCode(0x76)] = CreateTuple(Ror),
                [Instruction.GetByOpCode(0x77)] = CreateTuple(Rra),
                [Instruction.GetByOpCode(0x78)] = CreateTuple(Sei),
                [Instruction.GetByOpCode(0x79)] = CreateTuple(Adc),
                [Instruction.GetByOpCode(0x7a)] = CreateTuple(Nop),
                [Instruction.GetByOpCode(0x7b)] = CreateTuple(Rra),
                [Instruction.GetByOpCode(0x7c)] = CreateTuple(Top),
                [Instruction.GetByOpCode(0x7d)] = CreateTuple(Adc),
                [Instruction.GetByOpCode(0x7e)] = CreateTuple(Ror),
                [Instruction.GetByOpCode(0x7f)] = CreateTuple(Rra),
                [Instruction.GetByOpCode(0x80)] = CreateTuple(Dop),
                [Instruction.GetByOpCode(0x81)] = CreateTuple(Sta),
                [Instruction.GetByOpCode(0x82)] = CreateTuple(Dop),
                [Instruction.GetByOpCode(0x83)] = CreateTuple(Aax),
                [Instruction.GetByOpCode(0x84)] = CreateTuple(Sty),
                [Instruction.GetByOpCode(0x85)] = CreateTuple(Sta),
                [Instruction.GetByOpCode(0x86)] = CreateTuple(Stx),
                [Instruction.GetByOpCode(0x87)] = CreateTuple(Aax),
                [Instruction.GetByOpCode(0x88)] = CreateTuple(Dey),
                [Instruction.GetByOpCode(0x89)] = CreateTuple(Dop),
                [Instruction.GetByOpCode(0x8a)] = CreateTuple(Txa),
                [Instruction.GetByOpCode(0x8b)] = CreateTuple(Nop),
                [Instruction.GetByOpCode(0x8c)] = CreateTuple(Sty),
                [Instruction.GetByOpCode(0x8d)] = CreateTuple(Sta),
                [Instruction.GetByOpCode(0x8e)] = CreateTuple(Stx),
                [Instruction.GetByOpCode(0x8f)] = CreateTuple(Aax),
                [Instruction.GetByOpCode(0x90)] = CreateTuple(Bcc),
                [Instruction.GetByOpCode(0x91)] = CreateTuple(Sta),
                [Instruction.GetByOpCode(0x92)] = CreateTuple(Kil),
                [Instruction.GetByOpCode(0x93)] = CreateTuple(Nop),
                [Instruction.GetByOpCode(0x94)] = CreateTuple(Sty),
                [Instruction.GetByOpCode(0x95)] = CreateTuple(Sta),
                [Instruction.GetByOpCode(0x96)] = CreateTuple(Stx),
                [Instruction.GetByOpCode(0x97)] = CreateTuple(Aax),
                [Instruction.GetByOpCode(0x98)] = CreateTuple(Tya),
                [Instruction.GetByOpCode(0x99)] = CreateTuple(Sta),
                [Instruction.GetByOpCode(0x9a)] = CreateTuple(Txs),
                [Instruction.GetByOpCode(0x9b)] = CreateTuple(Nop),
                [Instruction.GetByOpCode(0x9c)] = CreateTuple(Nop),
                [Instruction.GetByOpCode(0x9d)] = CreateTuple(Sta),
                [Instruction.GetByOpCode(0x9e)] = CreateTuple(Nop),
                [Instruction.GetByOpCode(0x9f)] = CreateTuple(Nop),
                [Instruction.GetByOpCode(0xa0)] = CreateTuple(Ldy),
                [Instruction.GetByOpCode(0xa1)] = CreateTuple(Lda),
                [Instruction.GetByOpCode(0xa2)] = CreateTuple(Ldx),
                [Instruction.GetByOpCode(0xa3)] = CreateTuple(Lax),
                [Instruction.GetByOpCode(0xa4)] = CreateTuple(Ldy),
                [Instruction.GetByOpCode(0xa5)] = CreateTuple(Lda),
                [Instruction.GetByOpCode(0xa6)] = CreateTuple(Ldx),
                [Instruction.GetByOpCode(0xa7)] = CreateTuple(Lax),
                [Instruction.GetByOpCode(0xa8)] = CreateTuple(Tay),
                [Instruction.GetByOpCode(0xa9)] = CreateTuple(Lda),
                [Instruction.GetByOpCode(0xaa)] = CreateTuple(Tax),
                [Instruction.GetByOpCode(0xab)] = CreateTuple(Nop),
                [Instruction.GetByOpCode(0xac)] = CreateTuple(Ldy),
                [Instruction.GetByOpCode(0xad)] = CreateTuple(Lda),
                [Instruction.GetByOpCode(0xae)] = CreateTuple(Ldx),
                [Instruction.GetByOpCode(0xaf)] = CreateTuple(Lax),
                [Instruction.GetByOpCode(0xb0)] = CreateTuple(Bcs),
                [Instruction.GetByOpCode(0xb1)] = CreateTuple(Lda),
                [Instruction.GetByOpCode(0xb2)] = CreateTuple(Kil),
                [Instruction.GetByOpCode(0xb3)] = CreateTuple(Lax),
                [Instruction.GetByOpCode(0xb4)] = CreateTuple(Ldy),
                [Instruction.GetByOpCode(0xb5)] = CreateTuple(Lda),
                [Instruction.GetByOpCode(0xb6)] = CreateTuple(Ldx),
                [Instruction.GetByOpCode(0xb7)] = CreateTuple(Lax),
                [Instruction.GetByOpCode(0xb8)] = CreateTuple(Clv),
                [Instruction.GetByOpCode(0xb9)] = CreateTuple(Lda),
                [Instruction.GetByOpCode(0xba)] = CreateTuple(Tsx),
                [Instruction.GetByOpCode(0xbb)] = CreateTuple(Nop),
                [Instruction.GetByOpCode(0xbc)] = CreateTuple(Ldy),
                [Instruction.GetByOpCode(0xbd)] = CreateTuple(Lda),
                [Instruction.GetByOpCode(0xbe)] = CreateTuple(Ldx),
                [Instruction.GetByOpCode(0xbf)] = CreateTuple(Lax),
                [Instruction.GetByOpCode(0xc0)] = CreateTuple(Cpy),
                [Instruction.GetByOpCode(0xc1)] = CreateTuple(Cmp),
                [Instruction.GetByOpCode(0xc2)] = CreateTuple(Dop),
                [Instruction.GetByOpCode(0xc3)] = CreateTuple(Nop),
                [Instruction.GetByOpCode(0xc4)] = CreateTuple(Cpy),
                [Instruction.GetByOpCode(0xc5)] = CreateTuple(Cmp),
                [Instruction.GetByOpCode(0xc6)] = CreateTuple(Dec),
                [Instruction.GetByOpCode(0xc7)] = CreateTuple(Nop),
                [Instruction.GetByOpCode(0xc8)] = CreateTuple(Iny),
                [Instruction.GetByOpCode(0xc9)] = CreateTuple(Cmp),
                [Instruction.GetByOpCode(0xca)] = CreateTuple(Dex),
                [Instruction.GetByOpCode(0xcb)] = CreateTuple(Axs),
                [Instruction.GetByOpCode(0xcc)] = CreateTuple(Cpy),
                [Instruction.GetByOpCode(0xcd)] = CreateTuple(Cmp),
                [Instruction.GetByOpCode(0xce)] = CreateTuple(Dec),
                [Instruction.GetByOpCode(0xcf)] = CreateTuple(Nop),
                [Instruction.GetByOpCode(0xd0)] = CreateTuple(Bne),
                [Instruction.GetByOpCode(0xd1)] = CreateTuple(Cmp),
                [Instruction.GetByOpCode(0xd2)] = CreateTuple(Kil),
                [Instruction.GetByOpCode(0xd3)] = CreateTuple(Nop),
                [Instruction.GetByOpCode(0xd4)] = CreateTuple(Dop),
                [Instruction.GetByOpCode(0xd5)] = CreateTuple(Cmp),
                [Instruction.GetByOpCode(0xd6)] = CreateTuple(Dec),
                [Instruction.GetByOpCode(0xd7)] = CreateTuple(Nop),
                [Instruction.GetByOpCode(0xd8)] = CreateTuple(Cld),
                [Instruction.GetByOpCode(0xd9)] = CreateTuple(Cmp),
                [Instruction.GetByOpCode(0xda)] = CreateTuple(Nop),
                [Instruction.GetByOpCode(0xdb)] = CreateTuple(Nop),
                [Instruction.GetByOpCode(0xdc)] = CreateTuple(Top),
                [Instruction.GetByOpCode(0xdd)] = CreateTuple(Cmp),
                [Instruction.GetByOpCode(0xde)] = CreateTuple(Dec),
                [Instruction.GetByOpCode(0xdf)] = CreateTuple(Nop),
                [Instruction.GetByOpCode(0xe0)] = CreateTuple(Cpx),
                [Instruction.GetByOpCode(0xe1)] = CreateTuple(Sbc),
                [Instruction.GetByOpCode(0xe2)] = CreateTuple(Dop),
                [Instruction.GetByOpCode(0xe3)] = CreateTuple(Nop),
                [Instruction.GetByOpCode(0xe4)] = CreateTuple(Cpx),
                [Instruction.GetByOpCode(0xe5)] = CreateTuple(Sbc),
                [Instruction.GetByOpCode(0xe6)] = CreateTuple(Inc),
                [Instruction.GetByOpCode(0xe7)] = CreateTuple(Nop),
                [Instruction.GetByOpCode(0xe8)] = CreateTuple(Inx),
                [Instruction.GetByOpCode(0xe9)] = CreateTuple(Sbc),
                [Instruction.GetByOpCode(0xea)] = CreateTuple(Nop),
                [Instruction.GetByOpCode(0xeb)] = CreateTuple(Sbc),
                [Instruction.GetByOpCode(0xec)] = CreateTuple(Cpx),
                [Instruction.GetByOpCode(0xed)] = CreateTuple(Sbc),
                [Instruction.GetByOpCode(0xee)] = CreateTuple(Inc),
                [Instruction.GetByOpCode(0xef)] = CreateTuple(Nop),
                [Instruction.GetByOpCode(0xf0)] = CreateTuple(Beq),
                [Instruction.GetByOpCode(0xf1)] = CreateTuple(Sbc),
                [Instruction.GetByOpCode(0xf2)] = CreateTuple(Kil),
                [Instruction.GetByOpCode(0xf3)] = CreateTuple(Nop),
                [Instruction.GetByOpCode(0xf4)] = CreateTuple(Dop),
                [Instruction.GetByOpCode(0xf5)] = CreateTuple(Sbc),
                [Instruction.GetByOpCode(0xf6)] = CreateTuple(Inc),
                [Instruction.GetByOpCode(0xf7)] = CreateTuple(Nop),
                [Instruction.GetByOpCode(0xf8)] = CreateTuple(Sed),
                [Instruction.GetByOpCode(0xf9)] = CreateTuple(Sbc),
                [Instruction.GetByOpCode(0xfa)] = CreateTuple(Nop),
                [Instruction.GetByOpCode(0xfb)] = CreateTuple(Nop),
                [Instruction.GetByOpCode(0xfc)] = CreateTuple(Top),
                [Instruction.GetByOpCode(0xfd)] = CreateTuple(Sbc),
                [Instruction.GetByOpCode(0xfe)] = CreateTuple(Inc),
                [Instruction.GetByOpCode(0xff)] = CreateTuple(Nop),
            };
        }

        public static int ExecuteInstruction(Instruction cpuInstruction, IBus bus, CpuState cpuState)
        {
            if (cpuInstruction is null) throw new ArgumentNullException(nameof(cpuInstruction));
            if (bus is null) throw new ArgumentNullException(nameof(bus));
            if (cpuState is null) throw new ArgumentNullException(nameof(cpuState));

            var executors = _instructionExecutors[cpuInstruction];

            if (executors.Executor is not null)
            {
                return executors.Executor(cpuInstruction.AddressingMode, cpuInstruction.BaseCycles, bus, cpuState);
            }

            if (executors.ExecutorEx is not null)
            {
                return executors.ExecutorEx(cpuInstruction.AddressingMode, cpuInstruction.BaseCycles, bus, cpuState, null, out _);
            }

            return 0;
        }

        /// <summary>
        /// Gets a 16-bit address from memory based on the addressing mode. Addressing modes
        /// <see cref="AddressingMode.Implied"/>, <see cref="AddressingMode.Accumulator"/>, and
        /// <see cref="AddressingMode.Immediate"/> are not valid modes for this method.
        /// </summary>
        /// <param name="addressingMode">The addressing mode that determines how to access memory.</param>
        /// <param name="bus">The bus used to perform reads. The bus is never written to.</param>
        /// <param name="cpuState">The CPU state.</param>
        /// <returns>A tuple containing the 16-bit address and the amount of penalty cycles incurred, if any.</returns>
        private static (ushort Address, int AdditionalCycles) GetAddress(AddressingMode addressingMode, IBus bus, CpuState cpuState)
        {
            switch (addressingMode)
            {
                case AddressingMode.Implied:
                case AddressingMode.Accumulator:
                case AddressingMode.Immediate:
                    {
                        throw new InvalidOperationException("Cannot get an address when addressing mode is Implied, Accumulator, or Immediate.");
                    }

                case AddressingMode.ZeroPage:
                    {
                        ushort address = bus.Read(cpuState.PC++);

                        return (address, 0);
                    }

                case AddressingMode.ZeroPageWithXOffset:
                    {
                        var address = (ushort)bus.Read(cpuState.PC++);
                        var offsetAddress = (ushort)(address + cpuState.X);

                        offsetAddress &= 0x00ff;

                        return (offsetAddress, 0);
                    }

                case AddressingMode.ZeroPageWithYOffset:
                    {
                        var address = (ushort)bus.Read(cpuState.PC++);
                        var offsetAddress = (ushort)(address + cpuState.Y);

                        offsetAddress &= 0x00ff;

                        return (offsetAddress, 0);
                    }

                case AddressingMode.Absolute:
                    {
                        var addressLow = (ushort)bus.Read(cpuState.PC++);
                        var addressHigh = (ushort)(bus.Read(cpuState.PC++) << 8);

                        var address = (ushort)(addressLow | addressHigh);

                        return (address, 0);
                    }

                case AddressingMode.AbsoluteWithXOffset:
                    {
                        var addressLow = (ushort)bus.Read(cpuState.PC++);
                        var addressHigh = (ushort)(bus.Read(cpuState.PC++) << 8);

                        var address = (ushort)(addressLow | addressHigh);
                        var offsetAddress = (ushort)(address + cpuState.X);

                        // Compare the high byte of the address and offset address. If they are different, add an
                        // additional cycle as a penalty.
                        var additionalCycles = IsDifferentPage(address, offsetAddress) ? 1 : 0;

                        return (offsetAddress, additionalCycles);
                    }

                case AddressingMode.AbsoluteWithYOffset:
                    {
                        var addressLow = (ushort)bus.Read(cpuState.PC++);
                        var addressHigh = (ushort)(bus.Read(cpuState.PC++) << 8);

                        var address = (ushort)(addressLow | addressHigh);
                        var offsetAddress = (ushort)(address + cpuState.Y);

                        // Compare the high byte of the address and offset address. If they are different, add an
                        // additional cycle as a penalty.
                        var additionalCycles = IsDifferentPage(address, offsetAddress) ? 1 : 0;

                        return (offsetAddress, additionalCycles);
                    }

                case AddressingMode.Indirect:
                    {
                        var addressLow = (ushort)bus.Read(cpuState.PC++);
                        var addressHigh = (ushort)(bus.Read(cpuState.PC++) << 8);

                        var address = (ushort)(addressLow | addressHigh);

                        // This simulates a bug in the 6502 hardware.
                        if (addressLow == 0x00ff)
                        {
                            var newAddressLow = (ushort)bus.Read(address);
                            var newAddressHigh = (ushort)(bus.Read((ushort)(address & 0xff00)) << 8);

                            var newAddress = (ushort)(newAddressLow | newAddressHigh);

                            return (newAddress, 0);
                        }
                        else
                        {
                            var newAddressLow = (ushort)bus.Read(address);
                            var newAddressHigh = (ushort)(bus.Read((ushort)(address + 1)) << 8);

                            var newAddress = (ushort)(newAddressLow | newAddressHigh);

                            return (newAddress, 0);
                        }
                    }

                case AddressingMode.IndirectZeroPageWithXOffset:
                    {
                        var address = (ushort)bus.Read(cpuState.PC++);

                        address = (ushort)((address + cpuState.X) & 0x00ff);

                        var newAddressLow = (ushort)bus.Read(address);
                        var newAddressHigh = (ushort)(bus.Read((ushort)((address + 1) & 0x00ff)) << 8);

                        var newAddress = (ushort)(newAddressLow | newAddressHigh);

                        return (newAddress, 0);
                    }

                case AddressingMode.IndirectZeroPageWithYOffset:
                    {
                        var indirectAddress = (ushort)bus.Read(cpuState.PC++);

                        var addressLow = (ushort)bus.Read((ushort)(indirectAddress & 0x00ff));
                        var addressHigh = (ushort)(bus.Read((ushort)((indirectAddress + 1) & 0x00ff)) << 8);

                        var address = (ushort)(addressLow | addressHigh);
                        var finalAddress = (ushort)(address + cpuState.Y);

                        var additionalCycles = IsDifferentPage(address, finalAddress) ? 1 : 0;

                        return (finalAddress, additionalCycles);
                    }

                case AddressingMode.Relative:
                    {
                        var data = bus.Read(cpuState.PC++);

                        var address = (ushort)((short)cpuState.PC + (sbyte)data);

                        return (address, 0);
                    }

                case AddressingMode.Dummy:
                    {
                        return (0, 0);
                    }

                default:
                    throw new InvalidAddressingModeException();
            }
        }

        private static (byte Data, ushort Address, int AdditionalCycles) FetchData(AddressingMode addressingMode, IBus bus, CpuState cpuState)
        {
            switch (addressingMode)
            {
                case AddressingMode.Implied:
                case AddressingMode.Indirect:
                case AddressingMode.Relative:
                    {
                        throw new InvalidOperationException("Cannot fetch data when addressing mode is Implied, Indirect, or Relative.");
                    }

                case AddressingMode.Accumulator:
                    {
                        return (cpuState.A, 0, 0);
                    }

                case AddressingMode.Immediate:
                    {
                        var data = bus.Read(cpuState.PC++);

                        return (data, 0, 0);
                    }

                case AddressingMode.ZeroPage:
                case AddressingMode.ZeroPageWithXOffset:
                case AddressingMode.ZeroPageWithYOffset:
                case AddressingMode.Absolute:
                case AddressingMode.AbsoluteWithXOffset:
                case AddressingMode.AbsoluteWithYOffset:
                case AddressingMode.IndirectZeroPageWithXOffset:
                case AddressingMode.IndirectZeroPageWithYOffset:
                    {
                        var (address, additionalCycles) = GetAddress(addressingMode, bus, cpuState);
                        var data = bus.Read(address);

                        return (data, address, additionalCycles);
                    }

                case AddressingMode.Dummy:
                    {
                        return (0, 0, 0);
                    }

                default:
                    throw new InvalidAddressingModeException();
            }
        }

        /// <summary>
        /// Writes a byte into the memory location 0x0100 + S and then decrements S.
        /// </summary>
        /// <param name="bus">The bus used to perform the write.</param>
        /// <param name="cpuState">The CPU state to read and write S from.</param>
        /// <param name="data">The data to write into memory.</param>
        private static void Push(IBus bus, CpuState cpuState, byte data)
        {
            bus.Write((ushort)(0x0100 + cpuState.S), data);
            cpuState.S--;
        }

        /// <summary>
        /// Increments S and then reads the byte into the memory location 0x0100 + S.
        /// </summary>
        /// <param name="bus">The bus used to perform the read.</param>
        /// <param name="cpuState">The CPU state to read and write S from.</param>
        /// <returns>The data read from memory.</returns>
        private static byte Pop(IBus bus, CpuState cpuState)
        {
            cpuState.S++;
            return bus.Read((ushort)(0x0100 + cpuState.S));
        }

        /// <summary>
        /// Determines if two addresses are different pages. Pages are defined by the high byte of the 16-bit address.
        /// If the high byte of two 16-bit addresses are different, they are on two separate pages.
        /// </summary>
        /// <param name="address1">The first address to compare.</param>
        /// <param name="address2">The second address to compare.</param>
        /// <returns>true if the addresses are on separate pages; otherwise, false.</returns>
        private static bool IsDifferentPage(ushort address1, ushort address2) => (address1 & 0xff00) != (address2 & 0xff00);

        /// <summary>
        /// Performs a jump if <paramref name="condition"/> is true. Conditional branch instructions only use the
        /// relative addressing mode which gives them a range of -128 bytes to +127 bytes. If a branch is taken, an
        /// additional cycle is added as a penalty. If a branch is taken and the target address is in a different page
        /// than page the PC currently resides in, another penalty cycle is added for a maximum of two penalty cycles
        /// added onto the base cycle cost.
        /// </summary>
        /// <param name="baseCycles">The number of cycles the instruction would take without penalties.</param>
        /// <param name="bus">The bus used to perform reads and writes.</param>
        /// <param name="cpuState">The CPU state.</param>
        /// <param name="condition">true if the jump is to be taken; otherwise, false.</param>
        /// <returns>The number of cycles used to execute this instruction taking into account any penalties.</returns>
        private static int PerformConditionalJump(int baseCycles, IBus bus, CpuState cpuState, bool condition)
        {
            var (address, _) = GetAddress(AddressingMode.Relative, bus, cpuState);

            var totalCycles = baseCycles;

            if (condition)
            {
                totalCycles++;

                var oldPc = cpuState.PC;
                cpuState.PC = address;

                // If the new PC is on a different page, add an additional cycle penalty.
                if (IsDifferentPage(oldPc, address))
                {
                    totalCycles++;
                }

                return totalCycles;
            }

            return totalCycles;
        }

        private static int Aac(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            var (data, address, additionalCycles) = FetchData(addressingMode, bus, cpuState);

            var result = (byte)(cpuState.A & data);

            cpuState.SetZeroFlag(data);
            cpuState.SetNegativeFlag(data);

            // If the result is negative (MSB is set), set carry flag.
            // TODO: Do we clear carry flag if result is positive? This should be tested in Visual 6502.
            if ((result & 0x80) != 0)
            {
                cpuState.SetFlag(CpuFlags.C, true);
            }

            bus.Write(address, data);

            return baseCycles + additionalCycles;
        }

        private static int Aax(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            var (address, additionalCycles) = GetAddress(addressingMode, bus, cpuState);

            var data = (byte)(cpuState.A & cpuState.X);

            cpuState.SetZeroFlag(data);
            cpuState.SetNegativeFlag(data);

            bus.Write(address, data);

            return baseCycles + additionalCycles;
        }

        private static int Adc(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState, byte? dataIn, out byte result)
        {
            var (data, _, additionalCycles) = FetchData(addressingMode, bus, cpuState);

            if (dataIn is not null)
            {
                data = dataIn.Value;
            }

            var resultTemp = cpuState.A + data + (cpuState.GetFlag(CpuFlags.C) ? 1 : 0);
            var resultByte = (byte)(resultTemp & 0xff);

            cpuState.SetFlag(CpuFlags.C, resultTemp > 0xff);
            cpuState.SetZeroFlag(resultByte);
            cpuState.SetFlag(CpuFlags.V, ((cpuState.A ^ data) & 0x80) == 0 && ((cpuState.A ^ resultTemp) & 0x80) != 0);
            cpuState.SetNegativeFlag(resultByte);

            cpuState.A = resultByte;

            result = resultByte;

            return baseCycles + additionalCycles;
        }

        /// <summary>
        /// <para>
        ///     Performs a bitwise 'and' operation between the accumulator (on the left) and memory (on the right). The
        ///     result is stored in the accumulator.
        /// </para>
        /// <para>
        ///     Available addressing modes:
        ///     <list type="table">
        ///         <listheader>
        ///             <term>Addressing Mode</term>
        ///             <term>Assembly</term>
        ///         </listheader>
        ///         <item>
        ///             <term>Immediate</term>
        ///             <term>AND #oper</term>
        ///         </item>
        ///         <item>
        ///             <term>Zero Page</term>
        ///             <term>AND oper</term>
        ///         </item>
        ///         <item>
        ///             <term>Zero Page X</term>
        ///             <term>AND oper,x</term>
        ///         </item>
        ///         <item>
        ///             <term>Absolute</term>
        ///             <term>AND oper</term>
        ///         </item>
        ///         <item>
        ///             <term>Absolute X</term>
        ///             <term>AND oper,x</term>
        ///         </item>
        ///         <item>
        ///             <term>Absolute Y</term>
        ///             <term>AND oper,y</term>
        ///         </item>
        ///         <item>
        ///             <term>Indirect X</term>
        ///             <term>AND (oper,x)</term>
        ///         </item>
        ///         <item>
        ///             <term>Indirect Y</term>
        ///             <term>AND (oper),y</term>
        ///         </item>
        ///     </list>
        /// </para>
        /// <para>
        ///     Effect on flags:
        ///     <list type="table">
        ///         <listheader>
        ///             <term>Flag</term>
        ///             <term>Result</term>
        ///         </listheader>
        ///         <item>
        ///             <term>N (Negative)</term>
        ///             <term>Set based on result in A</term>
        ///         </item>
        ///         <item>
        ///             <term>Z (Zero)</term>
        ///             <term>Set based on result in A</term>
        ///         </item>
        ///         <item>
        ///             <term>C (Carry)</term>
        ///             <term>Unchanged</term>
        ///         </item>
        ///         <item>
        ///             <term>I (Interrupt)</term>
        ///             <term>Unchanged</term>
        ///         </item>
        ///         <item>
        ///             <term>D (Decimal)</term>
        ///             <term>Unchanged</term>
        ///         </item>
        ///         <item>
        ///             <term>V (Overflow)</term>
        ///             <term>Unchanged</term>
        ///         </item>
        ///     </list>
        /// </para>
        /// </summary>
        /// <param name="addressingMode">The addressing mode.</param>
        /// <param name="baseCycles">The number of cycles the instruction would take without penalties.</param>
        /// <param name="bus">The bus used to perform reads and writes.</param>
        /// <param name="cpuState">The CPU state.</param>
        /// <param name="dataIn">Optionally specifies the data that is to be used for the operation. If provided, this overrides the data that would ordinarily be gathered from the data fetch. The data fetch will still occur.</param>
        /// <param name="result">Contains the result of the operation.</param>
        /// <returns>The number of cycles used to execute this instruction taking into account any penalties.</returns>
        private static int And(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState, byte? dataIn, out byte result)
        {
            var (data, _, additionalCycles) = FetchData(addressingMode, bus, cpuState);

            if (dataIn is not null)
            {
                data = dataIn.Value;
            }

            cpuState.A = (byte)(cpuState.A & data);

            cpuState.SetZeroFlag(cpuState.A);
            cpuState.SetNegativeFlag(cpuState.A);

            result = cpuState.A;

            return baseCycles + additionalCycles;
        }

        private static int Asl(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState, byte? dataIn, out byte result)
        {
            // Note: This instruction operators on and stores the result to either memory or accumulator.
            // Note: This instruction always takes a constant amount of cycles to complete.

            var (data, address, _) = FetchData(addressingMode, bus, cpuState);

            if (dataIn is not null)
            {
                data = dataIn.Value;
            }

            cpuState.SetFlag(CpuFlags.C, (data & 0x80) != 0); // Carry flag is set to the bit that is being shifted out.

            data = (byte)(data << 1);

            cpuState.SetZeroFlag(data);
            cpuState.SetNegativeFlag(data);

            if (addressingMode == AddressingMode.Accumulator)
            {
                cpuState.A = data;
            }
            else
            {
                bus.Write(address, data);
            }

            result = data;

            return baseCycles;
        }

        private static int Axs(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            var (data, _, _) = FetchData(addressingMode, bus, cpuState);

            cpuState.X = (byte)(cpuState.A & cpuState.X);

            cpuState.X -= data;

            return baseCycles;
        }

        private static int Bcc(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            return PerformConditionalJump(baseCycles, bus, cpuState, !cpuState.GetFlag(CpuFlags.C));
        }

        private static int Bcs(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            return PerformConditionalJump(baseCycles, bus, cpuState, cpuState.GetFlag(CpuFlags.C));
        }

        private static int Beq(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            return PerformConditionalJump(baseCycles, bus, cpuState, cpuState.GetFlag(CpuFlags.Z));
        }

        private static int Bit(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            var (data, _, _) = FetchData(addressingMode, bus, cpuState);

            cpuState.SetZeroFlag((byte)(data & cpuState.A));
            cpuState.SetFlag(CpuFlags.V, (data & 0x40) != 0); // Set overflow flag to bit 6 of data.
            cpuState.SetNegativeFlag(data);

            return baseCycles;
        }

        private static int Bmi(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            return PerformConditionalJump(baseCycles, bus, cpuState, cpuState.GetFlag(CpuFlags.N));
        }

        private static int Bne(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            return PerformConditionalJump(baseCycles, bus, cpuState, !cpuState.GetFlag(CpuFlags.Z));
        }

        private static int Bpl(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            return PerformConditionalJump(baseCycles, bus, cpuState, !cpuState.GetFlag(CpuFlags.N));
        }

        private static int Brk(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            // The BRK instruction saves the address that is two after the BRK instruction. This allows for the
            // application to store a byte of data directly after the BRK instruction, perhaps to signify the type of
            // interrupt the application wants to process.
            cpuState.PC++;

            var pcHigh = (byte)((uint)cpuState.PC >> 8);
            var pcLow = (byte)(cpuState.PC & 0x00ff);

            Push(bus, cpuState, pcHigh);
            Push(bus, cpuState, pcLow);

            Push(bus, cpuState, (byte)((byte)cpuState.P | 0x30)); // Push the program state with B = 1 and U = 1.

            cpuState.SetFlag(CpuFlags.I, true);

            var addressLow = (ushort)bus.Read(0xfffe);
            var addressHigh = (ushort)(bus.Read(0xffff) << 8);

            var address = (ushort)(addressLow | addressHigh);

            cpuState.PC = address;

            return baseCycles;
        }

        private static int Bvc(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            return PerformConditionalJump(baseCycles, bus, cpuState, !cpuState.GetFlag(CpuFlags.V));
        }

        private static int Bvs(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            return PerformConditionalJump(baseCycles, bus, cpuState, cpuState.GetFlag(CpuFlags.V));
        }

        private static int Clc(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            cpuState.SetFlag(CpuFlags.C, false);
            return baseCycles;
        }

        private static int Cld(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            cpuState.SetFlag(CpuFlags.D, false);
            return baseCycles;
        }

        private static int Cli(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            cpuState.SetFlag(CpuFlags.I, false);
            return baseCycles;
        }

        private static int Clv(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            cpuState.SetFlag(CpuFlags.V, false);
            return baseCycles;
        }

        private static int Cmp(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            var (data, _, additionalCycles) = FetchData(addressingMode, bus, cpuState);

            var result = (ushort)(cpuState.A - data);

            cpuState.SetFlag(CpuFlags.C, cpuState.A >= data);
            cpuState.SetZeroFlag(result);
            cpuState.SetNegativeFlag(result);

            return baseCycles + additionalCycles;
        }

        private static int Cpx(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            var (data, _, additionalCycles) = FetchData(addressingMode, bus, cpuState);

            var result = (ushort)(cpuState.X - data);

            cpuState.SetFlag(CpuFlags.C, cpuState.X >= data);
            cpuState.SetZeroFlag(result);
            cpuState.SetNegativeFlag(result);

            return baseCycles + additionalCycles;
        }

        private static int Cpy(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            var (data, _, additionalCycles) = FetchData(addressingMode, bus, cpuState);

            var result = (ushort)(cpuState.Y - data);

            cpuState.SetFlag(CpuFlags.C, cpuState.Y >= data);
            cpuState.SetZeroFlag(result);
            cpuState.SetNegativeFlag(result);

            return baseCycles + additionalCycles;
        }

        private static int Dec(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            // No addressing mode of this instruction incurs additional addressing penalties. All cycles are counted in
            // the base cycles of this instruction.
            var (address, _) = GetAddress(addressingMode, bus, cpuState);

            var data = bus.Read(address);

            data--;

            bus.Write(address, data);

            cpuState.SetZeroFlag(data);
            cpuState.SetNegativeFlag(data);

            return baseCycles;
        }

        private static int Dex(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            cpuState.X--;

            cpuState.SetZeroFlag(cpuState.X);
            cpuState.SetNegativeFlag(cpuState.X);

            return baseCycles;
        }

        private static int Dey(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            cpuState.Y--;

            cpuState.SetZeroFlag(cpuState.Y);
            cpuState.SetNegativeFlag(cpuState.Y);

            return baseCycles;
        }

        // Undocumented Instruction
        private static int Dop(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            FetchData(addressingMode, bus, cpuState);

            return baseCycles;
        }

        private static int Eor(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            var (data, _, additionalCycles) = FetchData(addressingMode, bus, cpuState);

            cpuState.A = (byte)(cpuState.A ^ data);

            cpuState.SetZeroFlag(cpuState.A);
            cpuState.SetNegativeFlag(cpuState.A);

            return baseCycles + additionalCycles;
        }

        private static int Inc(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            // No addressing mode of this instruction incurs additional addressing penalties. All cycles are counted in
            // the base cycles of this instruction.
            var (address, _) = GetAddress(addressingMode, bus, cpuState);

            var data = bus.Read(address);

            data++;

            bus.Write(address, data);

            cpuState.SetZeroFlag(data);
            cpuState.SetNegativeFlag(data);

            return baseCycles;
        }

        private static int Inx(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            cpuState.X++;

            cpuState.SetZeroFlag(cpuState.X);
            cpuState.SetNegativeFlag(cpuState.X);

            return baseCycles;
        }

        private static int Iny(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            cpuState.Y++;

            cpuState.SetZeroFlag(cpuState.Y);
            cpuState.SetNegativeFlag(cpuState.Y);

            return baseCycles;
        }

        // This instruction is supposed to lock up the CPU. We'll just do nothing.
        private static int Kil(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            return baseCycles;
        }

        private static int Jmp(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            var (address, _) = GetAddress(addressingMode, bus, cpuState);

            cpuState.PC = address;

            return baseCycles;
        }

        private static int Jsr(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            var (address, _) = GetAddress(addressingMode, bus, cpuState);

            cpuState.PC--;

            var pcHigh = (byte)((uint)cpuState.PC >> 8);
            var pcLow = (byte)(cpuState.PC & 0x00ff);

            Push(bus, cpuState, pcHigh);
            Push(bus, cpuState, pcLow);

            cpuState.PC = address;

            return baseCycles;
        }

        // Undocumented Instruction
        private static int Lax(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            var (data, _, additionalCycles) = FetchData(addressingMode, bus, cpuState);

            cpuState.A = data;
            cpuState.X = data;

            cpuState.SetZeroFlag(cpuState.A);
            cpuState.SetNegativeFlag(cpuState.A);

            return baseCycles + additionalCycles;
        }

        private static int Lda(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            var (data, _, additionalCycles) = FetchData(addressingMode, bus, cpuState);

            cpuState.A = data;

            cpuState.SetZeroFlag(cpuState.A);
            cpuState.SetNegativeFlag(cpuState.A);

            return baseCycles + additionalCycles;
        }

        private static int Ldx(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            var (data, _, additionalCycles) = FetchData(addressingMode, bus, cpuState);

            cpuState.X = data;

            cpuState.SetZeroFlag(cpuState.X);
            cpuState.SetNegativeFlag(cpuState.X);

            return baseCycles + additionalCycles;
        }

        private static int Ldy(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            var (data, _, additionalCycles) = FetchData(addressingMode, bus, cpuState);

            cpuState.Y = data;

            cpuState.SetZeroFlag(cpuState.Y);
            cpuState.SetNegativeFlag(cpuState.Y);

            return baseCycles + additionalCycles;
        }

        private static int Lsr(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            // Note: This instruction operators on and stores the result to either memory or accumulator.
            // Note: This instruction always takes a constant amount of cycles to complete.

            var (data, address, _) = FetchData(addressingMode, bus, cpuState);

            cpuState.SetFlag(CpuFlags.C, (data & 0x01) != 0); // Carry flag is set to the bit that is being shifted out.

            data = (byte)(data >> 1);

            cpuState.SetZeroFlag(data);
            cpuState.SetNegativeFlag(data);

            if (addressingMode == AddressingMode.Accumulator)
            {
                cpuState.A = data;
            }
            else
            {
                bus.Write(address, data);
            }

            return baseCycles;
        }

        private static int Nop(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            return baseCycles;
        }

        private static int Ora(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState, byte? dataIn, out byte result)
        {
            var (data, _, additionalCycles) = FetchData(addressingMode, bus, cpuState);

            if (dataIn is not null)
            {
                data = dataIn.Value;
            }

            cpuState.A = (byte)(cpuState.A | data);

            cpuState.SetZeroFlag(cpuState.A);
            cpuState.SetNegativeFlag(cpuState.A);

            result = cpuState.A;

            return baseCycles + additionalCycles;
        }

        private static int Pha(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            Push(bus, cpuState, cpuState.A);

            return baseCycles;
        }

        private static int Php(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            Push(bus, cpuState, (byte)((byte)cpuState.P | 0x30)); // Push the program state with B = 1 and U = 1.

            return baseCycles;
        }

        private static int Pla(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            cpuState.A = Pop(bus, cpuState);

            cpuState.SetZeroFlag(cpuState.A);
            cpuState.SetNegativeFlag(cpuState.A);

            return baseCycles;
        }

        private static int Plp(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            cpuState.P = (CpuFlags)Pop(bus, cpuState);

            return baseCycles;
        }

        private static int Rla(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            // Note: This instruction always takes a constant amount of cycles to complete.

            Rol(addressingMode, baseCycles, bus, cpuState, null, out var result);
            And(AddressingMode.Dummy, baseCycles, bus, cpuState, result, out _);

            return baseCycles;
        }

        private static int Rol(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState, byte? dataIn, out byte result)
        {
            // Note: This instruction operators on and stores the result to either memory or accumulator.
            // Note: This instruction always takes a constant amount of cycles to complete.

            var (data, address, _) = FetchData(addressingMode, bus, cpuState);

            if (dataIn is not null)
            {
                data = dataIn.Value;
            }

            var newCarry = (data & 0x80) != 0;

            data = (byte)(data << 1);

            if (cpuState.GetFlag(CpuFlags.C))
            {
                data |= 0x01;
            }

            cpuState.SetFlag(CpuFlags.C, newCarry);
            cpuState.SetZeroFlag(data);
            cpuState.SetNegativeFlag(data);

            if (addressingMode == AddressingMode.Accumulator)
            {
                cpuState.A = data;
            }
            else
            {
                bus.Write(address, data);
            }

            result = data;

            return baseCycles;
        }

        private static int Ror(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState, byte? dataIn, out byte result)
        {
            // Note: This instruction operators on and stores the result to either memory or accumulator.
            // Note: This instruction always takes a constant amount of cycles to complete.

            var (data, address, _) = FetchData(addressingMode, bus, cpuState);

            if (dataIn is not null)
            {
                data = dataIn.Value;
            }

            var newCarry = (data & 0x01) != 0;

            data = (byte)(data >> 1);

            if (cpuState.GetFlag(CpuFlags.C))
            {
                data |= 0x80;
            }

            cpuState.SetFlag(CpuFlags.C, newCarry);
            cpuState.SetZeroFlag(data);
            cpuState.SetNegativeFlag(data);

            if (addressingMode == AddressingMode.Accumulator)
            {
                cpuState.A = data;
            }
            else
            {
                bus.Write(address, data);
            }

            result = data;

            return baseCycles;
        }

        private static int Rra(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            // Note: This instruction always takes a constant amount of cycles to complete.

            Ror(addressingMode, baseCycles, bus, cpuState, null, out var result);
            Adc(AddressingMode.Dummy, baseCycles, bus, cpuState, result, out _);

            return baseCycles;
        }

        private static int Rti(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            cpuState.P = (CpuFlags)Pop(bus, cpuState);

            var pcLow = (ushort)Pop(bus, cpuState);
            var pcHigh = (ushort)(Pop(bus, cpuState) << 8);

            cpuState.PC = (ushort)(pcLow | pcHigh);

            return baseCycles;
        }

        private static int Rts(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            var pcLow = (ushort)Pop(bus, cpuState);
            var pcHigh = (ushort)(Pop(bus, cpuState) << 8);

            cpuState.PC = (ushort)(pcLow | pcHigh);
            cpuState.PC++;

            return baseCycles;
        }

        private static int Sbc(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            var (data, _, additionalCycles) = FetchData(addressingMode, bus, cpuState);

            var result = cpuState.A - data - (cpuState.GetFlag(CpuFlags.C) ? 0 : 1);
            var resultByte = (byte)(result & 0xff);

            cpuState.SetFlag(CpuFlags.C, (ushort)result < 0x100);
            cpuState.SetZeroFlag(resultByte);
            cpuState.SetFlag(CpuFlags.V, ((cpuState.A ^ data) & 0x80) != 0 && ((cpuState.A ^ result) & 0x80) != 0);
            cpuState.SetNegativeFlag(resultByte);

            cpuState.A = resultByte;

            return baseCycles + additionalCycles;
        }

        private static int Sec(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            cpuState.SetFlag(CpuFlags.C, true);
            return baseCycles;
        }

        private static int Sed(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            cpuState.SetFlag(CpuFlags.D, true);
            return baseCycles;
        }

        private static int Sei(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            cpuState.SetFlag(CpuFlags.I, true);
            return baseCycles;
        }

        private static int Slo(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            // Note: This instruction always takes a constant amount of cycles to complete.

            Asl(addressingMode, baseCycles, bus, cpuState, null, out var result);
            Ora(AddressingMode.Dummy, baseCycles, bus, cpuState, result, out _);

            return baseCycles;
        }

        private static int Sta(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            // Note: This instruction always takes a constant amount of cycles to complete.
            var (address, _) = GetAddress(addressingMode, bus, cpuState);

            bus.Write(address, cpuState.A);

            return baseCycles;
        }

        private static int Stx(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            var (address, _) = GetAddress(addressingMode, bus, cpuState);

            bus.Write(address, cpuState.X);

            return baseCycles;
        }

        private static int Sty(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            var (address, _) = GetAddress(addressingMode, bus, cpuState);

            bus.Write(address, cpuState.Y);

            return baseCycles;
        }

        private static int Tax(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            cpuState.X = cpuState.A;

            cpuState.SetZeroFlag(cpuState.X);
            cpuState.SetNegativeFlag(cpuState.X);

            return baseCycles;
        }

        private static int Tay(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            cpuState.Y = cpuState.A;

            cpuState.SetZeroFlag(cpuState.Y);
            cpuState.SetNegativeFlag(cpuState.Y);

            return baseCycles;
        }

        // Undocumented Instruction
        private static int Top(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            var (_, _, additionalCycles) = FetchData(addressingMode, bus, cpuState);

            return baseCycles + additionalCycles;
        }

        private static int Tsx(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            cpuState.X = cpuState.S;

            cpuState.SetZeroFlag(cpuState.X);
            cpuState.SetNegativeFlag(cpuState.X);

            return baseCycles;
        }

        private static int Txa(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            cpuState.A = cpuState.X;

            cpuState.SetZeroFlag(cpuState.A);
            cpuState.SetNegativeFlag(cpuState.A);

            return baseCycles;
        }

        private static int Txs(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            cpuState.S = cpuState.X;
            return baseCycles;
        }

        private static int Tya(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            cpuState.A = cpuState.Y;

            cpuState.SetZeroFlag(cpuState.A);
            cpuState.SetNegativeFlag(cpuState.A);

            return baseCycles;
        }
    }
}