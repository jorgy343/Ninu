using Ninu.Emulator;
using System.Windows.Media;

namespace Ninu
{
    public static class Color4Extensions
    {
        public static Color ToMediaColor(this Color4 color)
        {
            return Color.FromArgb(color.A, color.R, color.G, color.B);
        }
    }
}