using System.Drawing;

namespace ColorEngine
{
    public struct Lch : IColorModel, IRgbConvertible
    {
        public double L { get; }
        public double C { get; }
        public double H { get; }

        public Lch() : this(0, 0, 0)
        {
        }

        public Lch(double l, double c, double h)
        {
            if (l < 0 || l > 100)
                throw new ArgumentException("Lightness must be from 0 to 100.", nameof(l));

            if (c < 0 || c > 132)
                throw new ArgumentException("Chroma must be from 0 to 132.", nameof(c));

            if (h < 0 || h > 360)
                throw new ArgumentException("Hue must be from 0 to 360.", nameof(h));

            L = l;
            C = c;
            H = h;
        }

        public Rgb ToRgb()
        {
            return ColorTransformer.LchToRgb(this);
        }

        public Color GetColor()
        {
            return ToRgb().GetColor();
        }
    }
}
