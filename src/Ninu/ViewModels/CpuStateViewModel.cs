using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Ninu.Emulator;

namespace Ninu.ViewModels
{
    public class CpuStateViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private byte _a;
        public byte A
        {
            get => _a;
            set => SetField(ref _a, value);
        }

        private byte _x;
        public byte X
        {
            get => _x;
            set => SetField(ref _x, value);
        }

        private byte _y;
        public byte Y
        {
            get => _y;
            set => SetField(ref _y, value);
        }

        private byte _s;
        public byte S
        {
            get => _s;
            set => SetField(ref _s, value);
        }

        private byte _p;
        public byte P
        {
            get => _p;
            set => SetField(ref _p, value);
        }

        private ushort _pc;
        public ushort PC
        {
            get => _pc;
            set => SetField(ref _pc, value);
        }

        public ObservableCollection<string> Instructions { get; } = new ObservableCollection<string>();

        private string? _selectedInstruction;
        public string? SelectedInstruction
        {
            get => _selectedInstruction;
            set => SetField(ref _selectedInstruction, value);
        }

        public void Update(CpuState cpuState)
        {
            if (cpuState == null) throw new ArgumentNullException(nameof(cpuState));

            A = cpuState.A;
            X = cpuState.X;
            Y = cpuState.Y;
            S = cpuState.S;
            P = (byte)cpuState.Flags;
            PC = cpuState.PC;
        }

        // TODO: Move these out.
        private int _selectedPalette;

        public int SelectedPalette
        {
            get => _selectedPalette;
            set => SetField(ref _selectedPalette, value);
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected void SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (!EqualityComparer<T>.Default.Equals(field, value))
            {
                field = value;
                OnPropertyChanged(propertyName);
            }
        }
    }
}