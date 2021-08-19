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
        public ObservableCollection<MessageDate> ActiveDates { get; private set; }

        #region Commands

        public ICommand SetDatesCommand { get; }
        public bool CanSetDatesCommandExecute(object parameter)
        {
            return true;
        }
        public void OnSetDatesCommandExecuted(object parameter)
        {
            if (!CanSetDatesCommandExecute(parameter))
                return;

            IEnumerable<MessageDate> msgDates = parameter is IEnumerable<DateTime> dates
                ? ToMessageDates(dates)
                : GetMessageDates();

            if (msgDates.IsNullOrEmpty())
            {
                ActiveDates.Clear();
                return;
            }

            ActiveDates = new ObservableCollection<MessageDate>(msgDates);
            OnPropertyChanged(nameof(ActiveDates));
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

        public DatesViewModel()
        {
            ActiveDates = new ObservableCollection<MessageDate>();

            SetDatesCommand = new RelayCommand(OnSetDatesCommandExecuted, CanSetDatesCommandExecute);
            AddDatesCommand = new RelayCommand(OnAddDatesCommandExecuted, CanAddDatesCommandExecute);
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

        private IEnumerable<MessageDate> GetMessageDates()
        {
            var dateSet = new HashSet<MessageDate>();

            foreach (var message in MessageContainer.Messages)
            {
                string messageContent = message.Contents[ProjectInfo.DateFieldKey].ToString();

                DateTime date = DateTime.Parse(messageContent).Date;
                MessageDate msgDate = new MessageDate(date);

                dateSet.Add(msgDate);
            }

            return dateSet;
        }
    }
}
