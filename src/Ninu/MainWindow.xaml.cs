// ReSharper disable ShiftExpressionRealShiftCountIsZero
using Ninu.Emulator;
using Ninu.ViewModels;
using System.Timers;
using System.Windows;
using System.Windows.Input;

namespace Ninu
{
    public partial class MainWindow : Window
    {
        private readonly byte[] _patternRom1Pixels = new byte[128 * 128 * 4];
        private readonly byte[] _patternRom2Pixels = new byte[128 * 128 * 4];

        public MainWindowViewModel ViewModel { get; }

        public MainWindow(MainWindowViewModel viewModel)
        {
            InitializeComponent();

            ViewModel = viewModel;
            DataContext = ViewModel;
        }

        public void StartTimer()
        {
            const double fps = 1000.0 / 60.0;

            var timer = new Timer(fps);
            timer.Elapsed += TimerTick;

            timer.AutoReset = false;

            timer.Start();
        }

        private void TimerTick(object sender, ElapsedEventArgs e)
        {
            lock (ViewModel.Console)
            {
                byte controllerData = 0;

                // Update the controller state.
                Dispatcher.Invoke(() =>
                {
                    controllerData |= Keyboard.IsKeyDown(Key.S) ? (byte)(1 << 7) : (byte)0x00;
                    controllerData |= Keyboard.IsKeyDown(Key.D) ? (byte)(1 << 6) : (byte)0x00;
                    controllerData |= Keyboard.IsKeyDown(Key.W) ? (byte)(1 << 5) : (byte)0x00;
                    controllerData |= Keyboard.IsKeyDown(Key.E) ? (byte)(1 << 4) : (byte)0x00;
                    controllerData |= Keyboard.IsKeyDown(Key.Up) ? (byte)(1 << 3) : (byte)0x00;
                    controllerData |= Keyboard.IsKeyDown(Key.Down) ? (byte)(1 << 2) : (byte)0x00;
                    controllerData |= Keyboard.IsKeyDown(Key.Left) ? (byte)(1 << 1) : (byte)0x00;
                    controllerData |= Keyboard.IsKeyDown(Key.Right) ? (byte)(1 << 0) : (byte)0x00;
                });

                ViewModel.Console.Controllers.SetControllerData(0, controllerData);

                ViewModel.Console.CompleteFrame();

                var pixels = new byte[256 * 240 * 4];

                for (var i = 0; i < 256 * 240; i++)
                {
                    var color = SystemPalette.Colors[ViewModel.Console.Ppu.PreviousImageBuffer[i]];

                    var pixelIndex = i * 4;

                    pixels[pixelIndex + 0] = color.B;
                    pixels[pixelIndex + 1] = color.G;
                    pixels[pixelIndex + 2] = color.R;
                    pixels[pixelIndex + 3] = 255;
                }

                Dispatcher.Invoke(() =>
                {
                    ViewModel.CpuState.Update(ViewModel.Console.Cpu.CpuState);

                    UpdateInstructions(ViewModel.Console.Cpu);

                    UpdatePatternRoms();

                    ViewModel.GameImageBitmap.WritePixels(new Int32Rect(0, 0, 256, 240), pixels, 256 * 4, 0);
                });
            }

            ((Timer)sender).Start();
        }

        private void UpdateInstructions(Cpu cpu)
        {
            ViewModel.CpuState.Instructions.Clear();

            foreach (var decodedInstruction in cpu.DecodeInstructions(cpu.CpuState.PC, 16))
            {
                ViewModel.CpuState.Instructions.Add(decodedInstruction);
            }
        }

        private void UpdatePatternRoms()
        {
            // Clear both pattern bitmaps.
            for (var tileY = 0; tileY < 16; tileY++)
            {
                for (var tileX = 0; tileX < 16; tileX++)
                {
                    var leftTile = ViewModel.Console.Ppu.GetPatternTile(PatternTableEntry.Left, tileY * 16 + tileX);
                    var rightTile = ViewModel.Console.Ppu.GetPatternTile(PatternTableEntry.Right, tileY * 16 + tileX);

                    var xOffset = tileX * 8;
                    var yOffset = tileY * 8;

                    for (var y = 0; y < 8; y++)
                    {
                        for (var x = 0; x < 8; x++)
                        {
                            var index = ((y + yOffset) * 128 + (x + xOffset)) * 4;

                            {
                                var paletteEntryIndex = leftTile.GetPaletteColorIndex(x, y);

                                var color = SystemPalette.Colors[ViewModel.Console.Ppu.GetPaletteColor((byte)ViewModel.SelectedPalette, paletteEntryIndex)];

                                _patternRom1Pixels[index + 0] = color.B;
                                _patternRom1Pixels[index + 1] = color.G;
                                _patternRom1Pixels[index + 2] = color.R;
                                _patternRom1Pixels[index + 3] = 255;
                            }

                            {
                                var paletteEntryIndex = rightTile.GetPaletteColorIndex(x, y);

                                var color = SystemPalette.Colors[ViewModel.Console.Ppu.GetPaletteColor((byte)ViewModel.SelectedPalette, paletteEntryIndex)];

                                _patternRom2Pixels[index + 0] = color.B;
                                _patternRom2Pixels[index + 1] = color.G;
                                _patternRom2Pixels[index + 2] = color.R;
                                _patternRom2Pixels[index + 3] = 255;
                            }
                        }
                    }
                }
            }

            ViewModel.PatternTable1Bitmap.WritePixels(new Int32Rect(0, 0, 128, 128), _patternRom1Pixels, 128 * 4, 0);
            ViewModel.PatternTable2Bitmap.WritePixels(new Int32Rect(0, 0, 128, 128), _patternRom2Pixels, 128 * 4, 0);
        }
    }
}