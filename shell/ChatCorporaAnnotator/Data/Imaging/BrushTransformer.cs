using System.Windows.Media;

namespace ChatCorporaAnnotator.Data.Imaging
{
    internal static class BrushTransformer
    {
        public static SolidColorBrush ToSolidColorBrush(string value)
        {
            return new SolidColorBrush((Color)ColorConverter.ConvertFromString(value));
        }

        public static SolidColorBrush ToSolidColorBrush(System.Drawing.Color color)
        {
            return new SolidColorBrush(ColorTransformer.ToWindowsColor(color));
        }
    }
}
