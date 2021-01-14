using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using Ninu.Base;
using Ninu.Emulator;
using Ninu.Emulator.CentralProcessor;
using Ninu.Emulator.CentralProcessor.Profilers;
using Ninu.Emulator.GraphicsProcessor;
using Ninu.Models;
using System;
using System.Collections.Generic;
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
    public class MainWindowViewModel : ViewModelBase, IDisposable
    {
        private Thread _renderingThread;
        private readonly ManualResetEvent _resetEvent = new(false);

        private readonly byte[] _patternRom1Pixels = new byte[128 * 128 * 4];
        private readonly byte[] _patternRom2Pixels = new byte[128 * 128 * 4];

        private readonly byte[] _pixels = new byte[256 * 240 * 4];

        private readonly InputManager _inputManager;

        public Console Console { get; }

        public CpuStateModel CpuState { get; } = new();
        public PaletteColors PaletteColors { get; } = new();

        public int SelectedPalette { get; set; }

        public WriteableBitmap PatternTable1Bitmap { get; } = new(128, 128, 96, 96, PixelFormats.Bgra32, null);
        public WriteableBitmap PatternTable2Bitmap { get; } = new(128, 128, 96, 96, PixelFormats.Bgra32, null);

        public WriteableBitmap GameImageBitmap { get; } = new(256, 240, 96, 96, PixelFormats.Bgra32, null);

        public ICommand LoadRom { get; }
        public ICommand SaveState { get; }
        public ICommand LoadState { get; }

        public MainWindowViewModel(InputManager inputManager)
        {
            _inputManager = inputManager ?? throw new ArgumentNullException(nameof(inputManager));

            _inputManager.AcquireAll();

            _inputManager.SetMappings(new[]
            {
                new InputMapping(_inputManager._joysticks[0], DirectInputButton.JoystickButton1, GamepadButtons.A),
                new InputMapping(_inputManager._joysticks[0], DirectInputButton.JoystickButton2, GamepadButtons.B),
                new InputMapping(_inputManager._joysticks[0], DirectInputButton.JoystickButton7, GamepadButtons.Select),
                new InputMapping(_inputManager._joysticks[0], DirectInputButton.JoystickButton8, GamepadButtons.Start),
                new InputMapping(_inputManager._joysticks[0], DirectInputButton.JoystickPov0North, GamepadButtons.Up),
                new InputMapping(_inputManager._joysticks[0], DirectInputButton.JoystickPov0East, GamepadButtons.Right),
                new InputMapping(_inputManager._joysticks[0], DirectInputButton.JoystickPov0South, GamepadButtons.Down),
                new InputMapping(_inputManager._joysticks[0], DirectInputButton.JoystickPov0West, GamepadButtons.Left),
            });

            var loggerFactory = LoggerFactory.Create(x =>
            {
                x.ClearProviders();
                x.AddDebug();

                x.SetMinimumLevel(LogLevel.Trace);
            });

            Console = new Console(loggerFactory, loggerFactory.CreateLogger<Console>());

            LoadRom = new RelayCommand(x =>
            {
                StopRendering();

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
                    StartRendering();
                }
            });

            SaveState = new RelayCommand(x =>
            {
                StopRendering();

                var data = Emulator.SaveState.Save(Console);
                File.WriteAllBytes(@"C:\Users\Jorgy\Desktop\save-state.json", data);

                StartRendering();
            });

            LoadState = new RelayCommand(x =>
            {
                StopRendering();

                var data = File.ReadAllBytes(@"C:\Users\Jorgy\Desktop\save-state.json");
                Emulator.SaveState.Load(Console, data);

                StartRendering();
            });

            _resetEvent.Reset();

            _renderingThread = new Thread(BackgroundEmulation)
            {
                Name = "Emulation Thread",
            };
        }

        public void AcquireDevices()
        {
            _inputManager.AcquireAll();
        }

        public void Dispose()
        {
            _inputManager.Dispose();
        }

        public void StartRendering()
        {
            CompositionTarget.Rendering += Render;

            _resetEvent.Reset();

            _renderingThread = new Thread(BackgroundEmulation)
            {
                Name = "Rendering Thread",
            };

            _renderingThread.Start();
        }

        public void StopRendering()
        {
            _resetEvent.Set();
            SpinWait.SpinUntil(() => !_renderingThread.IsAlive);

            CompositionTarget.Rendering -= Render;
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
        }

        private void BackgroundEmulation()
        {
            const double targetFrameRateDelta = 1000.0 / 60.0;

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var previousTime = stopwatch.Elapsed;

            while (!_resetEvent.WaitOne(0))
            {
                while ((stopwatch.Elapsed - previousTime).TotalMilliseconds < targetFrameRateDelta) ;

                previousTime = stopwatch.Elapsed;

                var pressedButtons = _inputManager.GetPressedButtons();
                var controllerData = pressedButtons.ToControlByte();
                Console.Controllers.SetControllerData(0, controllerData);

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

        private void UpdateInstructions(NewCpu cpu)
        {
            IEnumerable<string> DecodeInstructions(ushort address, int count)
            {
                for (var i = 0; i < count; i++)
                {
                    var opCode = Console.Read(address);
                    var instruction = Instruction.GetByOpCode(opCode);

                    yield return instruction.Name;

                    address += (ushort)instruction.Size;
                }
            }

            CpuState.Instructions.Clear();

            foreach (var decodedInstruction in DecodeInstructions(cpu.CpuState.PC, 16))
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