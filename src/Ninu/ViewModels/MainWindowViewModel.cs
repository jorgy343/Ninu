using Microsoft.Extensions.Logging;
using Ninu.Emulator;
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
        private readonly ManualResetEvent _resetEvent = new ManualResetEvent(false);

        private readonly byte[] _patternRom1Pixels = new byte[128 * 128 * 4];
        private readonly byte[] _patternRom2Pixels = new byte[128 * 128 * 4];

        private readonly byte[] _pixels = new byte[256 * 240 * 4];

        private byte _controllerData;
        private readonly object _controllerDataLock = new object();

        public Console Console { get; }

        public CpuStateModel CpuState { get; } = new CpuStateModel();

        public int SelectedPalette { get; set; }

        public WriteableBitmap PatternTable1Bitmap { get; } = new WriteableBitmap(128, 128, 96, 96, PixelFormats.Bgra32, null);
        public WriteableBitmap PatternTable2Bitmap { get; } = new WriteableBitmap(128, 128, 96, 96, PixelFormats.Bgra32, null);

        public WriteableBitmap GameImageBitmap { get; } = new WriteableBitmap(256, 240, 96, 96, PixelFormats.Bgra32, null);

        public ICommand SaveState { get; }
        public ICommand LoadState { get; }

        public MainWindowViewModel()
        {
            var image = new NesImage(@"C:\Users\Jorgy\Desktop\roms\games\Dragon_warrior.nes");

            var loggerFactory = LoggerFactory.Create(x =>
            {
                x.ClearProviders();
                x.AddDebug();

                x.SetMinimumLevel(LogLevel.Trace);
            });

            var cartridge = new Cartridge(image, loggerFactory, loggerFactory.CreateLogger<Cartridge>());

            Console = new Console(cartridge, loggerFactory, loggerFactory.CreateLogger<Console>());
            Console.Reset();

            //Console.CompleteFrame();
            //Console.CompleteFrame();
            //Console.CompleteFrame();
            //Console.CompleteFrame();
            //Console.CompleteFrame();

            //File.WriteAllText(@"C:\Users\Jorgy\Desktop\log.txt", Console.Cpu.Log.ToString());

            SaveState = new RelayCommand(x =>
            {
                StopRenderingThread();

                var context = new SaveStateContext();
                Console.SaveState(context);

                File.WriteAllText(@"C:\Users\Jorgy\Desktop\save-state.json", context.ToDataString());

                StartRenderingThread();
            });

            LoadState = new RelayCommand(x =>
            {
                StopRenderingThread();

                var context = new SaveStateContext(File.ReadAllBytes(@"C:\Users\Jorgy\Desktop\save-state.json"));
                Console.LoadState(context);

                StartRenderingThread();
            });

            _resetEvent.Reset();

            _renderingThread = new Thread(ProcessFrame)
            {
                Name = "Rendering Thread",
            };

            _renderingThread.Start();
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

            lock (_pixels)
            {
                GameImageBitmap.WritePixels(new Int32Rect(0, 0, 256, 240), _pixels, 256 * 4, 0);

                CpuState.Update(Console.Cpu.CpuState);

                UpdateInstructions(Console.Cpu);

                UpdatePatternRoms();
            }
        }

        private void ProcessFrame()
        {
            const double targetFrameRateDelta = 1000.0 / 75.0;

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

        private void UpdatePatternRoms()
        {
            // Clear both pattern bitmaps.
            for (var tileY = 0; tileY < 16; tileY++)
            {
                for (var tileX = 0; tileX < 16; tileX++)
                {
                    var leftTile = Console.Ppu.GetPatternTile(PatternTableEntry.Left, tileY * 16 + tileX);
                    var rightTile = Console.Ppu.GetPatternTile(PatternTableEntry.Right, tileY * 16 + tileX);

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