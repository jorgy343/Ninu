using Microsoft.Extensions.Logging;
using Ninu.Emulator;
using Ninu.Models;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Ninu.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public Console Console { get; }

        public CpuStateModel CpuState { get; } = new CpuStateModel();

        public int SelectedPalette { get; set; }

        public WriteableBitmap PatternTable1Bitmap { get; } = new WriteableBitmap(128, 128, 96, 96, PixelFormats.Bgra32, null);
        public WriteableBitmap PatternTable2Bitmap { get; } = new WriteableBitmap(128, 128, 96, 96, PixelFormats.Bgra32, null);

        public WriteableBitmap GameImageBitmap { get; } = new WriteableBitmap(256, 240, 96, 96, PixelFormats.Bgra32, null);

        public MainWindowViewModel()
        {
            var image = new NesImage(@"C:\Users\Jorgy\Downloads\Dragon_warrior.nes");

            var loggerFactory = LoggerFactory.Create(x =>
            {
                x.ClearProviders();
                x.AddDebug();

                x.SetMinimumLevel(LogLevel.Trace);
            });

            var cartridge = new Cartridge(image, loggerFactory, loggerFactory.CreateLogger<Cartridge>());

            Console = new Console(cartridge, loggerFactory, loggerFactory.CreateLogger<Console>());
            Console.Reset();
        }
    }
}