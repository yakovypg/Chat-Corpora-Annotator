using System.Drawing;

namespace ColorEngine
{
    public static class ColorGenerator
    {
        private static readonly Random random = new();

        private static readonly string[] ColorValues =
        {
            "#98724c",
            "#908f32",
            "#c8b55b",
            "#af652f",
            "#c3c13d",
            "#70a16c",
            "#e4dc8c",
            "#d3d3d3",
            "#a1d569",
            "#f59335",
            "#4ec2e8",
            "#fec7cd",
            "#95c1c0",
            "#b3b3b3",
            "#ed5466",
            "#afdb80",
            "#d2a4b4",
            "#75a1a0",
            "#a54242",
            "#de935f",
            "#cc6666",
            "#b5bd68",
            "#f0c674",
            "#81a2be",
            "#b294bb",
            "#8abeb7",
            "#c5c8c6"
        };

        public static Color[] GenerateColorsFromList(int count)
        {
            var colors = new HashSet<Color>();

            for (int i = 0; i < count; ++i)
            {
                bool flag = false;

                while (!flag)
                {
                    flag = colors.Add(ColorTranslator.FromHtml(ColorValues[i]));
                }
            }

            return colors.ToArray();
        }

        public static Color[] GenerateHSVColors(int count)
        {
            double h;
            double s;
            double v;
            double golden_ratio_conjugate = 0.618033988749895;

            var colors = new HashSet<Color>();

            for (int i = 0; i < count; ++i)
            {
                bool flag = false;

                v = random.NextDouble() * (0.99 - 0.75) + 0.75;
                s = random.NextDouble() * (0.6 - 0.4) + 0.4;

                while (!flag)
                {
                    h = random.NextDouble();
                    h += golden_ratio_conjugate;
                    h %= 1;

                    flag = colors.Add(HsvToColor(h, s, v));
                }
            }

            return colors.ToArray();
        }

        public static Color HsvToColor(double h, double s, double v)
        {
            int hInt = (int)Math.Floor(h * 6.0);
            double f = h * 6 - hInt;
            double p = v * (1 - s);
            double q = v * (1 - f * s);
            double t = v * (1 - (1 - f) * s);
            double r = 256.0;
            double g = 256.0;
            double b = 256.0;

            switch (hInt)
            {
                case 0: r = v; g = t; b = p; break;
                case 1: r = q; g = v; b = p; break;
                case 2: r = p; g = v; b = t; break;
                case 3: r = p; g = q; b = v; break;
                case 4: r = t; g = p; b = v; break;
                case 5: r = v; g = p; b = q; break;
            }

            return Color.FromArgb(
                alpha: 255,
                red: (byte)Math.Floor(r * 255.0),
                green: (byte)Math.Floor(g * 255.0),
                blue: (byte)Math.Floor(b * 255.0)
            );
        }

        public static Color[] GenerateHSLuvColors(int count, bool back = true)
        {
            var colors = new HashSet<Color>();

            for (int i = 0; i < count; ++i)
            {
                bool flag = false;

                while (!flag)
                {
                    var color = GenerateHSLuvColor(back);
                    flag = colors.Add(color);
                }
            }

            return colors.ToArray();
        }

        public static Color GenerateHSLuvColor(bool back = true)
        {
            double h;
            double s;
            double l;

            if (back)
            {
                l = random.NextDouble() * (90.0 - 55.5) + 55.5;
                s = random.NextDouble() * (67.0 - 40.0) + 40.0;
            }
            else
            {
                l = random.NextDouble() * (55.5 - 10.0) + 10.0;
                s = random.NextDouble() * (99.0 - 30.0) + 30.0;
            }

            h = random.NextDouble() * (359.0 - 1.0) + 1.0;

            Rgb rgb = ColorTransformer.HsluvToRgb(new Hsluv(h, s, l));
            return rgb.GetColor();
        }
    }
}
