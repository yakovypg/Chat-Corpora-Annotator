using ChatCorporaAnnotator.Infrastructure.Commands;
using ChatCorporaAnnotator.Models.Chat;
using ChatCorporaAnnotator.ViewModels.Base;
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace ChatCorporaAnnotator.ViewModels.Windows
{
    internal class TagsetEditorWindowViewModel : ViewModel
    {
        public Action DeactivateAction { get; set; }

        public ObservableCollection<string> Tagsets { get; private set; }
        public ObservableCollection<Tag> CurrentTagset { get; private set; }

        private Tag _selectedTag;
        public Tag SelectedTag
        {
            get => _selectedTag;
            set => SetValue(ref _selectedTag, value);
        }

        private string _selectedTagset;
        public string SelectedTagset
        {
            get => _selectedTagset;
            set => SetValue(ref _selectedTagset, value);
        }

        private string _consoleText;
        public string ConsoleText
        {
            get => _consoleText;
            set => SetValue(ref _consoleText, value);
        }

        #region SystemCommands

        public ICommand DeactivateWindowCommand { get; }
        public bool CanDeactivateWindowCommandExecute(object parameter)
        {
            return true;
        }
        public void OnDeactivateWindowCommandExecuted(object parameter)
        {
            if (!CanDeactivateWindowCommandExecute(parameter))
                return;

            DeactivateAction?.Invoke();
        }

        #endregion

        public TagsetEditorWindowViewModel()
        {
            DeactivateWindowCommand = new RelayCommand(OnDeactivateWindowCommandExecuted, CanDeactivateWindowCommandExecute);
        }
    }
}
