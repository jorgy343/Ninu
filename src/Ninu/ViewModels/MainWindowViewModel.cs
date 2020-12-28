using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using Ninu.Emulator;
using Ninu.Emulator.CentralProcessor;
using Ninu.Emulator.CentralProcessor.Profilers;
using Ninu.Emulator.GraphicsProcessor;
using Ninu.Models;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Console = Ninu.Emulator.Console;

namespace Ninu.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private Thread _renderingThread;
        private readonly ManualResetEvent _resetEvent = new(false);

        private readonly byte[] _patternRom1Pixels = new byte[128 * 128 * 4];
        private readonly byte[] _patternRom2Pixels = new byte[128 * 128 * 4];

        private readonly byte[] _pixels = new byte[256 * 240 * 4];

        private byte _controllerData;
        private readonly object _controllerDataLock = new();

        public Console Console { get; }

        public CpuStateModel CpuState { get; } = new CpuStateModel();
        public PaletteColors PaletteColors { get; } = new PaletteColors();

        public int SelectedPalette { get; set; }

        public WriteableBitmap PatternTable1Bitmap { get; } = new WriteableBitmap(128, 128, 96, 96, PixelFormats.Bgra32, null);
        public WriteableBitmap PatternTable2Bitmap { get; } = new WriteableBitmap(128, 128, 96, 96, PixelFormats.Bgra32, null);

        public WriteableBitmap GameImageBitmap { get; } = new WriteableBitmap(256, 240, 96, 96, PixelFormats.Bgra32, null);

        public ICommand LoadRom{ get; }
        public ICommand SaveState { get; }
        public ICommand LoadState { get; }

        public MainWindowViewModel()
        {
            var loggerFactory = LoggerFactory.Create(x =>
            {
                x.ClearProviders();
                x.AddDebug();

                x.SetMinimumLevel(LogLevel.Trace);
            });

            Console = new Console(loggerFactory, loggerFactory.CreateLogger<Console>());

            Console.Cpu.AddProfiler(new NmiProfiler());

            //Console.CompleteFrame();
            //Console.CompleteFrame();
            //Console.CompleteFrame();
            //Console.CompleteFrame();
            //Console.CompleteFrame();
            //Console.CompleteFrame();

            //File.WriteAllText(@"C:\Users\Jorgy\Desktop\log.txt", Console.Cpu.Log.ToString());
            //return;

            LoadRom = new RelayCommand(x =>
            {
                StopRendering();
                StopRenderingThread();

                // TODO: Pull this out to be a service.
                var openFileDialog = new OpenFileDialog
                {
                    Title = "Load ROM File",
                    Filter = "NES ROMS (*.nes)|*.nes|All Files (*.*)|*.*",
                    CheckFileExists = true,
                    Multiselect = false,
                };

                if (openFileDialog.ShowDialog() == true)
                {
                    var image = new NesImage(openFileDialog.FileName);
                    var cartridge = new Cartridge(image, loggerFactory, loggerFactory.CreateLogger<Cartridge>());

                    Console.LoadCartridge(cartridge);
                    Console.PowerOn();

                    // TODO: If a ROM is already loaded and the user cancels the dialog, the current ROM will stop.
                    StartRenderingThread();
                    StartRendering();
                }
            });

            SaveState = new RelayCommand(x =>
            {
                StopRenderingThread();

                var data = Emulator.SaveState.Save(Console);
                File.WriteAllBytes(@"C:\Users\Jorgy\Desktop\save-state.json", data);

                StartRenderingThread();
            });

            LoadState = new RelayCommand(x =>
            {
                StopRenderingThread();

                var data = File.ReadAllBytes(@"C:\Users\Jorgy\Desktop\save-state.json");
                Emulator.SaveState.Load(Console, data);

                StartRenderingThread();
            });

            _resetEvent.Reset();

            _renderingThread = new Thread(ProcessFrame)
            {
                Name = "Rendering Thread",
            };
        }

        public void StartRendering()
        {
            CompositionTarget.Rendering += Render;
        }

        public void StopRendering()
        {
            CompositionTarget.Rendering -= Render;
        }

        public void StartRenderingThread()
        {
            _resetEvent.Reset();

            _renderingThread = new Thread(ProcessFrame)
            {
                Name = "Rendering Thread",
            };

            _renderingThread.Start();
        }

        public void StopRenderingThread()
        {
            _resetEvent.Set();
            SpinWait.SpinUntil(() => !_renderingThread.IsAlive);
        }

        private void Render(object? sender, EventArgs e)
        {
            lock (_pixels)
            {
                GameImageBitmap.WritePixels(new Int32Rect(0, 0, 256, 240), _pixels, 256 * 4, 0);

                //CpuState.Update(Console.Cpu.CpuState);
                //UpdateInstructions(Console.Cpu);

                //UpdatePaletteColors();
                //UpdatePatternRoms();
            }

            lock (_controllerDataLock)
            {
                _controllerData = 0;

                _controllerData |= Keyboard.IsKeyDown(Key.S) ? (byte)(1 << 7) : (byte)0x00;
                _controllerData |= Keyboard.IsKeyDown(Key.D) ? (byte)(1 << 6) : (byte)0x00;
                _controllerData |= Keyboard.IsKeyDown(Key.W) ? (byte)(1 << 5) : (byte)0x00;
                _controllerData |= Keyboard.IsKeyDown(Key.E) ? (byte)(1 << 4) : (byte)0x00;
                _controllerData |= Keyboard.IsKeyDown(Key.Up) ? (byte)(1 << 3) : (byte)0x00;
                _controllerData |= Keyboard.IsKeyDown(Key.Down) ? (byte)(1 << 2) : (byte)0x00;
                _controllerData |= Keyboard.IsKeyDown(Key.Left) ? (byte)(1 << 1) : (byte)0x00;
                _controllerData |= Keyboard.IsKeyDown(Key.Right) ? (byte)(1 << 0) : (byte)0x00;
            }
        }

        private void ProcessFrame()
        {
            const double targetFrameRateDelta = 1000.0 / 60.0;

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var previousTime = stopwatch.Elapsed;

            while (!_resetEvent.WaitOne(0))
            {
                while ((stopwatch.Elapsed - previousTime).TotalMilliseconds < targetFrameRateDelta) ;

                previousTime = stopwatch.Elapsed;

                lock (_controllerDataLock)
                {
                    Console.Controllers.SetControllerData(0, _controllerData);
                }

                Console.CompleteFrame();

                lock (_pixels)
                {
                    for (var i = 0; i < 256 * 240; i++)
                    {
                        var color = SystemPalette.Colors[Console.Ppu.PreviousImageBuffer[i]];

                        var pixelIndex = i * 4;

                        _pixels[pixelIndex + 0] = color.B;
                        _pixels[pixelIndex + 1] = color.G;
                        _pixels[pixelIndex + 2] = color.R;
                        _pixels[pixelIndex + 3] = 255;
                    }
                }
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

        private void UpdatePaletteColors()
        {
            PaletteColors.Palette0Color0 = SystemPalette.Colors[Console.Ppu.GetPaletteColor(0, 0)];
            PaletteColors.Palette0Color1 = SystemPalette.Colors[Console.Ppu.GetPaletteColor(0, 1)];
            PaletteColors.Palette0Color2 = SystemPalette.Colors[Console.Ppu.GetPaletteColor(0, 2)];
            PaletteColors.Palette0Color3 = SystemPalette.Colors[Console.Ppu.GetPaletteColor(0, 3)];

            PaletteColors.Palette1Color0 = SystemPalette.Colors[Console.Ppu.GetPaletteColor(1, 0)];
            PaletteColors.Palette1Color1 = SystemPalette.Colors[Console.Ppu.GetPaletteColor(1, 1)];
            PaletteColors.Palette1Color2 = SystemPalette.Colors[Console.Ppu.GetPaletteColor(1, 2)];
            PaletteColors.Palette1Color3 = SystemPalette.Colors[Console.Ppu.GetPaletteColor(1, 3)];

            PaletteColors.Palette2Color0 = SystemPalette.Colors[Console.Ppu.GetPaletteColor(2, 0)];
            PaletteColors.Palette2Color1 = SystemPalette.Colors[Console.Ppu.GetPaletteColor(2, 1)];
            PaletteColors.Palette2Color2 = SystemPalette.Colors[Console.Ppu.GetPaletteColor(2, 2)];
            PaletteColors.Palette2Color3 = SystemPalette.Colors[Console.Ppu.GetPaletteColor(2, 3)];

            PaletteColors.Palette3Color0 = SystemPalette.Colors[Console.Ppu.GetPaletteColor(3, 0)];
            PaletteColors.Palette3Color1 = SystemPalette.Colors[Console.Ppu.GetPaletteColor(3, 1)];
            PaletteColors.Palette3Color2 = SystemPalette.Colors[Console.Ppu.GetPaletteColor(3, 2)];
            PaletteColors.Palette3Color3 = SystemPalette.Colors[Console.Ppu.GetPaletteColor(3, 3)];

            PaletteColors.Palette4Color0 = SystemPalette.Colors[Console.Ppu.GetPaletteColor(4, 0)];
            PaletteColors.Palette4Color1 = SystemPalette.Colors[Console.Ppu.GetPaletteColor(4, 1)];
            PaletteColors.Palette4Color2 = SystemPalette.Colors[Console.Ppu.GetPaletteColor(4, 2)];
            PaletteColors.Palette4Color3 = SystemPalette.Colors[Console.Ppu.GetPaletteColor(4, 3)];

            PaletteColors.Palette5Color0 = SystemPalette.Colors[Console.Ppu.GetPaletteColor(5, 0)];
            PaletteColors.Palette5Color1 = SystemPalette.Colors[Console.Ppu.GetPaletteColor(5, 1)];
            PaletteColors.Palette5Color2 = SystemPalette.Colors[Console.Ppu.GetPaletteColor(5, 2)];
            PaletteColors.Palette5Color3 = SystemPalette.Colors[Console.Ppu.GetPaletteColor(5, 3)];

            PaletteColors.Palette6Color0 = SystemPalette.Colors[Console.Ppu.GetPaletteColor(6, 0)];
            PaletteColors.Palette6Color1 = SystemPalette.Colors[Console.Ppu.GetPaletteColor(6, 1)];
            PaletteColors.Palette6Color2 = SystemPalette.Colors[Console.Ppu.GetPaletteColor(6, 2)];
            PaletteColors.Palette6Color3 = SystemPalette.Colors[Console.Ppu.GetPaletteColor(6, 3)];

            PaletteColors.Palette7Color0 = SystemPalette.Colors[Console.Ppu.GetPaletteColor(7, 0)];
            PaletteColors.Palette7Color1 = SystemPalette.Colors[Console.Ppu.GetPaletteColor(7, 1)];
            PaletteColors.Palette7Color2 = SystemPalette.Colors[Console.Ppu.GetPaletteColor(7, 2)];
            PaletteColors.Palette7Color3 = SystemPalette.Colors[Console.Ppu.GetPaletteColor(7, 3)];
        }

        private void UpdatePatternRoms()
        {
            // Clear both pattern bitmaps.
            for (var tileY = 0; tileY < 16; tileY++)
            {
                for (var tileX = 0; tileX < 16; tileX++)
                {
                    var leftTile = Console.Ppu.GetPatternTile(PatternTableOffset.Left, tileY * 16 + tileX);
                    var rightTile = Console.Ppu.GetPatternTile(PatternTableOffset.Right, tileY * 16 + tileX);

                    var xOffset = tileX * 8;
                    var yOffset = tileY * 8;

                    for (var y = 0; y < 8; y++)
                    {
                        for (var x = 0; x < 8; x++)
                        {
                            var index = ((y + yOffset) * 128 + (x + xOffset)) * 4;

                            {
                                var paletteEntryIndex = leftTile.GetPaletteColorIndex(x, y);

                                var color = SystemPalette.Colors[Console.Ppu.GetPaletteColor((byte)SelectedPalette, paletteEntryIndex)];

                                _patternRom1Pixels[index + 0] = color.B;
                                _patternRom1Pixels[index + 1] = color.G;
                                _patternRom1Pixels[index + 2] = color.R;
                                _patternRom1Pixels[index + 3] = 255;
                            }

                            {
                                var paletteEntryIndex = rightTile.GetPaletteColorIndex(x, y);

                                var color = SystemPalette.Colors[Console.Ppu.GetPaletteColor((byte)SelectedPalette, paletteEntryIndex)];

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