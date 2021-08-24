using System;
using System.Collections.Generic;
using System.Drawing;

namespace ChatCorporaAnnotator.Data.WinFormsIntegration.Presenters.Views
{
    public interface IHeatmapView : IView
    {
        List<Color> Colors { get; set; }

        void DrawHeatmap(List<string> DateBlocks);
        void FillDates(List<DateTime> dates);
    }
}
