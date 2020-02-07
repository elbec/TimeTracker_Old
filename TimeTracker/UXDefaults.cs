using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace TimeTracker
{
    class UXDefaults
    {
        static public SolidColorBrush ColorBlue = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#003241"));
        static public SolidColorBrush ColorGreen = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6fb776"));
        static public SolidColorBrush ColorGray = Brushes.LightSlateGray;
        static public SolidColorBrush ColorWhite = Brushes.White;
    }
}
