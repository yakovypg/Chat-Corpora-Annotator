using HsluvS;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace ColorEngine
{
    public static class ColorGenerator
    {
        private static readonly Random random = new Random();

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
            Color[] colors;
            HashSet<Color> temp = new HashSet<Color>();

            bool flag;
            int num;

            for (int i = 0; i < count; i++)
            {
                flag = false;
                num = random.Next(0, count);

                while (!flag)
                {
                    flag = temp.Add(ColorTranslator.FromHtml(ColorValues[i]));
                }
            }

            colors = temp.ToArray();
            return colors;
        }

        public static Color[] GenerateHSVColors(int count)
        {
            Color[] colors;

            double h;
            double s;
            double v;
            double golden_ratio_conjugate = 0.618033988749895;
            bool flag;

            HashSet<Color> temp = new HashSet<Color>();

            for (int i = 0; i < count; i++)
            {
                flag = false;

                v = random.NextDouble() * (0.99 - 0.75) + 0.75;
                s = random.NextDouble() * (0.6 - 0.4) + 0.4;

                while (!flag)
                {
                    h = random.NextDouble();
                    h += golden_ratio_conjugate;
                    h %= 1;
                    flag = temp.Add(HsvToColor(h, s, v));
                }
            }

            colors = temp.ToArray();
            return colors;
        }

        public static Color HsvToColor(double h, double s, double v)
        {
            var hInt = (int)Math.Floor(h * 6.0);
            var f = h * 6 - hInt;
            var p = v * (1 - s);
            var q = v * (1 - f * s);
            var t = v * (1 - (1 - f) * s);
            var r = 256.0;
            var g = 256.0;
            var b = 256.0;

            switch (hInt)
            {
                case 0: r = v; g = t; b = p; break;
                case 1: r = q; g = v; b = p; break;
                case 2: r = p; g = v; b = t; break;
                case 3: r = p; g = q; b = v; break;
                case 4: r = t; g = p; b = v; break;
                case 5: r = v; g = p; b = q; break;
            }

            var c = Color.FromArgb(alpha: 255,
                                   red: (byte)Math.Floor(r * 255.0),
                                   green: (byte)Math.Floor(g * 255.0),
                                   blue: (byte)Math.Floor(b * 255.0));

            return c;
        }

        public static Color[] GenerateHSLuvColors(int count, bool back = true)
        {
            Color[] colors;
            bool flag;

            HashSet<Color> temp = new HashSet<Color>();

            for (int i = 0; i < count; i++)
            {
                flag = false;

                while (!flag)
                {
                    var col = GenerateHSLuvColor(back);
                    flag = temp.Add(col);
                }
            }

            colors = temp.ToArray();
            return colors;
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

            (double, double, double) hsl = (h, s, l);
            (double coefR, double coefG, double coefB) = Hsluv.HslToRgb(hsl);

            byte r = Convert.ToByte(coefR * 255);
            byte g = Convert.ToByte(coefG * 255);
            byte b = Convert.ToByte(coefB * 255);

            return Color.FromArgb(r, g, b);
        }
    }
}
