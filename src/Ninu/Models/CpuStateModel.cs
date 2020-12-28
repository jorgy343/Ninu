using Ninu.Emulator.CentralProcessor;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Ninu.Models
{
    public class CpuStateModel : INotifyPropertyChanged
    {
#pragma warning disable CS0067
        public event PropertyChangedEventHandler? PropertyChanged;
#pragma warning restore CS0067

        public byte A { get; set; }
        public byte X { get; set; }
        public byte Y { get; set; }
        public byte S { get; set; }
        public byte P { get; set; }
        public ushort PC { get; set; }

        public ObservableCollection<string> Instructions { get; } = new();

        public string? SelectedInstruction { get; set; }

        public void Update(CpuState cpuState)
        {
            if (cpuState is null) throw new ArgumentNullException(nameof(cpuState));

            A = cpuState.A;
            X = cpuState.X;
            Y = cpuState.Y;
            S = cpuState.S;
            P = (byte)cpuState.P;
            PC = cpuState.PC;
        }
    }
}