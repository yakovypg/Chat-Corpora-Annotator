using ChatCorporaAnnotator.Infrastructure.Commands;
using ChatCorporaAnnotator.Infrastructure.Extensions;
using ChatCorporaAnnotator.ViewModels.Base;
using IndexEngine;
using IndexEngine.Paths;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace ChatCorporaAnnotator.ViewModels.Chat
{
    internal class ChatViewModel : ViewModel
    {
        private readonly MainWindowViewModel _mainWindowVM;

        public UsersViewModel UsersVM { get; }
        public DatesViewModel DatesVM { get; }
        public MessagesViewModel MessagesVM { get; }
        public TagsViewModel TagsVM { get; }
        public SituationsViewModel SituationsVM { get; }

        public ObservableCollection<DataGridColumn> ChatColumns { get; private set; }

        #region ChatViewCommands

        public ICommand SetChatColumnsCommand { get; }
        public bool CanSetChatColumnsCommandExecute(object parameter)
        {
            return true;
        }
        public void OnSetChatColumnsCommandExecuted(object parameter)
        {
            if (!CanSetChatColumnsCommandExecute(parameter))
                return;

            List<string> selectedFields = parameter is List<string> fields
                ? fields
                : ProjectInfo.Data.SelectedFields;

            if (selectedFields.IsNullOrEmpty())
                return;

            if (selectedFields.Remove(ProjectInfo.TextFieldKey))
                selectedFields.Insert(0, ProjectInfo.TextFieldKey);

            if (selectedFields.Remove(ProjectInfo.DateFieldKey))
                selectedFields.Insert(0, ProjectInfo.DateFieldKey);

            if (selectedFields.Remove(ProjectInfo.SenderFieldKey))
                selectedFields.Insert(0, ProjectInfo.SenderFieldKey);

            selectedFields.Insert(0, "Tag");

            Thickness tbThickness = new Thickness(5, 3, 5, 3);

            foreach (var field in ProjectInfo.Data.SelectedFields)
            {
                DataTemplate columnDataTemplate = new DataTemplate(typeof(DynamicMessage));

                var textBlockFactory = new FrameworkElementFactory(typeof(TextBlock));
                textBlockFactory.SetBinding(TextBlock.TextProperty, new Binding($"Source.Contents[{field}]"));
                textBlockFactory.SetValue(TextBlock.TextWrappingProperty, TextWrapping.Wrap);
                textBlockFactory.SetValue(TextBlock.PaddingProperty, tbThickness);

                if (field == ProjectInfo.SenderFieldKey)
                    textBlockFactory.SetBinding(TextBlock.ForegroundProperty, new Binding($"SenderColor"));

                columnDataTemplate.VisualTree = textBlockFactory;

                var column = new DataGridTemplateColumn()
                {
                    Header = field,
                    CellTemplate = columnDataTemplate
                };

                ChatColumns.Add(column);
            }
        }

        #endregion

        #region TagCommands

        public ICommand AddTagCommand { get; }
        public bool CanAddTagCommandExecute(object parameter)
        {
            return MessagesVM.SelectedMessages.Count > 0;
        }
        public void OnAddTagCommandExecuted(object parameter)
        {
            if (!CanAddTagCommandExecute(parameter))
                return;
        }

        public ICommand RemoveTagCommand { get; }
        public bool CanRemoveTagCommandExecute(object parameter)
        {
            return MessagesVM.SelectedMessages.Count > 0;
        }
        public void OnRemoveTagCommandExecuted(object parameter)
        {
            if (!CanRemoveTagCommandExecute(parameter))
                return;
        }

        #endregion

        public ChatViewModel(MainWindowViewModel mainWindowVM)
        {
            _mainWindowVM = mainWindowVM ?? throw new ArgumentNullException(nameof(mainWindowVM));

            UsersVM = new UsersViewModel();
            DatesVM = new DatesViewModel();
            MessagesVM = new MessagesViewModel();
            TagsVM = new TagsViewModel(_mainWindowVM);
            SituationsVM = new SituationsViewModel(_mainWindowVM);

            ChatColumns = new ObservableCollection<DataGridColumn>();

            SetChatColumnsCommand = new RelayCommand(OnSetChatColumnsCommandExecuted, CanSetChatColumnsCommandExecute);

            AddTagCommand = new RelayCommand(OnAddTagCommandExecuted, CanAddTagCommandExecute);
            RemoveTagCommand = new RelayCommand(OnRemoveTagCommandExecuted, CanRemoveTagCommandExecute);
        }
    }
}
