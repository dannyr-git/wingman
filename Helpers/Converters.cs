using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using System;
using Windows.UI;

namespace wingman.Helpers
{
    public class ProgressBarValueToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is double progress)
            {
                var gradientStops = (GradientStopCollection)Application.Current.Resources["ProgressRingGradientStops"];
                var gradientStopCount = gradientStops.Count;
                var position = progress / 100.0 * (gradientStopCount - 1);
                var index = (int)Math.Floor(position);
                var color1 = gradientStops[index].Color;
                var color2 = gradientStops[index + 1].Color;
                var offset1 = gradientStops[index].Offset;
                var offset2 = gradientStops[index + 1].Offset;
                var weight = (position - index) / (offset2 - offset1);
                var color = Color.FromArgb(
                    (byte)(color1.A + (color2.A - color1.A) * weight),
                    (byte)(color1.R + (color2.R - color1.R) * weight),
                    (byte)(color1.G + (color2.G - color1.G) * weight),
                    (byte)(color1.B + (color2.B - color1.B) * weight));
                return new SolidColorBrush(color);
            }
            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class NullToRedBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            // If the value is null, return a red brush
            if (value == null)
            {
                return new SolidColorBrush(Colors.Red);
            }
            // Otherwise return a transparent brush
            else
            {
                return new SolidColorBrush(Colors.Transparent);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

}
