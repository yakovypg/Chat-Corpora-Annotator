using LiveCharts;
using System;
using System.Collections.Generic;

namespace ChatCorporaAnnotator.Data.WinFormsIntegration.Presenters.Views
{
    public interface IChartView : IView
    {
        ChartValues<int> ChartValues { get; set; }
        void DrawChart(List<DateTime> days, List<int> counts);
    }
}
