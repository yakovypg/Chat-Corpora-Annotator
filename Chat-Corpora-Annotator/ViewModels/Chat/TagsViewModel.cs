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

        public ObservableCollection<Tag> Tags { get; private set; }

        #region Commands

        public ICommand SetTagsCommand { get; }
        public bool CanSetTagsCommandExecute(object parameter)
        {
            return true;
        }
        public void OnSetTagsCommandExecuted(object parameter)
        {
            if (!CanSetTagsCommandExecute(parameter))
                return;

            IEnumerable<Tag> newTags = parameter is IEnumerable<Tag> tags
                ? tags
                : GetTags();

            if (newTags.IsNullOrEmpty())
            {
                Tags.Clear();
                return;
            }

            Tags = new ObservableCollection<Tag>(newTags);
            OnPropertyChanged(nameof(Tags));

            _mainWindowVM.SetTagsetNameCommand?.Execute(ProjectInfo.Tagset);
        }

        public ICommand AddTagsCommand { get; }
        public bool CanAddTagsCommandExecute(object parameter)
        {
            return parameter is IEnumerable<Tag>;
        }
        public void OnAddTagsCommandExecuted(object parameter)
        {
            if (!CanAddTagsCommandExecute(parameter))
                return;

            IEnumerable<Tag> addingTags = parameter as IEnumerable<Tag>;

            if (addingTags.IsNullOrEmpty())
                return;

            Tags = new ObservableCollection<Tag>(Tags.Concat(addingTags));
            OnPropertyChanged(nameof(Tags));

            _mainWindowVM.SetTagsetNameCommand?.Execute(ProjectInfo.Tagset);
        }

        #endregion

        public TagsViewModel(MainWindowViewModel mainWindowVM)
        {
            _mainWindowVM = mainWindowVM ?? throw new ArgumentNullException(nameof(mainWindowVM));
            Tags = new ObservableCollection<Tag>();

            SetTagsCommand = new RelayCommand(OnSetTagsCommandExecuted, CanSetTagsCommandExecute);
            AddTagsCommand = new RelayCommand(OnAddTagsCommandExecuted, CanAddTagsCommandExecute);
        }

        public void ClearData()
        {
            Tags.Clear();
        }

        private IEnumerable<Tag> GetTags()
        {
            TagsetIndex.GetInstance().IndexCollection.TryGetValue(ProjectInfo.Tagset, out var tags);

            return tags.IsNullOrEmpty()
                ? new Tag[0]
                : tags.Select(t => new Tag(t.Key, t.Value));
        }
    }
}
