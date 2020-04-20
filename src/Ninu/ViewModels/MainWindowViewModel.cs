using Microsoft.Extensions.Logging;
using Ninu.Emulator;
using Ninu.Models;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
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

        public ICommand SaveState { get; }
        public ICommand LoadState { get; }

        public MainWindowViewModel()
        {
            var image = new NesImage(@"C:\Users\Jorgy\Downloads\Super Mario Bros. (Japan, USA).nes");

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
                Task.Run(() =>
                {
                    // The lock will synchronize this code to between frames.
                    lock (Console)
                    {
                        var context = new SaveStateContext();

                        Console.SaveState(context);

                        File.WriteAllText(@"C:\Users\Jorgy\Desktop\save-state.json", context.ToDataString());
                    }
                });
            });

            LoadState = new RelayCommand(x =>
            {
                Task.Run(() =>
                {
                    // The lock will synchronize this code to between frames.
                    lock (Console)
                    {
                        var context = new SaveStateContext(File.ReadAllBytes(@"C:\Users\Jorgy\Desktop\save-state.json"));

                        Console.LoadState(context);
                    }
                });
            });
        }
    }
}