using ChatCorporaAnnotator.Infrastructure.Commands;
using ChatCorporaAnnotator.Infrastructure.Extensions;
using ChatCorporaAnnotator.Models.Chat;
using ChatCorporaAnnotator.ViewModels.Base;
using IndexEngine.Indexes;
using IndexEngine.Paths;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace ChatCorporaAnnotator.ViewModels.Chat
{
    internal class TagsViewModel : ViewModel
    {
        private readonly MainWindowViewModel _mainWindowVM;

        public ObservableCollection<Tag> CurrentTagset { get; private set; }

        private Tag _selectedTag;
        public Tag SelectedTag
        {
            get => _selectedTag;
            set => SetValue(ref _selectedTag, value);
        }

        #region AddingCommands

        public ICommand SetTagsetCommand { get; }
        public bool CanSetTagsetCommandExecute(object parameter)
        {
            return true;
        }
        public void OnSetTagsetCommandExecuted(object parameter)
        {
            if (!CanSetTagsetCommandExecute(parameter))
                return;

            IEnumerable<Tag> newTags = parameter is IEnumerable<Tag> tags
                ? tags
                : GetTagset();

            if (newTags.IsNullOrEmpty())
            {
                CurrentTagset.Clear();
                return;
            }

            CurrentTagset = new ObservableCollection<Tag>(newTags);
            OnPropertyChanged(nameof(CurrentTagset));

            _mainWindowVM.IsProjectChanged = true;
            _mainWindowVM.SetTagsetNameCommand?.Execute(ProjectInfo.Tagset);
        }

        #endregion

        public TagsViewModel(MainWindowViewModel mainWindowVM)
        {
            _mainWindowVM = mainWindowVM ?? throw new ArgumentNullException(nameof(mainWindowVM));
            CurrentTagset = new ObservableCollection<Tag>();

            SetTagsetCommand = new RelayCommand(OnSetTagsetCommandExecuted, CanSetTagsetCommandExecute);
        }

        public void ClearData()
        {
            CurrentTagset.Clear();
        }

        private IEnumerable<Tag> GetTagset()
        {
            TagsetIndex.GetInstance().IndexCollection.TryGetValue(ProjectInfo.Tagset, out var tagset);

            return tagset.IsNullOrEmpty()
                ? new Tag[0]
                : tagset.Select(t => new Tag(t.Key, t.Value));
        }
    }
}
