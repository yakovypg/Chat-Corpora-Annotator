using ChatCorporaAnnotator.Data.Dialogs;
using ChatCorporaAnnotator.Data.Imaging;
using ChatCorporaAnnotator.Infrastructure.Commands;
using ChatCorporaAnnotator.Models.Chat;
using ChatCorporaAnnotator.Models.Messages;
using ChatCorporaAnnotator.ViewModels.Base;
using ColorEngine;
using IndexEngine.Data.Paths;
using IndexEngine.Indexes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace ChatCorporaAnnotator.ViewModels.Windows
{
    internal class TagsetEditorWindowViewModel : ViewModel
    {
        private readonly MainWindowViewModel _mainWindowVM;

        public Action DeactivateAction { get; set; }

        #region TagsetData

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
            set
            {
                SetValue(ref _selectedTagset, value);

                if (string.IsNullOrEmpty(value))
                    return;

                Dictionary<string, Color> tagsCollection = TagsetIndex.GetInstance().IndexCollection[value];
                Tag[] tags = tagsCollection.Select(t => new Tag(t.Key, t.Value)).ToArray();

                CurrentTagset = new ObservableCollection<Tag>(tags);
                OnPropertyChanged(nameof(CurrentTagset));
            }
        }

        #endregion

        #region EditData

        public System.Windows.Media.Brush TagCreationBrush => BrushTransformer.ToSolidColorBrush(_tagCreationColor);

        private string _consoleText;
        public string ConsoleText
        {
            get => _consoleText;
            set => SetValue(ref _consoleText, value);
        }

        private Color _tagCreationColor;
        public Color TagCreationColor
        {
            get => _tagCreationColor;
            set
            {
                if (SetValue(ref _tagCreationColor, value))
                    OnPropertyChanged(nameof(TagCreationBrush));
            }
        }

        #endregion

        #region TagCommands

        public ICommand AddTagCommand { get; }
        public bool CanAddTagCommandExecute(object parameter)
        {
            return SelectedTagset != null &&
                   SelectedTagset != TagsetIndex.NOT_SELECTED_TAGSET_NAME &&
                   !string.IsNullOrEmpty(ConsoleText) &&
                   !string.IsNullOrWhiteSpace(ConsoleText) &&
                   !CurrentTagset.Select(t => t.Header).Contains(ConsoleText);
        }
        public void OnAddTagCommandExecuted(object parameter)
        {
            if (!CanAddTagCommandExecute(parameter))
                return;

            TagsetIndex.GetInstance().AddInnerIndexEntry(SelectedTagset, ConsoleText, TagCreationColor);

            var tag = new Tag(ConsoleText, TagCreationColor);
            CurrentTagset.Add(tag);

            ConsoleText = string.Empty;
            SetRandomTagCreationColor();
        }

        public ICommand DeleteTagCommand { get; }
        public bool CanDeleteTagCommandExecute(object parameter)
        {
            return SelectedTag != null;
        }
        public void OnDeleteTagCommandExecuted(object parameter)
        {
            if (!CanDeleteTagCommandExecute(parameter))
                return;

            TagsetIndex.GetInstance().DeleteInnerIndexEntry(SelectedTagset, SelectedTag.Header);
            CurrentTagset.Remove(SelectedTag);
        }

        public ICommand RenameTagCommand { get; }
        public bool CanRenameTagCommandExecute(object parameter)
        {
            return CanDeleteTagCommandExecute(null) && CanAddTagCommandExecute(null);
        }
        public void OnRenameTagCommandExecuted(object parameter)
        {
            if (!CanRenameTagCommandExecute(parameter))
                return;

            Color currTagColor = TagCreationColor;
            Color tagColor = SelectedTag.BackgroundColor;

            TagCreationColor = tagColor;

            DeleteTagCommand.Execute(null);
            AddTagCommand.Execute(null);

            TagCreationColor = currTagColor;
        }

        public ICommand ChangeTagColorCommand { get; }
        public bool CanChangeTagColorCommandExecute(object parameter)
        {
            return SelectedTag != null && SelectedTag.BackgroundColor != TagCreationColor;
        }
        public void OnChangeTagColorCommandExecuted(object parameter)
        {
            if (!CanChangeTagColorCommandExecute(parameter))
                return;

            SelectedTag.BackgroundColor = TagCreationColor;
            TagsetIndex.GetInstance().IndexCollection[SelectedTagset][SelectedTag.Header] = TagCreationColor;
        }

        #endregion

        #region TagsetCommands

        public ICommand AddTagsetCommand { get; }
        public bool CanAddTagsetCommandExecute(object parameter)
        {
            return !string.IsNullOrEmpty(ConsoleText) &&
                   !string.IsNullOrWhiteSpace(ConsoleText) &&
                   !Tagsets.Contains(ConsoleText);
        }
        public void OnAddTagsetCommandExecuted(object parameter)
        {
            if (!CanAddTagsetCommandExecute(parameter))
                return;

            TagsetIndex.GetInstance().AddIndexEntry(ConsoleText, null);
            Tagsets.Add(ConsoleText);

            SelectedTagset = ConsoleText;
            ConsoleText = string.Empty;
        }

        public ICommand DeleteTagsetCommand { get; }
        public bool CanDeleteTagsetCommandExecute(object parameter)
        {
            return SelectedTagset != null &&
                   SelectedTagset != TagsetIndex.NOT_SELECTED_TAGSET_NAME;
        }
        public void OnDeleteTagsetCommandExecuted(object parameter)
        {
            if (!CanDeleteTagsetCommandExecute(parameter))
                return;

            TagsetIndex.GetInstance().DeleteIndexEntry(SelectedTagset);
            Tagsets.Remove(SelectedTagset);

            SelectedTagset = TagsetIndex.NOT_SELECTED_TAGSET_NAME;
        }

        public ICommand RenameTagsetCommand { get; }
        public bool CanRenameTagsetCommandExecute(object parameter)
        {
            return CanDeleteTagsetCommandExecute(null) &&
                   CanAddTagsetCommandExecute(null);
        }
        public void OnRenameTagsetCommandExecuted(object parameter)
        {
            if (!CanRenameTagsetCommandExecute(parameter))
                return;

            var tagsetCollection = TagsetIndex.GetInstance().IndexCollection;
            var tagsDict = new Dictionary<string, Color>(tagsetCollection[SelectedTagset]);

            string newName = ConsoleText;

            DeleteTagsetCommand.Execute(null);
            AddTagsetCommand.Execute(null);

            tagsetCollection[newName] = tagsDict;
            SelectedTagset = newName;
        }

        public ICommand SetTagsetCommand { get; }
        public bool CanSetTagsetCommandExecute(object parameter)
        {
            return SelectedTagset != null;
        }
        public void OnSetTagsetCommandExecuted(object parameter)
        {
            if (!CanSetTagsetCommandExecute(parameter))
                return;

            var msgResult = new QuickMessage("This will delete all previously tagged messages from the project." +
                "Are you sure you want to perform this operation?").ShowWarning(MessageBoxButton.YesNo);

            if (msgResult == MessageBoxResult.No)
                return;

            UnloadTagset();

            if (SelectedTagset == TagsetIndex.NOT_SELECTED_TAGSET_NAME)
                return;

            try
            {
                File.WriteAllText(ProjectInfo.TagsetPath, SelectedTagset);
                ProjectInfo.TryUpdateTagset(SelectedTagset);

                var tagsetCopy = CurrentTagset.Select(t => t.Clone() as Tag).ToArray();
                _mainWindowVM.ChatVM.TagsVM.SetTagsetCommand.Execute(tagsetCopy);
            }
            catch (Exception ex)
            {
                new QuickMessage(ex.Message).ShowError();
            }
        }

        #endregion

        #region ConsoleCommands

        public ICommand ClearConsoleCommand { get; }
        public bool CanClearConsoleCommandExecute(object parameter)
        {
            return true;
        }
        public void OnClearConsoleCommandExecuted(object parameter)
        {
            if (!CanClearConsoleCommandExecute(parameter))
                return;

            ConsoleText = string.Empty;
        }

        public ICommand SetTagNameToConsoleCommand { get; }
        public bool CanSetTagNameToConsoleCommandExecute(object parameter)
        {
            return SelectedTag != null;
        }
        public void OnSetTagNameToConsoleCommandExecuted(object parameter)
        {
            if (!CanSetTagNameToConsoleCommandExecute(parameter))
                return;

            ConsoleText = SelectedTag.Header;
        }

        public ICommand ChooseTagCreationColorCommand { get; }
        public bool CanChooseTagCreationColorCommandExecute(object parameter)
        {
            return true;
        }
        public void OnChooseTagCreationColorCommandExecuted(object parameter)
        {
            if (!CanChooseTagCreationColorCommandExecute(parameter))
                return;

            if (DialogProvider.GetColor(out Color color, TagCreationColor))
                TagCreationColor = color;
        }

        public ICommand SetRandomTagCreationColorCommand { get; }
        public bool CanSetRandomTagCreationColorCommandExecute(object parameter)
        {
            return true;
        }
        public void OnSetRandomTagCreationColorCommandExecuted(object parameter)
        {
            if (!CanSetRandomTagCreationColorCommandExecute(parameter))
                return;

            SetRandomTagCreationColor();
        }

        #endregion

        #region SystemCommands

        public ICommand CheckProjectTagsetCommand { get; }
        public bool CanCheckProjectTagsetCommandExecute(object parameter)
        {
            return parameter is CancelEventArgs;
        }
        public void OnCheckProjectTagsetCommandExecuted(object parameter)
        {
            if (!CanCheckProjectTagsetCommandExecute(parameter))
                return;

            var args = parameter as CancelEventArgs;

            if (_mainWindowVM.TagsetName == "No tagset" || Tagsets.Contains(_mainWindowVM.TagsetName))
                return;

            var msgResult = new QuickMessage("The tagset list does not contains project tagset. If you do not add the project tagset to this " +
                "list, all tagged messages will be deleted. Add a project tagset to the list?").ShowWarning(MessageBoxButton.YesNoCancel);

            if (msgResult == MessageBoxResult.Cancel)
            {
                args.Cancel = true;
                return;
            }

            if (msgResult == MessageBoxResult.Yes)
            {
                string projectTagsetName = _mainWindowVM.TagsetName;
                var projectTagset = _mainWindowVM.ChatVM.TagsVM.CurrentTagset.ToDictionary(t => t.Header, t => t.BackgroundColor);

                Tagsets.Add(projectTagsetName);
                TagsetIndex.GetInstance().AddIndexEntry(projectTagsetName, projectTagset);
            }
        }

        public ICommand DeactivateWindowCommand { get; }
        public bool CanDeactivateWindowCommandExecute(object parameter)
        {
            return true;
        }
        public void OnDeactivateWindowCommandExecuted(object parameter)
        {
            if (!CanDeactivateWindowCommandExecute(parameter))
                return;

            try
            {
                DeactivateAction?.Invoke();
                TagsetIndex.GetInstance().FlushIndexToDisk();

                if (!Tagsets.Contains(_mainWindowVM.TagsetName))
                {
                    UnloadTagset();
                    return;
                }
            }
            catch (Exception ex)
            {
                new QuickMessage(ex.Message).ShowError();
            }
        }

        #endregion

        public TagsetEditorWindowViewModel(MainWindowViewModel mainWindowVM)
        {
            _mainWindowVM = mainWindowVM ?? throw new ArgumentNullException(nameof(mainWindowVM));

            var tagsets = TagsetIndex.GetInstance().IndexCollection.Keys.ToArray();
            Tagsets = new ObservableCollection<string>(tagsets);

            CurrentTagset = new ObservableCollection<Tag>();

            SelectedTagset = ProjectInfo.TagsetSet
                ? ProjectInfo.Tagset
                : TagsetIndex.NOT_SELECTED_TAGSET_NAME;

            SetRandomTagCreationColor();

            AddTagCommand = new RelayCommand(OnAddTagCommandExecuted, CanAddTagCommandExecute);
            DeleteTagCommand = new RelayCommand(OnDeleteTagCommandExecuted, CanDeleteTagCommandExecute);
            RenameTagCommand = new RelayCommand(OnRenameTagCommandExecuted, CanRenameTagCommandExecute);
            ChangeTagColorCommand = new RelayCommand(OnChangeTagColorCommandExecuted, CanChangeTagColorCommandExecute);

            ClearConsoleCommand = new RelayCommand(OnClearConsoleCommandExecuted, CanClearConsoleCommandExecute);
            SetTagNameToConsoleCommand = new RelayCommand(OnSetTagNameToConsoleCommandExecuted, CanSetTagNameToConsoleCommandExecute);
            ChooseTagCreationColorCommand = new RelayCommand(OnChooseTagCreationColorCommandExecuted, CanChooseTagCreationColorCommandExecute);
            SetRandomTagCreationColorCommand = new RelayCommand(OnSetRandomTagCreationColorCommandExecuted, CanSetRandomTagCreationColorCommandExecute);

            AddTagsetCommand = new RelayCommand(OnAddTagsetCommandExecuted, CanAddTagsetCommandExecute);
            DeleteTagsetCommand = new RelayCommand(OnDeleteTagsetCommandExecuted, CanDeleteTagsetCommandExecute);
            RenameTagsetCommand = new RelayCommand(OnRenameTagsetCommandExecuted, CanRenameTagsetCommandExecute);
            SetTagsetCommand = new RelayCommand(OnSetTagsetCommandExecuted, CanSetTagsetCommandExecute);

            CheckProjectTagsetCommand = new RelayCommand(OnCheckProjectTagsetCommandExecuted, CanCheckProjectTagsetCommandExecute);
            DeactivateWindowCommand = new RelayCommand(OnDeactivateWindowCommandExecuted, CanDeactivateWindowCommandExecute);
        }

        private void SetRandomTagCreationColor()
        {
            TagCreationColor = ColorGenerator.GenerateHSLuvColor();
        }

        private void UnloadTagset()
        {
            try
            {
                if (ProjectInfo.TagsetSet)
                    File.Delete(ProjectInfo.TagsetPath);

                ProjectInfo.TryUpdateTagset();

                _mainWindowVM.ChatVM.TagsVM.ClearData();
                _mainWindowVM.ChatVM.RemoveAllTagsCommand.Execute(null);
            }
            catch (Exception ex)
            {
                new QuickMessage(ex.Message).ShowError();
            }
        }
    }
}
