using ChatCorporaAnnotator.Infrastructure.Commands;
using ChatCorporaAnnotator.Models.Chat;
using ChatCorporaAnnotator.Models.Messages;
using ChatCorporaAnnotator.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace ChatCorporaAnnotator.ViewModels.Windows
{
    internal class MergeSituationsWindowViewModel : ViewModel
    {
        #region Actions

        public Func<Situation, Situation, Tuple<bool, string>> CheckDataFunc { get; set; }
        public Func<Situation, Situation, bool> MergeSituationsFunc { get; set; }

        #endregion

        #region Data

        public IEnumerable<Situation> Situations { get; }

        private Situation _firstSelectedSituation;
        public Situation FirstSelectedSituation
        {
            get => _firstSelectedSituation;
            set => SetValue(ref _firstSelectedSituation, value);
        }

        private Situation _secondSelectedSituation;
        public Situation SecondSelectedSituation
        {
            get => _secondSelectedSituation;
            set => SetValue(ref _secondSelectedSituation, value);
        }

        #endregion

        #region Strings

        private string _title;
        public string Title
        {
            get => _title;
            set => SetValue(ref _title, value);
        }

        private string _hint;
        public string Hint
        {
            get => _hint;
            set => SetValue(ref _hint, value);
        }

        private string _textBetweenComboBoxes;
        public string TextBetweenComboBoxes
        {
            get => _textBetweenComboBoxes;
            set => SetValue(ref _textBetweenComboBoxes, value);
        }

        private string _performButtonContent;
        public string PerformButtonContent
        {
            get => _performButtonContent;
            set => SetValue(ref _performButtonContent, value);
        }

        #endregion

        #region Commands

        public ICommand CloseWindowCommand { get; }

        public ICommand PerformMergeActionCommand { get; }
        public bool CanPerformMergeActionCommandExecute(object parameter)
        {
            return parameter is System.Windows.Window;
        }
        public void OnPerformMergeActionCommandExecuted(object parameter)
        {
            if (!CanPerformMergeActionCommandExecute(parameter))
                return;

            Tuple<bool, string> checkResult = CheckDataFunc?.Invoke(FirstSelectedSituation, SecondSelectedSituation) ??
                new Tuple<bool, string>(true, null);

            if (!checkResult.Item1)
            {
                new QuickMessage(checkResult.Item2).ShowInformation();
                return;
            }

            bool mergeResult = MergeSituationsFunc?.Invoke(FirstSelectedSituation, SecondSelectedSituation) ?? true;

            if (!mergeResult)
            {
                new QuickMessage("Failed to merge situations.").ShowError();
                return;
            }

            CloseWindowCommand.Execute(parameter);
        }

        #endregion

        public MergeSituationsWindowViewModel(IEnumerable<Situation> situations, string title = null)
        {
            if (situations == null || situations.Count() < 2)
                throw new ArgumentException("The number of situations must be greater than 2.");

            Title = title;
            Situations = situations;
            FirstSelectedSituation = situations.First();
            SecondSelectedSituation = situations.First();

            CloseWindowCommand = new CloseWindowCommand();
            PerformMergeActionCommand = new RelayCommand(OnPerformMergeActionCommandExecuted, CanPerformMergeActionCommandExecute);
        }
    }
}
