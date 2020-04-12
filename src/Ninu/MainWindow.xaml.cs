using Ninu.Emulator;
using Ninu.ViewModels;
using System;
using System.IO;
using System.Windows;
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

        public MainWindow()
        {
            InitializeComponent();

            DataContext = this;

            var image = new NesImage(@"C:\Users\Jorgy\Downloads\nestest.nes");
            var cartridge = new Cartridge(image);

            _console = new Console(cartridge);

            _console.Ppu.FrameComplete += Ppu_FrameComplete;

            //console.Cpu.TotalCycles = 8;
            //console.Cpu.CpuState.S = 0xfd;
            //console.Cpu.CpuState.Flags = CpuFlags.U | CpuFlags.I;
            //console.Cpu.CpuState.PC = 0xc000;

            _console.Reset();

            // Run some instructions.
            for (var i = 0; i < 50; i++)
            {
                _console.CompleteFrame();
            }

            _console.CompleteFrame();

            var pixels = new byte[256 * 240 * 4];

            for (var i = 0; i < 256 * 240; i++)
            {
                var color = SystemPalette.Colors[_console.Ppu.PreviousImageBuffer[i] % SystemPalette.Colors.Length];

                var pixelIndex = i * 4;

                pixels[pixelIndex + 0] = color.B;
                pixels[pixelIndex + 1] = color.G;
                pixels[pixelIndex + 2] = color.R;
                pixels[pixelIndex + 3] = 255;
            }

            GameImageBitmap.WritePixels(new Int32Rect(0, 0, 256, 240), pixels, 256 * 4, 0);

            UpdateInstructions(_console.Cpu);

            // Get the palette.
            var palette = _console.Ppu.PaletteRam.GetEntry(0);

            // Pattern table bitmap pixels.
            var pixels1 = new byte[128 * 128 * 4];
            var pixels2 = new byte[128 * 128 * 4];

            // Clear both pattern bitmaps.
            for (var y = 0; y < 128; y++)
            {
                for (var x = 0; x < 128; x++)
                {
                    var index = (y * 128 + x) * 4;

                    pixels1[index + 0] = 255;
                    pixels1[index + 1] = 255;
                    pixels1[index + 2] = 255;
                    pixels1[index + 3] = 255;

                    pixels2[index + 0] = 255;
                    pixels2[index + 1] = 255;
                    pixels2[index + 2] = 255;
                    pixels2[index + 3] = 255;
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

                                var color = SystemPalette.Colors[colorIndex % SystemPalette.Colors.Length];

                                pixels1[index + 0] = color.B;
                                pixels1[index + 1] = color.G;
                                pixels1[index + 2] = color.R;
                                pixels1[index + 3] = 255;
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

                                var color = SystemPalette.Colors[colorIndex % SystemPalette.Colors.Length];

                                pixels2[index + 0] = color.B;
                                pixels2[index + 1] = color.G;
                                pixels2[index + 2] = color.R;
                                pixels2[index + 3] = 255;
                            }
                        }
                    }
                }
            }

            PatternTable1Bitmap.WritePixels(new Int32Rect(0, 0, 128, 128), pixels1, 128 * 4, 0);
            PatternTable2Bitmap.WritePixels(new Int32Rect(0, 0, 128, 128), pixels2, 128 * 4, 0);

            CpuState.Update(_console.Cpu.CpuState);

            File.WriteAllText(@"C:\Users\Jorgy\Desktop\cpu.log.txt", _console.Cpu._log.ToString());
        }

        private void Ppu_FrameComplete(object source, EventArgs e)
        {
            var ppu = (Ppu)source;
        }

        private void UpdateInstructions(Cpu cpu)
        {
            CpuState.Instructions.Clear();

            foreach (var decodedInstruction in cpu.DecodeInstructions(cpu.CpuState.PC, 300))
            {
                CpuState.Instructions.Add(decodedInstruction);
            }
        }
    }
}