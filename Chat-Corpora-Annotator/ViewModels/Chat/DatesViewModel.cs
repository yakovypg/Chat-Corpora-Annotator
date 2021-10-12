using ChatCorporaAnnotator.Data.Indexing;
using ChatCorporaAnnotator.Data.Windows;
using ChatCorporaAnnotator.Infrastructure.Commands;
using ChatCorporaAnnotator.Infrastructure.Extensions;
using ChatCorporaAnnotator.Models.Chat;
using ChatCorporaAnnotator.ViewModels.Base;
using IndexEngine;
using IndexEngine.Paths;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace ChatCorporaAnnotator.ViewModels.Chat
{
    internal class DatesViewModel : ViewModel
    {
        private readonly ChatViewModel _chatVM;

        public ObservableCollection<MessageDate> ActiveDates { get; private set; }

        private MessageDate _selectedDate;
        public MessageDate SelectedDate
        {
            get => _selectedDate;
            set => SetValue(ref _selectedDate, value);
        }

        #region AddingCommands

        public ICommand SetAllActiveDatesCommand { get; }
        public bool CanSetAllActiveDatesCommandExecute(object parameter)
        {
            return true;
        }
        public void OnSetAllActiveDatesCommandExecuted(object parameter)
        {
            if (!CanSetAllActiveDatesCommandExecute(parameter))
                return;

            var window = new WindowFinder().Find(typeof(Views.Windows.MainWindow));
            _ = IndexInteraction.LoadAndSetActiveDatesAsync(t => window.Dispatcher.Invoke(() => SetActiveDates(t)));
        }

        public ICommand SetDatesCommand { get; }
        public bool CanSetDatesCommandExecute(object parameter)
        {
            return true;
        }
        public void OnSetDatesCommandExecuted(object parameter)
        {
            if (!CanSetDatesCommandExecute(parameter))
                return;

            IEnumerable<DateTime> msgDates = parameter is IEnumerable<DateTime> dates
                ? dates
                : GetMessageDatesFromContainer();

            if (msgDates.IsNullOrEmpty())
            {
                ActiveDates.Clear();
                return;
            }

            SetActiveDates(msgDates);
        }

        public ICommand AddDatesCommand { get; }
        public bool CanAddDatesCommandExecute(object parameter)
        {
            return parameter is IEnumerable<DateTime>;
        }
        public void OnAddDatesCommandExecuted(object parameter)
        {
            if (!CanAddDatesCommandExecute(parameter))
                return;

            IEnumerable<MessageDate> msgDates = ToMessageDates(parameter as IEnumerable<DateTime>);

            if (msgDates.IsNullOrEmpty())
                return;

            ActiveDates = new ObservableCollection<MessageDate>(ActiveDates.Concat(msgDates));
            OnPropertyChanged(nameof(ActiveDates));
        }

        #endregion

        #region Commands

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

        public DatesViewModel(ChatViewModel chatVM)
        {
            _chatVM = chatVM ?? throw new ArgumentNullException(nameof(chatVM));

            ActiveDates = new ObservableCollection<MessageDate>();

            SetAllActiveDatesCommand = new RelayCommand(OnSetAllActiveDatesCommandExecuted, CanSetAllActiveDatesCommandExecute);
            SetDatesCommand = new RelayCommand(OnSetDatesCommandExecuted, CanSetDatesCommandExecute);
            AddDatesCommand = new RelayCommand(OnAddDatesCommandExecuted, CanAddDatesCommandExecute);

            MoveToSelectedDateCommand = new RelayCommand(OnMoveToSelectedDateExecuted, CanMoveToSelectedDateExecute);
        }

        public void ClearData()
        {
            ActiveDates.Clear();
        }

        public void SetCurrentMessagesActiveDates()
        {
            var dateSet = new HashSet<MessageDate>();

            foreach (var msg in _chatVM.MessagesVM.MessagesCase.CurrentMessages)
            {
                string messageContent = msg.Source.Contents[ProjectInfo.DateFieldKey].ToString();

                DateTime date = DateTime.Parse(messageContent).Date;
                MessageDate msgDate = new MessageDate(date);

                dateSet.Add(msgDate);
            }

            ActiveDates = new ObservableCollection<MessageDate>(dateSet);
            OnPropertyChanged(nameof(ActiveDates));
        }

        private void SetActiveDates(IEnumerable<DateTime> dates)
        {
            IEnumerable<MessageDate> msgDates = ToMessageDates(dates);
            ActiveDates = new ObservableCollection<MessageDate>(msgDates);

            OnPropertyChanged(nameof(ActiveDates));
        }

        private IEnumerable<DateTime> GetMessageDatesFromContainer()
        {
            var dateSet = new HashSet<DateTime>();

            foreach (var message in MessageContainer.Messages)
            {
                string messageContent = message.Contents[ProjectInfo.DateFieldKey].ToString();

                DateTime date = DateTime.Parse(messageContent).Date;
                dateSet.Add(date);
            }

            return dateSet;
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
    }
}
