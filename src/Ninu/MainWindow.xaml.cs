// ReSharper disable ShiftExpressionRealShiftCountIsZero
using Ninu.Emulator;
using Ninu.ViewModels;
using System;
using System.IO;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Console = Ninu.Emulator.Console;

namespace Ninu
{
    public partial class MainWindow : Window
    {
        public CpuStateViewModel CpuState { get; } = new CpuStateViewModel();

        public WriteableBitmap PatternTable1Bitmap { get; } = new WriteableBitmap(128, 128, 96, 96, PixelFormats.Bgra32, null);
        public WriteableBitmap PatternTable2Bitmap { get; } = new WriteableBitmap(128, 128, 96, 96, PixelFormats.Bgra32, null);

        public WriteableBitmap GameImageBitmap { get; } = new WriteableBitmap(256, 240, 96, 96, PixelFormats.Bgra32, null);

        private readonly Console _console;

        private readonly byte[] _patternRom1Pixels = new byte[128 * 128 * 4];
        private readonly byte[] _patternRom2Pixels = new byte[128 * 128 * 4];

        public MainWindow()
        {
            InitializeComponent();

            DataContext = this;

            var image = new NesImage(@"C:\Users\Jorgy\Downloads\Super Mario Bros. (Japan, USA).nes");
            var cartridge = new Cartridge(image);

            _console = new Console(cartridge);
            _console.Reset();

            const double fps = 1000.0 / 60.0;

            var timer = new Timer(fps);
            timer.Elapsed += TimerTick;

            timer.Start();
        }

        private void TimerTick(object sender, ElapsedEventArgs e)
        {
            lock (_console)
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

                _console.Controllers.SetControllerData(0, controllerData);

                _console.CompleteFrame();

                var pixels = new byte[256 * 240 * 4];

                for (var i = 0; i < 256 * 240; i++)
                {
                    var color = SystemPalette.Colors[_console.Ppu.PreviousImageBuffer[i]];

                    var pixelIndex = i * 4;

                    pixels[pixelIndex + 0] = color.B;
                    pixels[pixelIndex + 1] = color.G;
                    pixels[pixelIndex + 2] = color.R;
                    pixels[pixelIndex + 3] = 255;
                }

                Dispatcher.Invoke(() =>
                {
                    CpuState.Update(_console.Cpu.CpuState);

                    UpdateInstructions(_console.Cpu);

                    UpdatePatternRoms();

                    GameImageBitmap.WritePixels(new Int32Rect(0, 0, 256, 240), pixels, 256 * 4, 0);
                });
            }
        }

        private void UpdateInstructions(Cpu cpu)
        {
            CpuState.Instructions.Clear();

            foreach (var decodedInstruction in cpu.DecodeInstructions(cpu.CpuState.PC, 16))
            {
                CpuState.Instructions.Add(decodedInstruction);
            }
        }

        private void UpdatePatternRoms()
        {
            // Get the palette.
            var palette = _console.Ppu.PaletteRam.GetEntry(CpuState.SelectedPalette);

            // Clear both pattern bitmaps.
            for (var y = 0; y < 128; y++)
            {
                for (var x = 0; x < 128; x++)
                {
                    var index = (y * 128 + x) * 4;

                    _patternRom1Pixels[index + 0] = 255;
                    _patternRom1Pixels[index + 1] = 255;
                    _patternRom1Pixels[index + 2] = 255;
                    _patternRom1Pixels[index + 3] = 255;

                    _patternRom2Pixels[index + 0] = 255;
                    _patternRom2Pixels[index + 1] = 255;
                    _patternRom2Pixels[index + 2] = 255;
                    _patternRom2Pixels[index + 3] = 255;
                }
            }

            for (var tileY = 0; tileY < 16; tileY++)
            {
                for (var tileX = 0; tileX < 16; tileX++)
                {
                    var leftTile = _console.Ppu.GetPatternTile(PatternTableEntry.Left, tileY * 16 + tileX);
                    var rightTile = _console.Ppu.GetPatternTile(PatternTableEntry.Right, tileY * 16 + tileX);

                    var xOffset = tileX * 8;
                    var yOffset = tileY * 8;

                    for (var y = 0; y < 8; y++)
                    {
                        for (var x = 0; x < 8; x++)
                        {
                            var index = ((y + yOffset) * 128 + (x + xOffset)) * 4;

                            {
                                var paletteIndex = leftTile.GetPaletteColorIndex(x, y);

                                var colorIndex = paletteIndex switch
                                {
                                    PaletteColor.Color0 => palette.Byte1,
                                    PaletteColor.Color1 => palette.Byte2,
                                    PaletteColor.Color2 => palette.Byte3,
                                    PaletteColor.Color3 => palette.Byte4,
                                    _ => throw new ArgumentOutOfRangeException(),
                                };

                                var color = SystemPalette.Colors[colorIndex];

                                _patternRom1Pixels[index + 0] = color.B;
                                _patternRom1Pixels[index + 1] = color.G;
                                _patternRom1Pixels[index + 2] = color.R;
                                _patternRom1Pixels[index + 3] = 255;
                            }

                            {
                                var paletteIndex = rightTile.GetPaletteColorIndex(x, y);

                                var colorIndex = paletteIndex switch
                                {
                                    PaletteColor.Color0 => palette.Byte1,
                                    PaletteColor.Color1 => palette.Byte2,
                                    PaletteColor.Color2 => palette.Byte3,
                                    PaletteColor.Color3 => palette.Byte4,
                                    _ => throw new ArgumentOutOfRangeException(),
                                };

                                var color = SystemPalette.Colors[colorIndex];

                                _patternRom2Pixels[index + 0] = color.B;
                                _patternRom2Pixels[index + 1] = color.G;
                                _patternRom2Pixels[index + 2] = color.R;
                                _patternRom2Pixels[index + 3] = 255;
                            }
                        }
                    }
                }
            }

            PatternTable1Bitmap.WritePixels(new Int32Rect(0, 0, 128, 128), _patternRom1Pixels, 128 * 4, 0);
            PatternTable2Bitmap.WritePixels(new Int32Rect(0, 0, 128, 128), _patternRom2Pixels, 128 * 4, 0);
        }
    }
}