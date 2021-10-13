using ChatCorporaAnnotator.Data.Windows;
using ChatCorporaAnnotator.Infrastructure.Commands;
using ChatCorporaAnnotator.Models.Chat;
using ChatCorporaAnnotator.ViewModels.Base;
using IndexEngine;
using IndexingServices.Containers;
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
        private readonly ChatViewModel _chatVM;

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
            return SelectedDate != null;
        }
        public void OnMoveToSelectedDateExecuted(object parameter)
        {
            if (!CanMoveToSelectedDateExecute(parameter))
                return;

            int shiftIndex = SelectedDate.MessageId - 1;

            if (shiftIndex < 0)
                shiftIndex = 0;

            _chatVM.ShiftChatPageCommand.Execute(shiftIndex);
        }

        #endregion

        public DatesViewModel(ChatViewModel chatVM)
        {
            _chatVM = chatVM ?? throw new ArgumentNullException(nameof(chatVM));

            ActiveDates = new ObservableCollection<MessageDate>();

            SetAllActiveDatesCommand = new RelayCommand(OnSetAllActiveDatesCommandExecuted, CanSetAllActiveDatesCommandExecute);
            MoveToSelectedDateCommand = new RelayCommand(OnMoveToSelectedDateExecuted, CanMoveToSelectedDateExecute);
        }

        public void ClearData()
        {
            ActiveDates.Clear();
        }

        private void SetActiveDates(IEnumerable<ActiveDate> activeDates)
        {
            IEnumerable<MessageDate> msgDates = ToMessageDates(activeDates);
            ActiveDates = new ObservableCollection<MessageDate>(msgDates);

            OnPropertyChanged(nameof(ActiveDates));
        }

        private IEnumerable<MessageDate> ToMessageDates(IEnumerable<ActiveDate> activeDates)
        {
            var dateSet = new HashSet<MessageDate>();

            foreach (var activeDate in activeDates)
            {
                MessageDate date = new MessageDate(activeDate);
                dateSet.Add(date);
            }

            return dateSet;
        }

        private async Task SetActiveDatesAsync(Dispatcher dispatcher)
        {
            await Task.Run(delegate
            {
                HashSet<ActiveDate> dates = IndexHelper.LoadAllActiveDates();
                dispatcher?.Invoke(() => SetActiveDates(dates));

                IsAvtiveDatesLoadingInProgress = false;
            });
        }
    }
}
