using ChatCorporaAnnotator.Data.Windows;
using ChatCorporaAnnotator.Infrastructure.Commands;
using ChatCorporaAnnotator.Models.Chat;
using ChatCorporaAnnotator.ViewModels.Base;
using IndexEngine;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;

namespace ChatCorporaAnnotator.ViewModels.Chat
{
    internal class DatesViewModel : ViewModel
    {
        public bool IsAvtiveDatesLoadingInProgress { get; private set; }

        public ObservableCollection<MessageDate> ActiveDates { get; private set; }

        private MessageDate _selectedDate;
        public MessageDate SelectedDate
        {
            get => _selectedDate;
            set => SetValue(ref _selectedDate, value);
        }

        #region Commands

        public ICommand SetAllActiveDatesCommand { get; }
        public bool CanSetAllActiveDatesCommandExecute(object parameter)
        {
            return !IsAvtiveDatesLoadingInProgress;
        }
        public void OnSetAllActiveDatesCommandExecuted(object parameter)
        {
            if (!CanSetAllActiveDatesCommandExecute(parameter))
                return;

            IsAvtiveDatesLoadingInProgress = true;

            var window = new WindowFinder().Find(typeof(Views.Windows.MainWindow));
            _ = SetActiveDatesAsync(window.Dispatcher);
        }

        public ICommand MoveToSelectedDateCommand { get; }
        public bool CanMoveToSelectedDateExecute(object parameter)
        {
            return true;
        }
        public void OnMoveToSelectedDateExecuted(object parameter)
        {
            if (!CanMoveToSelectedDateExecute(parameter))
                return;
        }

        #endregion

        public DatesViewModel()
        {
            ActiveDates = new ObservableCollection<MessageDate>();

            SetAllActiveDatesCommand = new RelayCommand(OnSetAllActiveDatesCommandExecuted, CanSetAllActiveDatesCommandExecute);
            MoveToSelectedDateCommand = new RelayCommand(OnMoveToSelectedDateExecuted, CanMoveToSelectedDateExecute);
        }

        public void ClearData()
        {
            ActiveDates.Clear();
        }

        private void SetActiveDates(IEnumerable<DateTime> dates)
        {
            IEnumerable<MessageDate> msgDates = ToMessageDates(dates);
            ActiveDates = new ObservableCollection<MessageDate>(msgDates);

            OnPropertyChanged(nameof(ActiveDates));
        }

        private IEnumerable<MessageDate> ToMessageDates(IEnumerable<DateTime> datesCollection)
        {
            var dateSet = new HashSet<MessageDate>();

            foreach (var date in datesCollection)
            {
                MessageDate msgDate = new MessageDate(date);
                dateSet.Add(msgDate);
            }

            return dateSet;
        }

        private async Task SetActiveDatesAsync(Dispatcher dispatcher)
        {
            await Task.Run(delegate
            {
                HashSet<DateTime> dates = IndexHelper.LoadAllActiveDates();
                dispatcher?.Invoke(() => SetActiveDates(dates));

                IsAvtiveDatesLoadingInProgress = false;
            });
        }
    }
}
