using System.Windows.Media;

namespace TimeTracker
{
    static class UXDefaults
    {
        static readonly public SolidColorBrush ColorBlue = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#003241"));
        static readonly public SolidColorBrush ColorGreen = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6fb776"));
        static readonly public SolidColorBrush ColorGray = Brushes.DimGray;
        static readonly public SolidColorBrush ColorWhite = Brushes.White;
    }
}
