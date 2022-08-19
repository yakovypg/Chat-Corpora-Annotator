using System.Drawing;

namespace ColorEngine
{
    public struct Hsluv : IColorModel, IRgbConvertible
    {
        public double H { get; }
        public double S { get; }
        public double L { get; }

        public Hsluv() : this(0, 0, 0)
        {
        }

        public Hsluv(double h, double s, double l)
        {
            if (h < 0 || h > 360)
                throw new ArgumentException("Hue must be from 0 to 360.", nameof(h));

            if (s < 0 || s > 100)
                throw new ArgumentException("Saturation must be from 0 to 100.", nameof(s));

            if (l < 0 || l > 100)
                throw new ArgumentException("Lightness must be from 0 to 100.", nameof(l));

            H = h;
            S = s;
            L = l;
        }

        public Rgb ToRgb()
        {
            return ColorTransformer.HsluvToRgb(this);
        }

        public Color GetColor()
        {
            return ToRgb().GetColor();
        }
    }
}
