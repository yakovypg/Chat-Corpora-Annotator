using ChatCorporaAnnotator.Data.WinFormsIntegration.Presenters.Views;
using IndexEngine.Paths;
using System;
using System.Linq;

namespace ChatCorporaAnnotator.Data.WinFormsIntegration.Presenters
{
    public class ChartPresenter
    {
        private readonly IMainView _main;
        private readonly IChartView _chart;

        public ChartPresenter(IMainView main, IChartView chart)
        {
            _main = main;
            _chart = chart;
            _main.ChartClick += _main_ChartClick;
        }

        private void _main_ChartClick(object sender, EventArgs e)
        {
            _chart.DrawChart(ProjectInfo.Data.MessagesPerDay.Keys.ToList(), ProjectInfo.Data.MessagesPerDay.Values.ToList());
            _chart.ShowView();
        }
    }
}
