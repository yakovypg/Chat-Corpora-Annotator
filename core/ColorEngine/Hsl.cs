using System.Drawing;

namespace ColorEngine
{
    public struct Hsl : IColorModel, IRgbConvertible
    {
        public double H { get; }
        public double S { get; }
        public double L { get; }

        public Hsl() : this(0, 0, 0)
        {
        }

        public Hsl(double h, double s, double l)
        {
            if (h < 0 || h > 360)
                throw new ArgumentException("Hue must be from 0 to 360.", nameof(h));

            if (s < 0 || s > 1)
                throw new ArgumentException("Saturation must be from 0 to 1.", nameof(s));

            if (l < 0 || l > 1)
                throw new ArgumentException("Lightness must be from 0 to 1.", nameof(l));

            H = h;
            S = s;
            L = l;
        }

        public Rgb ToRgb()
        {
            return ColorTransformer.HslToRgb(this);
        }

        public Color GetColor()
        {
            return ToRgb().GetColor();
        }
    }
}
