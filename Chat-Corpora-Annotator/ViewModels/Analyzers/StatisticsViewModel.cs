using ChatCorporaAnnotator.Infrastructure.Commands;
using ChatCorporaAnnotator.Models.Messages;
using ChatCorporaAnnotator.Models.Statistics;
using ChatCorporaAnnotator.ViewModels.Base;
using ChatCorporaAnnotator.ViewModels.Windows;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace ChatCorporaAnnotator.ViewModels.Analyzers
{
    internal class StatisticsViewModel : ViewModel
    {
        private readonly MainWindowViewModel _mainWindowVM;

        public ObservableCollection<StatisticsItem> CorpusStatisticsItems { get; private set; }
        public ObservableCollection<StatisticsItem> DatasetStatisticsItems { get; private set; }

        #region ProgressBarItems

        public bool IsStatisticsCaulculatingActive { get; private set; }

        private double _progressBarMinimum = 0;
        public double ProgressBarMinimum
        {
            get => _progressBarMinimum;
            set => SetValue(ref _progressBarMinimum, value);
        }

        private double _progressBarMaximum = 100;
        public double ProgressBarMaximum
        {
            get => _progressBarMaximum;
            set => SetValue(ref _progressBarMaximum, value);
        }

        private double _progressBarCurrentValue = 0;
        public double ProgressBarCurrentValue
        {
            get => _progressBarCurrentValue;
            set => SetValue(ref _progressBarCurrentValue, value);
        }

        #endregion

        #region Commands

        public ICommand CalculateStatisticsCommand { get; }
        public bool CanCalculateStatisticsCommandExecute(object parameter)
        {
            return !IsStatisticsCaulculatingActive && !_mainWindowVM.IsTagsetEditorWindowOpen;
        }
        public void OnCalculateStatisticsCommandExecuted(object parameter)
        {
            if (!CanCalculateStatisticsCommandExecute(parameter))
                return;

            var calculator = new StatisticsCalculator();

            IsStatisticsCaulculatingActive = true;

            ProgressBarCurrentValue = 0;
            ProgressBarMaximum = calculator.MaxProgressValue;

            CorpusStatisticsItems.Clear();
            DatasetStatisticsItems.Clear();

            calculator.FailedCalculating += delegate (Exception ex)
            {
                new QuickMessage($"Failed to calculate statistics. {ex.Message}").ShowError();

                ProgressBarCurrentValue = 0;
                IsStatisticsCaulculatingActive = false;
            };

            calculator.SuccessfulCalculating += delegate
            {
                DisplayCorpusStatistics(calculator.CorpusStatistics);
                DisplayDatasetStatistics(calculator.DatasetStatistics);

                ProgressBarCurrentValue = 0;
                IsStatisticsCaulculatingActive = false;
            };

            calculator.ProgressChanged += delegate (long delta, long currValue)
            {
                ProgressBarCurrentValue = currValue;
            };

            _ = calculator.CalculateAsync();
        }

        #endregion

        public StatisticsViewModel(MainWindowViewModel mainWindowVM)
        {
            _mainWindowVM = mainWindowVM ?? throw new ArgumentNullException(nameof(mainWindowVM));

            DatasetStatisticsItems = new ObservableCollection<StatisticsItem>();
            CorpusStatisticsItems = new ObservableCollection<StatisticsItem>();

            CalculateStatisticsCommand = new RelayCommand(OnCalculateStatisticsCommandExecuted, CanCalculateStatisticsCommandExecute);
        }

        public void ClearData()
        {
            CorpusStatisticsItems.Clear();
            DatasetStatisticsItems.Clear();

            ProgressBarMinimum = 0;
            ProgressBarMaximum = 100;
            ProgressBarCurrentValue = 0;
        }

        private void DisplayCorpusStatistics(IEnumerable<StatisticsItem> stats)
        {
            CorpusStatisticsItems = new ObservableCollection<StatisticsItem>(stats);
            OnPropertyChanged(nameof(CorpusStatisticsItems));
        }

        private void DisplayDatasetStatistics(IEnumerable<StatisticsItem> stats)
        {
            DatasetStatisticsItems = new ObservableCollection<StatisticsItem>(stats);
            OnPropertyChanged(nameof(DatasetStatisticsItems));
        }
    }
}
