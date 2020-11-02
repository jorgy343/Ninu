using Ninu.Emulator.CentralProcessor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Ninu.TraceLogParser
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            await using var file = File.OpenRead(@"C:\Users\Jorgy\Desktop\tracelog.txt");
            using var reader = new StreamReader(file);

            using var writer = new StreamWriter(@"C:\Users\Jorgy\Desktop\tracelog_parsed.txt");

            var regex = new Regex(@"cycle: ([0-9A-F]+) cpu_pcl: ([0-9A-F]+) cpu_a: ([0-9A-F]+) cpu_x: ([0-9A-F]+) cpu_y: ([0-9A-F]+) cpu_p: ([0-9A-F]+) cpu_pch: ([0-9A-F]+) cpu_clk0: ([0-9A-F]+) cpu_ir: ([0-9A-F]+)", RegexOptions.Compiled);

            var rows = new List<Row>();

            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync();

                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                var result = regex.Match(line);

                rows.Add(new Row
                {
                    Cycle = Convert.ToInt64(result.Groups[1].Value, 16),
                    Clock = Convert.ToInt32(result.Groups[8].Value, 16),
                    A = Convert.ToInt32(result.Groups[3].Value, 16),
                    X = Convert.ToInt32(result.Groups[4].Value, 16),
                    Y = Convert.ToInt32(result.Groups[5].Value, 16),
                    P = Convert.ToInt32(result.Groups[6].Value, 16),
                    Pcl = Convert.ToInt32(result.Groups[2].Value, 16),
                    Pch = Convert.ToInt32(result.Groups[7].Value, 16),
                    Instruction = Convert.ToInt32(result.Groups[9].Value, 16),
                });
            }

            for (var i = 11; i < rows.Count;)
            {
                var row = rows[i];

                var instruction = CpuInstruction.GetInstruction((byte)row.Instruction);

                await writer.WriteAsync($"{row.Pch:X2}{row.Pcl:X2}  {row.Instruction:X2}        {instruction.Name.ToUpperInvariant()}  A:{row.A:X2} X:{row.X:X2} Y: {row.Y:X2} CYC: {row.Cycle}");
                await writer.WriteLineAsync();

                // TODO: Some instructions have 1 or 2 cycles of variance. How should we detect this?
                i += instruction.BaseCycles * 24;
            }
        }

        public class Row
        {
            public long Cycle { get; set; }
            public int Clock { get; set; }
            public int A { get; set; }
            public int X { get; set; }
            public int Y { get; set; }
            public int P { get; set; }
            public int Pcl { get; set; }
            public int Pch { get; set; }
            public int Instruction { get; set; }
        }
    }
}