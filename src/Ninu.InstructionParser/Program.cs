using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Ninu.InstructionParser
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            using var reader = new StreamReader(@"C:\Users\Jorgy\Desktop\6502-instructions-new.txt");

            var regex = new Regex(@"[ ]+([^ ]+)[ ]+([^ ]+)[ ]+([^ ]+[ ]+)?([0-9A-F]{2})[ ]+([1-3]+)[ ]+([0-9]+)");

            var instructionDict = new Dictionary<int, string>();
            var instructionNames = new HashSet<string>();

            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine()!;

                var match = regex.Match(line);

                if (!match.Success)
                {
                    continue;
                }

                var addressingMode = match.Groups[1].Value;
                var instruction = match.Groups[2].Value;
                var opCode = match.Groups[4].Value;
                var byteCount = match.Groups[5].Value;
                var cycles = match.Groups[6].Value;

                instructionNames.Add(instruction.ToLowerInvariant());

                var translatedAddressingMode = addressingMode.ToLowerInvariant().Replace(" ", "") switch
                {
                    "implied"      => "Implied",
                    "accumulator"  => "Accumulator",
                    "immediate"    => "Immediate",
                    "immidiate"    => "Immediate",
                    "zeropage"     => "ZeroPage",
                    "zeropage,x"   => "ZeroPageWithXOffset",
                    "zeropage,y"   => "ZeroPageWithYOffset",
                    "absolute"     => "Absolute",
                    "absolute,x"   => "AbsoluteWithXOffset",
                    "absolute,y"   => "AbsoluteWithYOffset",
                    "indirect"     => "Indirect",
                    "(indirect,x)" => "IndirectZeroPageWithXOffset",
                    "(indirect),y" => "IndirectZeroPageWithYOffset",
                    "(indirect,y)" => "IndirectZeroPageWithYOffset",
                    "relative"     => "Relative",
                    _              => throw new InvalidOperationException(),
                };

                var instructionFunction = instruction[0].ToString().ToUpperInvariant() + instruction.Substring(1).ToLowerInvariant();

                var instructionLine = $@"new Instruction(""{instruction.ToLowerInvariant()}"", 0x{opCode.ToLowerInvariant()}, {byteCount}, {cycles}, AddressingMode.{translatedAddressingMode}, {instructionFunction}),";

                if (instructionDict.TryGetValue(Convert.ToInt32(opCode, 16), out var existing))
                {
                    instructionDict[Convert.ToInt32(opCode, 16)] = existing + " | " + instructionLine;
                }
                else
                {
                    instructionDict[Convert.ToInt32(opCode, 16)] = instructionLine;
                }
            }

            for (var i = 0; i < 256; i++)
            {
                if (!instructionDict.ContainsKey(i))
                {
                    instructionDict[i] = $@"new Instruction(""???"", 0x{i:x2}, 1, 2, AddressingMode.Implied, Nop),";
                }

                Console.WriteLine(instructionDict[i]);
            }

            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine();

            foreach (var instructionName in instructionNames.OrderBy(x => x))
            {
                Console.WriteLine($"public static int {instructionName[0].ToString().ToUpperInvariant() + instructionName.Substring(1).ToLowerInvariant()}(AddressingMode addressingMode, int baseCycles, IBus bus, CpuState cpuState)");
                Console.WriteLine("{");
                Console.WriteLine("    throw new NotImplementedException();");
                Console.WriteLine("}");
                Console.WriteLine();
            }
        }
    }
}