using CSharpTest.Net.Collections;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace ChatCorporaAnnotator.Data.WinFormsIntegration.Services
{

    public interface IHeatmapService
    {
        List<string> DateBlocks { get; set; }
        Color HeatMapColor(double value, double min, double max);
        List<Color> PopulateHeatmap(BTreeDictionary<DateTime, int> MessagesPerDay);
    }

    public class HeatmapService : IHeatmapService
    {
        public List<string> DateBlocks { get; set; } = new List<string>();

        public Color HeatMapColor(double value, double min, double max)
        {
            double val = (value - min) / (max - min);
            int r = Convert.ToByte(255 * val);
            int g = Convert.ToByte(255 * (1 - val));
            int b = 10;

            return Color.FromArgb(255, r, g, b);
        }

        public List<Color> PopulateHeatmap(BTreeDictionary<DateTime, int> MessagesPerDay)
        {
            List<Color> colors = new List<Color>();
            double max = MessagesPerDay.Values.Max();
            double min = MessagesPerDay.Values.Min();

            double temp = 1230 / MessagesPerDay.Keys.Count;

            if (temp >= 10.0)
            {
                foreach (var date in MessagesPerDay.Keys)
                {
                    double x = MessagesPerDay[date];
                    DateBlocks.Add(date.Date.ToString());
                    colors.Add(HeatMapColor(x, min, max));
                }

                return colors;
            }
            else
            {
                DateTime[] days = new DateTime[MessagesPerDay.Keys.Count];

                MessagesPerDay.Keys.CopyTo(days, 0);

                double block = MessagesPerDay.Keys.Count / 1230.0 * 10.0;
                int blockDayCount = (int)Math.Floor(block);

                List<double> newCounts = new List<double>();

                double x = 0;
                int count = 0;
                int index = 0;

                StringBuilder blockdate = new StringBuilder();

                while (index < days.Length)
                {
                    if (days.Length - index > blockDayCount)
                    {
                        while (count < blockDayCount)
                        {
                            if (count == 0)
                                blockdate.Append(days[index].Date.ToString() + " ");

                            x += MessagesPerDay[days[index]];
                            index++;
                            count++;
                        }
                        if (count == blockDayCount)
                        {
                            blockdate.Append(days[index].Date.ToString() + " ");
                            DateBlocks.Add(blockdate.ToString());
                            blockdate = new StringBuilder();
                            x += MessagesPerDay[days[index]];
                            newCounts.Add(x);
                            count = 0;
                            x = 0;
                            index++;
                        }
                    }
                    else
                    {
                        blockdate.Append(days[index].Date.ToString() + " ");
                        x += MessagesPerDay[days[index]];
                        index++;

                        if (index == days.Length - 1)
                        {
                            DateBlocks.Add(blockdate.ToString());
                            newCounts.Add(x);
                        }
                    }
                }

                var newmax = newCounts.Max();
                var newmin = newCounts.Min();

                foreach (var cc in newCounts)
                {
                    colors.Add(HeatMapColor(cc, newmin, newmax));
                }

                return colors;
            }
        }
    }
}
