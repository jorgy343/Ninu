using System;

namespace Ninu.Emulator
{
    public static class InstructionOperations
    {
        /// <summary>
        /// Gets a 16-bit address from memory based on the addressing mode. Addressing modes <see cref="AddressingMode.Implied"/>,
        /// <see cref="AddressingMode.Accumulator"/>, and <see cref="AddressingMode.Immediate"/> are not valid modes for this method.
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

                        // Compare the high byte of the address and offset address. If they are different,
                        // add an additional cycle as a penalty.
                        var additionalCycles = IsDifferentPage(address, offsetAddress) ? 1 : 0;

                        return (offsetAddress, additionalCycles);
                    }

                case AddressingMode.AbsoluteWithYOffset:
                    {
                        var addressLow = (ushort)bus.Read(cpuState.PC++);
                        var addressHigh = (ushort)(bus.Read(cpuState.PC++) << 8);

                        var address = (ushort)(addressLow | addressHigh);
                        var offsetAddress = (ushort)(address + cpuState.Y);

                        // Compare the high byte of the address and offset address. If they are different,
                        // add an additional cycle as a penalty.
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

        private static void Push(IBus bus, CpuState cpuState, byte data)
        {
            bus.Write((ushort)(0x0100 + cpuState.S), data);
            cpuState.S--;
        }

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
        /// relative addressing mode which gives them a range of -128 bytes to +127 bytes. If a branch is taken,
        /// an additional cycle is added as a penalty. If a branch is taken and the target address is in a different
        /// page than page the PC currently resides in, another penalty cycle is added for a maximum of two penalty
        /// cycles added onto the base cycle cost.
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

        public static int Adc(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState, byte? dataIn, out byte result)
        {
            var (data, _, additionalCycles) = FetchData(addressingMode, bus, cpuState);

            if (dataIn != null)
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
        ///     Performs a bitwise 'and' operation between the accumulator (on the left) and memory (on the right).
        ///     The result is stored in the accumulator.
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
        public static int And(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState, byte? dataIn, out byte result)
        {
            var (data, _, additionalCycles) = FetchData(addressingMode, bus, cpuState);

            if (dataIn != null)
            {
                data = dataIn.Value;
            }

            cpuState.A = (byte)(cpuState.A & data);

            cpuState.SetZeroFlag(cpuState.A);
            cpuState.SetNegativeFlag(cpuState.A);

            result = cpuState.A;

            return baseCycles + additionalCycles;
        }

        public static int Asl(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState, byte? dataIn, out byte result)
        {
            // Note: This instruction operators on and stores the result to either memory or accumulator.
            // Note: This instruction always takes a constant amount of cycles to complete.

            var (data, address, _) = FetchData(addressingMode, bus, cpuState);

            if (dataIn != null)
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

        public static int Bcc(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            return PerformConditionalJump(baseCycles, bus, cpuState, !cpuState.GetFlag(CpuFlags.C));
        }

        public static int Bcs(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            return PerformConditionalJump(baseCycles, bus, cpuState, cpuState.GetFlag(CpuFlags.C));
        }

        public static int Beq(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            return PerformConditionalJump(baseCycles, bus, cpuState, cpuState.GetFlag(CpuFlags.Z));
        }

        public static int Bit(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            var (data, _, _) = FetchData(addressingMode, bus, cpuState);

            cpuState.SetZeroFlag((byte)(data & cpuState.A));
            cpuState.SetFlag(CpuFlags.V, (data & 0x40) != 0); // Set overflow flag to bit 6 of data
            cpuState.SetNegativeFlag(data);

            return baseCycles;
        }

        public static int Bmi(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            return PerformConditionalJump(baseCycles, bus, cpuState, cpuState.GetFlag(CpuFlags.N));
        }

        public static int Bne(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            return PerformConditionalJump(baseCycles, bus, cpuState, !cpuState.GetFlag(CpuFlags.Z));
        }

        public static int Bpl(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            return PerformConditionalJump(baseCycles, bus, cpuState, !cpuState.GetFlag(CpuFlags.N));
        }

        public static int Brk(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            cpuState.PC++;

            var pcHigh = (byte)((uint)cpuState.PC >> 8);
            var pcLow = (byte)(cpuState.PC & 0x00ff);

            Push(bus, cpuState, pcHigh);
            Push(bus, cpuState, pcLow);

            cpuState.SetFlag(CpuFlags.B, true);

            Push(bus, cpuState, (byte)cpuState.P);

            cpuState.SetFlag(CpuFlags.I, true);

            var addressLow = (ushort)bus.Read(0xfffe);
            var addressHigh = (ushort)(bus.Read(0xffff) << 8);

            var address = (ushort)(addressLow | addressHigh);

            cpuState.PC = address;

            return baseCycles;
        }

        public static int Bvc(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            return PerformConditionalJump(baseCycles, bus, cpuState, !cpuState.GetFlag(CpuFlags.V));
        }

        public static int Bvs(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            return PerformConditionalJump(baseCycles, bus, cpuState, cpuState.GetFlag(CpuFlags.V));
        }

        public static int Clc(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            cpuState.SetFlag(CpuFlags.C, false);
            return baseCycles;
        }

        public static int Cld(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            cpuState.SetFlag(CpuFlags.D, false);
            return baseCycles;
        }

        public static int Cli(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            cpuState.SetFlag(CpuFlags.I, false);
            return baseCycles;
        }

        public static int Clv(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            cpuState.SetFlag(CpuFlags.V, false);
            return baseCycles;
        }

        public static int Cmp(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            var (data, _, additionalCycles) = FetchData(addressingMode, bus, cpuState);

            var result = (ushort)(cpuState.A - data);

            cpuState.SetFlag(CpuFlags.C, cpuState.A >= data);
            cpuState.SetZeroFlag(result);
            cpuState.SetNegativeFlag(result);

            return baseCycles + additionalCycles;
        }

        public static int Cpx(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            var (data, _, additionalCycles) = FetchData(addressingMode, bus, cpuState);

            var result = (ushort)(cpuState.X - data);

            cpuState.SetFlag(CpuFlags.C, cpuState.X >= data);
            cpuState.SetZeroFlag(result);
            cpuState.SetNegativeFlag(result);

            return baseCycles + additionalCycles;
        }

        public static int Cpy(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            var (data, _, additionalCycles) = FetchData(addressingMode, bus, cpuState);

            var result = (ushort)(cpuState.Y - data);

            cpuState.SetFlag(CpuFlags.C, cpuState.Y >= data);
            cpuState.SetZeroFlag(result);
            cpuState.SetNegativeFlag(result);

            return baseCycles + additionalCycles;
        }

        public static int Dec(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            // No addressing mode of this instruction incurs additional addressing penalties. All cycles are
            // counted in the base cycles of this instruction.
            var (address, _) = GetAddress(addressingMode, bus, cpuState);

            var data = bus.Read(address);

            data--;

            bus.Write(address, data);

            cpuState.SetZeroFlag(data);
            cpuState.SetNegativeFlag(data);

            return baseCycles;
        }

        public static int Dex(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            cpuState.X--;

            cpuState.SetZeroFlag(cpuState.X);
            cpuState.SetNegativeFlag(cpuState.X);

            return baseCycles;
        }

        public static int Dey(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            cpuState.Y--;

            cpuState.SetZeroFlag(cpuState.Y);
            cpuState.SetNegativeFlag(cpuState.Y);

            return baseCycles;
        }

        // Undocumented Instruction
        public static int Dop(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            FetchData(addressingMode, bus, cpuState);

            return baseCycles;
        }

        public static int Eor(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            var (data, _, additionalCycles) = FetchData(addressingMode, bus, cpuState);

            cpuState.A = (byte)(cpuState.A ^ data);

            cpuState.SetZeroFlag(cpuState.A);
            cpuState.SetNegativeFlag(cpuState.A);

            return baseCycles + additionalCycles;
        }

        public static int Inc(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            // No addressing mode of this instruction incurs additional addressing penalties. All cycles are
            // counted in the base cycles of this instruction.
            var (address, _) = GetAddress(addressingMode, bus, cpuState);

            var data = bus.Read(address);

            data++;

            bus.Write(address, data);

            cpuState.SetZeroFlag(data);
            cpuState.SetNegativeFlag(data);

            return baseCycles;
        }

        public static int Inx(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            cpuState.X++;

            cpuState.SetZeroFlag(cpuState.X);
            cpuState.SetNegativeFlag(cpuState.X);

            return baseCycles;
        }

        public static int Iny(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            cpuState.Y++;

            cpuState.SetZeroFlag(cpuState.Y);
            cpuState.SetNegativeFlag(cpuState.Y);

            return baseCycles;
        }

        // This instruction is supposed to lock up the CPU. We'll just do nothing.
        public static int Jam(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            return baseCycles;
        }

        public static int Jmp(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            var (address, _) = GetAddress(addressingMode, bus, cpuState);

            cpuState.PC = address;

            return baseCycles;
        }

        public static int Jsr(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
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
        public static int Lax(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            var (data, _, additionalCycles) = FetchData(addressingMode, bus, cpuState);

            cpuState.A = data;
            cpuState.X = data;

            cpuState.SetZeroFlag(cpuState.A);
            cpuState.SetNegativeFlag(cpuState.A);

            return baseCycles + additionalCycles;
        }

        public static int Lda(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            var (data, _, additionalCycles) = FetchData(addressingMode, bus, cpuState);

            cpuState.A = data;

            cpuState.SetZeroFlag(cpuState.A);
            cpuState.SetNegativeFlag(cpuState.A);

            return baseCycles + additionalCycles;
        }

        public static int Ldx(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            var (data, _, additionalCycles) = FetchData(addressingMode, bus, cpuState);

            cpuState.X = data;

            cpuState.SetZeroFlag(cpuState.X);
            cpuState.SetNegativeFlag(cpuState.X);

            return baseCycles + additionalCycles;
        }

        public static int Ldy(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            var (data, _, additionalCycles) = FetchData(addressingMode, bus, cpuState);

            cpuState.Y = data;

            cpuState.SetZeroFlag(cpuState.Y);
            cpuState.SetNegativeFlag(cpuState.Y);

            return baseCycles + additionalCycles;
        }

        public static int Lsr(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
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

        public static int Nop(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            return baseCycles;
        }

        public static int Ora(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState, byte? dataIn, out byte result)
        {
            var (data, _, additionalCycles) = FetchData(addressingMode, bus, cpuState);

            if (dataIn != null)
            {
                data = dataIn.Value;
            }

            cpuState.A = (byte)(cpuState.A | data);

            cpuState.SetZeroFlag(cpuState.A);
            cpuState.SetNegativeFlag(cpuState.A);

            result = cpuState.A;

            return baseCycles + additionalCycles;
        }

        public static int Pha(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            Push(bus, cpuState, cpuState.A);

            return baseCycles;
        }

        public static int Php(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            Push(bus, cpuState, (byte)(cpuState.P | CpuFlags.B));

            return baseCycles;
        }

        public static int Pla(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            cpuState.A = Pop(bus, cpuState);

            cpuState.SetZeroFlag(cpuState.A);
            cpuState.SetNegativeFlag(cpuState.A);

            return baseCycles;
        }

        public static int Plp(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            cpuState.P = (CpuFlags)Pop(bus, cpuState);

            cpuState.SetFlag(CpuFlags.U, true);
            cpuState.SetFlag(CpuFlags.B, false);

            return baseCycles;
        }

        public static int Rla(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            // Note: This instruction always takes a constant amount of cycles to complete.

            Rol(addressingMode, baseCycles, bus, cpuState, null, out var result);
            And(AddressingMode.Dummy, baseCycles, bus, cpuState, result, out _);

            return baseCycles;
        }

        public static int Rol(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState, byte? dataIn, out byte result)
        {
            // Note: This instruction operators on and stores the result to either memory or accumulator.
            // Note: This instruction always takes a constant amount of cycles to complete.

            var (data, address, _) = FetchData(addressingMode, bus, cpuState);

            if (dataIn != null)
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

        public static int Ror(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState, byte? dataIn, out byte result)
        {
            // Note: This instruction operators on and stores the result to either memory or accumulator.
            // Note: This instruction always takes a constant amount of cycles to complete.

            var (data, address, _) = FetchData(addressingMode, bus, cpuState);

            if (dataIn != null)
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

        public static int Rra(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            // Note: This instruction always takes a constant amount of cycles to complete.

            Ror(addressingMode, baseCycles, bus, cpuState, null, out var result);
            Adc(AddressingMode.Dummy, baseCycles, bus, cpuState, result, out _);

            return baseCycles;
        }

        public static int Rti(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            cpuState.P = (CpuFlags)Pop(bus, cpuState);

            cpuState.SetFlag(CpuFlags.U, true);
            cpuState.SetFlag(CpuFlags.B, false);

            var pcLow = (ushort)Pop(bus, cpuState);
            var pcHigh = (ushort)(Pop(bus, cpuState) << 8);

            cpuState.PC = (ushort)(pcLow | pcHigh);

            return baseCycles;
        }

        public static int Rts(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            var pcLow = (ushort)Pop(bus, cpuState);
            var pcHigh = (ushort)(Pop(bus, cpuState) << 8);

            cpuState.PC = (ushort)(pcLow | pcHigh);
            cpuState.PC++;

            return baseCycles;
        }

        public static int Sax(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            var (address, additionalCycles) = GetAddress(addressingMode, bus, cpuState);

            var data = (byte)(cpuState.A & cpuState.X);

            cpuState.SetZeroFlag(data);
            cpuState.SetNegativeFlag(data);

            bus.Write(address, data);

            return baseCycles + additionalCycles;
        }

        public static int Sbc(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
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

        public static int Sbx(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            var (data, _, _) = FetchData(addressingMode, bus, cpuState);

            cpuState.X = (byte)(cpuState.A & cpuState.X);

            cpuState.X -= data;

            return baseCycles;
        }

        public static int Sec(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            cpuState.SetFlag(CpuFlags.C, true);
            return baseCycles;
        }

        public static int Sed(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            cpuState.SetFlag(CpuFlags.D, true);
            return baseCycles;
        }

        public static int Sei(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            cpuState.SetFlag(CpuFlags.I, true);
            return baseCycles;
        }

        public static int Slo(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            // Note: This instruction always takes a constant amount of cycles to complete.

            Asl(addressingMode, baseCycles, bus, cpuState, null, out var result);
            Ora(AddressingMode.Dummy, baseCycles, bus, cpuState, result, out _);

            return baseCycles;
        }

        public static int Sta(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            // Note: This instruction always takes a constant amount of cycles to complete.
            var (address, _) = GetAddress(addressingMode, bus, cpuState);

            bus.Write(address, cpuState.A);

            return baseCycles;
        }

        public static int Stx(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            var (address, _) = GetAddress(addressingMode, bus, cpuState);

            bus.Write(address, cpuState.X);

            return baseCycles;
        }

        public static int Sty(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            var (address, _) = GetAddress(addressingMode, bus, cpuState);

            bus.Write(address, cpuState.Y);

            return baseCycles;
        }

        public static int Tax(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            cpuState.X = cpuState.A;

            cpuState.SetZeroFlag(cpuState.X);
            cpuState.SetNegativeFlag(cpuState.X);

            return baseCycles;
        }

        public static int Tay(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            cpuState.Y = cpuState.A;

            cpuState.SetZeroFlag(cpuState.Y);
            cpuState.SetNegativeFlag(cpuState.Y);

            return baseCycles;
        }

        // Undocumented Instruction
        public static int Top(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            var (_, _, additionalCycles) = FetchData(addressingMode, bus, cpuState);

            return baseCycles + additionalCycles;
        }

        public static int Tsx(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            cpuState.X = cpuState.S;

            cpuState.SetZeroFlag(cpuState.X);
            cpuState.SetNegativeFlag(cpuState.X);

            return baseCycles;
        }

        public static int Txa(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            cpuState.A = cpuState.X;

            cpuState.SetZeroFlag(cpuState.A);
            cpuState.SetNegativeFlag(cpuState.A);

            return baseCycles;
        }

        public static int Txs(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            cpuState.S = cpuState.X;
            return baseCycles;
        }

        public static int Tya(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)
        {
            cpuState.A = cpuState.Y;

            cpuState.SetZeroFlag(cpuState.A);
            cpuState.SetNegativeFlag(cpuState.A);

            return baseCycles;
        }
    }
}