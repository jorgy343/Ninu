using System.ComponentModel;
using System.Windows.Media;

namespace Ninu.Models
{
    public class PaletteColors : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public Color Palette0Color0 { get; set; }
        public Color Palette0Color1 { get; set; }
        public Color Palette0Color2 { get; set; }
        public Color Palette0Color3 { get; set; }

        public Color Palette1Color0 { get; set; }
        public Color Palette1Color1 { get; set; }
        public Color Palette1Color2 { get; set; }
        public Color Palette1Color3 { get; set; }

        public Color Palette2Color0 { get; set; }
        public Color Palette2Color1 { get; set; }
        public Color Palette2Color2 { get; set; }
        public Color Palette2Color3 { get; set; }

        public Color Palette3Color0 { get; set; }
        public Color Palette3Color1 { get; set; }
        public Color Palette3Color2 { get; set; }
        public Color Palette3Color3 { get; set; }

        public Color Palette4Color0 { get; set; }
        public Color Palette4Color1 { get; set; }
        public Color Palette4Color2 { get; set; }
        public Color Palette4Color3 { get; set; }

        public Color Palette5Color0 { get; set; }
        public Color Palette5Color1 { get; set; }
        public Color Palette5Color2 { get; set; }
        public Color Palette5Color3 { get; set; }

        public Color Palette6Color0 { get; set; }
        public Color Palette6Color1 { get; set; }
        public Color Palette6Color2 { get; set; }
        public Color Palette6Color3 { get; set; }

        public Color Palette7Color0 { get; set; }
        public Color Palette7Color1 { get; set; }
        public Color Palette7Color2 { get; set; }
        public Color Palette7Color3 { get; set; }
    }
}