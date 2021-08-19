using System.Windows.Media;

namespace ChatCorporaAnnotator.Data.Imaging
{
    internal static class ColorTransformer
    {
        public static Color ToWindowsColor(System.Drawing.Color color)
        {
            return new Color()
            {
                A = color.A,
                R = color.R,
                G = color.G,
                B = color.B
            };
        }
    }
}
