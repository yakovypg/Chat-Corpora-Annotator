using System.Drawing;

namespace ColorEngine
{
    public struct Rgb : IColorModel
    {
        public byte R { get; }
        public byte G { get; }
        public byte B { get; }

        public Rgb() : this(0, 0, 0)
        {
        }

        public Rgb(byte r, byte g, byte b)
        {
            R = r;
            G = g;
            B = b;
        }

        public Color GetColor()
        {
            return Color.FromArgb(R, G, B);
        }

        public static byte CoefficientToRgbValue(double rgbCoefficient)
        {
            return Convert.ToByte(rgbCoefficient * 255);
        }

        public static Rgb FromCoefficients(IList<double> rgbCoefficients)
        {
            if (rgbCoefficients.Count != 3)
                throw new ArgumentException("The number of coefficients should be equal to three.");

            double rCoef = rgbCoefficients[0];
            double gCoef = rgbCoefficients[1];
            double bCoef = rgbCoefficients[2];

            return FromCoefficients(rCoef, gCoef, bCoef);
        }

        public static Rgb FromCoefficients(double rCoef, double gCoef, double bCoef)
        {
            byte r = CoefficientToRgbValue(rCoef);
            byte g = CoefficientToRgbValue(gCoef);
            byte b = CoefficientToRgbValue(bCoef);

            return new Rgb(r, g, b);
        }
    }
}
