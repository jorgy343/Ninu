using System;
using Ninu.Emulator;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Ninu.ViewModels;
using Console = Ninu.Emulator.Console;

namespace Ninu
{
    public partial class MainWindow : Window
    {
        public CpuStateViewModel CpuState { get; } = new CpuStateViewModel();

        public WriteableBitmap PatternTable1Bitmap { get; } = new WriteableBitmap(128, 128, 96, 96, PixelFormats.Bgra32, null);
        public WriteableBitmap PatternTable2Bitmap { get; } = new WriteableBitmap(128, 128, 96, 96, PixelFormats.Bgra32, null);

        public MainWindow()
        {
            InitializeComponent();

            DataContext = this;

            var image = new NesImage(@"C:\Users\Jorgy\Downloads\nestest.nes");
            var cartridge = new Cartridge(image);
            var console = new Console(cartridge);

            //console.Cpu.TotalCycles = 8;
            //console.Cpu.CpuState.S = 0xfd;
            //console.Cpu.CpuState.Flags = CpuFlags.U | CpuFlags.I;
            //console.Cpu.CpuState.PC = 0xc000;

            console.Reset();

            // Run some instructions.
            for (var i = 0; i < 100_000; i++)
            {
                console.Clock();
            }

            // Get the palette.
            var palette = console.Ppu.PaletteRam.GetEntry(0);

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
                }
            }

            for (var tileY = 0; tileY < 16; tileY++)
            {
                for (var tileX = 0; tileX < 16; tileX++)
                {
                    var tile = console.Ppu.GetPatternTile(tileY * 16 + tileX);

                    var xOffset = tileX * 8;
                    var yOffset = tileY * 8;

                    for (var y = 0; y < 8; y++)
                    {
                        for (var x = 0; x < 8; x++)
                        {
                            var index = ((y + yOffset) * 128 + (x + xOffset)) * 4;

                            var paletteIndex = tile.GetPaletteIndex(x, y);

                            var colorIndex = paletteIndex switch
                            {
                                PaletteIndex.Color0 => palette.Byte1,
                                PaletteIndex.Color1 => palette.Byte2,
                                PaletteIndex.Color2 => palette.Byte3,
                                PaletteIndex.Color3 => palette.Byte4,
                                _ => throw new ArgumentOutOfRangeException(),
                            };

                            var color = SystemPalette.Colors[colorIndex];

                            pixels1[index + 0] = color.B;
                            pixels1[index + 1] = color.G;
                            pixels1[index + 2] = color.R;
                            pixels1[index + 3] = 255;
                        }
                    }
                }
            }

            PatternTable1Bitmap.WritePixels(new Int32Rect(0, 0, 128, 128), pixels1, 128 * 4, 0);

            CpuState.Update(console.Cpu.CpuState);

            File.WriteAllText(@"C:\Users\Jorgy\Desktop\cpu.log.txt", console.Cpu._log.ToString());
        }
    }
}