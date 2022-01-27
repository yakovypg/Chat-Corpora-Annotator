using ChatCorporaAnnotator.Infrastructure.Commands;
using ChatCorporaAnnotator.Infrastructure.Extensions;
using ChatCorporaAnnotator.Models.Chat;
using ChatCorporaAnnotator.Models.Messages;
using ChatCorporaAnnotator.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ChatCorporaAnnotator.ViewModels.Windows
{
    internal class QueryLanguageWindowViewModel : ViewModel
    {
        private readonly MainWindowViewModel _mainWindowVM;

        public Action DeactivateAction { get; set; }

        public ObservableCollection<object> UserDictionary { get; }
        public ObservableCollection<Button> QueryItems { get; }

        private string _queryText = string.Empty;
        public string QueryText
        {
            get => _queryText;
            set => SetValue(ref _queryText, value);
        }

        #region InfoItems

        private int _foundSuggestionsCount;
        public int FoundSuggestionsCount
        {
            get => _foundSuggestionsCount;
            private set => SetValue(ref _foundSuggestionsCount, value);
        }

        private int _foundGroupsCount;
        public int FoundGroupsCount
        {
            get => _foundGroupsCount;
            private set => SetValue(ref _foundGroupsCount, value);
        }

        private int _currentSuggestionIndex;
        public int CurrentSuggestionIndex
        {
            get => _currentSuggestionIndex;
            private set => SetValue(ref _currentSuggestionIndex, value);
        }

        private int _suggestionsCount;
        public int SuggestionsCount
        {
            get => _suggestionsCount;
            private set => SetValue(ref _suggestionsCount, value);
        }

        #endregion

        #region VisibilityItems

        private Visibility _queryTextBoxVisibility = Visibility.Visible;
        public Visibility QueryTextBoxVisibility
        {
            get => _queryTextBoxVisibility;
            set => SetValue(ref _queryTextBoxVisibility, value);
        }

        #endregion

        #region QueryCreatorCommands

        public ICommand SwitchModeCommand { get; }
        public bool CanSwitchModeCommandExecute(object parameter)
        {
            return true;
        }
        public void OnSwitchModeCommandExecuted(object parameter)
        {
            if (!CanSwitchModeCommandExecute(parameter))
                return;

            QueryTextBoxVisibility = QueryTextBoxVisibility == Visibility.Hidden
                ? Visibility.Visible
                : Visibility.Hidden;
        }

        public ICommand ImportQueriesCommand { get; }
        public bool CanImportQueriesCommandExecute(object parameter)
        {
            return true;
        }
        public void OnImportQueriesCommandExecuted(object parameter)
        {
            if (!CanImportQueriesCommandExecute(parameter))
                return;
        }

        public ICommand ClearQueryCommand { get; }
        public bool CanClearQueryCommandExecute(object parameter)
        {
            return true;
        }
        public void OnClearQueryCommandExecuted(object parameter)
        {
            if (!CanClearQueryCommandExecute(parameter))
                return;

            QueryItems.Clear();
            QueryText = string.Empty;
        }

        public ICommand RunQueryCommand { get; }
        public bool CanRunQueryCommandExecute(object parameter)
        {
            return true;
        }
        public void OnRunQueryCommandExecuted(object parameter)
        {
            if (!CanRunQueryCommandExecute(parameter))
                return;
        }

        #endregion

        #region UserDictionaryCommands

        public ICommand ShowDictionaryEditorCommand { get; }
        public bool CanShowDictionaryEditorCommandExecute(object parameter)
        {
            return true;
        }
        public void OnShowDictionaryEditorCommandExecuted(object parameter)
        {
            if (!CanShowDictionaryEditorCommandExecute(parameter))
                return;
        }

        #endregion

        #region SuggestionsCommands

        public ICommand ShowPreviousSuggestionCommand { get; }
        public bool CanShowPreviousSuggestionCommandExecute(object parameter)
        {
            return CurrentSuggestionIndex < SuggestionsCount;
        }
        public void OnShowPreviousSuggestionCommandExecuted(object parameter)
        {
            if (!CanShowPreviousSuggestionCommandExecute(parameter))
                return;
        }

        public ICommand ShowNextSuggestionCommand { get; }
        public bool CanShowNextSuggestionCommandExecute(object parameter)
        {
            return CurrentSuggestionIndex > 1;
        }
        public void OnShowNextSuggestionCommandExecuted(object parameter)
        {
            if (!CanShowNextSuggestionCommandExecute(parameter))
                return;
        }

        #endregion

        #region DragDropCommands

        public ICommand DragButtonCommand { get; }
        public bool CanDragButtonCommandExecute(object parameter)
        {
            return true;
        }
        public void OnDragButtonCommandExecuted(object parameter)
        {
            if (!CanDragButtonCommandExecute(parameter))
                return;

            Button senderBtn = parameter as Button;
            var data = new DataObject(typeof(Button), senderBtn);

            DragDrop.DoDragDrop(senderBtn, data, DragDropEffects.Copy);
        }

        public ICommand WrapPanelTakeDropCommand { get; }
        public bool CanWrapPanelTakeDropCommandExecute(object parameter)
        {
            return true;
        }
        public void OnWrapPanelTakeDropCommandExecuted(object parameter)
        {
            if (!CanWrapPanelTakeDropCommandExecute(parameter))
                return;

            var e = parameter as DragEventArgs;
            var data = e.Data as DataObject;

            Button sourceBtn = data.GetData(typeof(Button)) as Button;

            if (QueryItems.Contains(sourceBtn))
                return;

            Button btn = new Button
            {
                Content = sourceBtn.Content,
                Height = sourceBtn.ActualHeight,
                Foreground = sourceBtn.Foreground,
                Background = sourceBtn.Background,
                BorderBrush = sourceBtn.BorderBrush,
                BorderThickness = sourceBtn.BorderThickness,
                FontSize = sourceBtn.FontSize,

                AllowDrop = true,
                Cursor = Cursors.SizeAll,
                Margin = new Thickness(3, 3, 3, 3),
                Padding = new Thickness(4, 0, 4, 0)
            };

            btn.DragEnter += QueryItemButton_DragEnter;
            btn.PreviewMouseLeftButtonDown += DragDropButton_MouseDown;

            QueryItems.Add(btn);
        }

        #endregion

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

            try
            {
                DeactivateAction?.Invoke();
            }
            catch (Exception ex)
            {
                new QuickMessage($"Error: {ex.Message}").ShowError();
            }
        }

       #endregion

        public QueryLanguageWindowViewModel(MainWindowViewModel mainWindowVM)
        {
            _mainWindowVM = mainWindowVM ?? throw new ArgumentNullException(nameof(mainWindowVM));

            UserDictionary = new ObservableCollection<object>();
            QueryItems = new ObservableCollection<Button>();

            SwitchModeCommand = new RelayCommand(OnSwitchModeCommandExecuted, CanSwitchModeCommandExecute);
            ImportQueriesCommand = new RelayCommand(OnImportQueriesCommandExecuted, CanImportQueriesCommandExecute);
            ClearQueryCommand = new RelayCommand(OnClearQueryCommandExecuted, CanRunQueryCommandExecute);
            RunQueryCommand = new RelayCommand(OnRunQueryCommandExecuted, CanRunQueryCommandExecute);

            ShowDictionaryEditorCommand = new RelayCommand(OnShowDictionaryEditorCommandExecuted, CanShowDictionaryEditorCommandExecute);

            ShowPreviousSuggestionCommand = new RelayCommand(OnShowPreviousSuggestionCommandExecuted, CanShowPreviousSuggestionCommandExecute);
            ShowNextSuggestionCommand = new RelayCommand(OnShowNextSuggestionCommandExecuted, CanShowNextSuggestionCommandExecute);

            DragButtonCommand = new RelayCommand(OnDragButtonCommandExecuted, CanDragButtonCommandExecute);
            WrapPanelTakeDropCommand = new RelayCommand(OnWrapPanelTakeDropCommandExecuted, CanWrapPanelTakeDropCommandExecute);

            DeactivateWindowCommand = new RelayCommand(OnDeactivateWindowCommandExecuted, CanDeactivateWindowCommandExecute);
        }

        #region DragDropEventMethods

        private void QueryItemButton_DragEnter(object sender, DragEventArgs e)
        {
            var data = e.Data as DataObject;
            Button source = data.GetData(typeof(Button)) as Button;

            if (!QueryItems.Contains(source))
                return;

            QueryItems.MoveItem(source, sender as Button);
        }

        private void DragDropButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            OnDragButtonCommandExecuted(sender as Button);
        }

        #endregion
    }
}
