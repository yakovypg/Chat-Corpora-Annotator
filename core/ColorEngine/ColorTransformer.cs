namespace ColorEngine
{
    public static class ColorTransformer
    {
        public static Rgb HslToRgb(Hsl hsl)
        {
            if (hsl.S == 0)
                return Rgb.FromCoefficients(hsl.L, hsl.L, hsl.L);

            double q = hsl.L < 0.5
                ? hsl.L * (1 + hsl.S)
                : hsl.L + hsl.S - hsl.L * hsl.S;

            double p = 2 * hsl.L - q;

            var rHue = new Hue(p, q, hsl.H + 1 / 3);
            var gHue = new Hue(p, q, hsl.H);
            var bHue = new Hue(p, q, hsl.H - 1 / 3);

            double rCoef = rHue.ToRgbCoefficient();
            double gCoef = gHue.ToRgbCoefficient();
            double bCoef = bHue.ToRgbCoefficient();

            return Rgb.FromCoefficients(rCoef, gCoef, bCoef);
        }

        public static Rgb HsluvToRgb(Hsluv hsluv)
        {
            var coefficients = HsluvConverter.HsluvToRgb(new double[] { hsluv.H, hsluv.S, hsluv.L });
            return Rgb.FromCoefficients(coefficients);
        }

        public static Rgb LchToRgb(Lch lch)
        {
            var coefficients = HsluvConverter.LchToRgb(new double[] { lch.L, lch.C, lch.H });
            return Rgb.FromCoefficients(coefficients);
        }
    }
}
