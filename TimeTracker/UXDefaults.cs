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
        public static SolidColorBrush ColorBlue = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#003241"));
        public static SolidColorBrush ColorGreen = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6de579"));
        public static SolidColorBrush ColorGray = Brushes.LightSlateGray;
    }
}
