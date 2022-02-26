using ChatCorporaAnnotator.Data.Dialogs;
using ChatCorporaAnnotator.Data.Indexing;
using ChatCorporaAnnotator.Data.Parsers.Suggester;
using ChatCorporaAnnotator.Infrastructure.Commands;
using ChatCorporaAnnotator.Infrastructure.Enums;
using ChatCorporaAnnotator.Infrastructure.Extensions;
using ChatCorporaAnnotator.Infrastructure.Extensions.Controls;
using ChatCorporaAnnotator.Models.Chat;
using ChatCorporaAnnotator.Models.Messages;
using ChatCorporaAnnotator.Models.Suggester;
using ChatCorporaAnnotator.ViewModels.Base;
using IndexEngine.Containers;
using IndexEngine.Data.Paths;
using IndexEngine.Indexes;
using IndexEngine.Search;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace ChatCorporaAnnotator.ViewModels.Windows
{
    internal class SuggesterWindowViewModel : ViewModel
    {
        private const int DRAG_DROP_SWITCH_PAUSE = 1000;

        private delegate void LastQueryItemChangedHandler();
        private event LastQueryItemChangedHandler LastQueryItemChanged;

        private readonly ChatColumnCreator _chatColumnCreator;

        private bool _isWindowClosing = false;

        public MainWindowViewModel MainWindowVM { get; }
        public Action DeactivateAction { get; set; }

        public ObservableCollection<DataGridColumn> MessageContainerColumns { get; }

        #region UserDictionaryItems

        public ObservableCollection<IUserDictionaryItem> UserDictionary { get; private set; }
        public ObservableCollection<string> CurrentUserDictItemWords { get; private set; }

        private string _dictEditorConsoleText;
        public string DictEditorConsoleText
        {
            get => _dictEditorConsoleText;
            set => SetValue(ref _dictEditorConsoleText, value);
        }

        private string _selectedUserDictItemWord;
        public string SelectedUserDictItemWord
        {
            get => _selectedUserDictItemWord;
            set => SetValue(ref _selectedUserDictItemWord, value);
        }

        private IUserDictionaryItem _selectedUserDictItem;
        public IUserDictionaryItem SelectedUserDictItem
        {
            get => _selectedUserDictItem;
            set
            {
                if (!SetValue(ref _selectedUserDictItem, value))
                    return;

                if (value == null || value.Words.Count == 0)
                {
                    CurrentUserDictItemWords?.Clear();
                    return;
                }

                CurrentUserDictItemWords = new ObservableCollection<string>(value.Words);
                OnPropertyChanged(nameof(CurrentUserDictItemWords));
            }
        }

        #endregion

        #region QueryItems

        private Tuple<Button, DateTime> _lastDragDropSwitch = new Tuple<Button, DateTime>(null, DateTime.MinValue);

        private List<List<List<int>>> _queryResult = new List<List<List<int>>>();
        private List<int> _hits = new List<int>();

        public ObservableCollection<ChatMessage> CurrentGroupMessages { get; private set; }
        public ObservableCollection<Button> QueryItems { get; }

        private OperationState _queryExecutionState = OperationState.NotStarted;
        public OperationState QueryExecutionState
        {
            get => _queryExecutionState;
            set => SetValue(ref _queryExecutionState, value);
        }

        private string _queryText = string.Empty;
        public string QueryText
        {
            get => _queryText;
            set => SetValue(ref _queryText, value);
        }

        private bool _isQueryExecutionWaitingIconSpinActive;
        public bool IsQueryExecutionWaitingIconSpinActive
        {
            get => _isQueryExecutionWaitingIconSpinActive;
            set => SetValue(ref _isQueryExecutionWaitingIconSpinActive, value);
        }

        private bool _isQueryItemPopupOpen;
        public bool IsQueryItemPopupOpen
        {
            get => _isQueryItemPopupOpen;
            set => SetValue(ref _isQueryItemPopupOpen, value);
        }

        private UIElement _queryItemPopupChild;
        public UIElement QueryItemPopupChild
        {
            get => _queryItemPopupChild;
            private set
            {
                var grid = new UniformGrid()
                {
                    Background = Brushes.GhostWhite,
                    Margin = new Thickness(5, 5, 5, 5),
                };

                grid.Children.Add(value);

                var border = new Border()
                {
                    BorderBrush = Brushes.DarkGray,
                    Background = Brushes.GhostWhite,
                    BorderThickness = new Thickness(1, 1, 1, 1),
                    Child = grid
                };

                SetValue(ref _queryItemPopupChild, border);
            }
        }

        #endregion

        #region ImportedQueryItems

        public ObservableCollection<IImportedQuery> ImportedQueries { get; private set; }
        public IImportedQuery SelectedImportedQuery { get; set; }

        #endregion

        #region StaticItems

        private static readonly Color DefaultButtonBackgroundColor = Colors.Transparent;
        private static readonly Color HighlightedButtonBackgroundColor = Colors.BlanchedAlmond;

        /*
         * n - num
         * && - and
         * st - select
         * ! - not
         * || - or
         * i - inwin
         * org - hasorganization
         * dt - hasdate
         * tm - hastime
         * us - hasusermentioned
         * lc - haslocation
         * bs - byuser
         * dc - haswordofdict
         */
        private static readonly Dictionary<string, int[]> HighlightRules = new Dictionary<string, int[]>()
        {
            //                               ;  ,  )  (  n  && st ! ||  i org dt tm us lc bs dc
            {";",                 new int[] {0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0} },
            {",",                 new int[] {0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1} },
            {")",                 new int[] {1, 1, 0, 0, 0, 1, 0, 0, 1, 1, 0, 0, 0, 0, 0, 0, 0} },
            {"(",                 new int[] {0, 0, 0, 1, 0, 0, 1, 1, 0, 0, 1, 1, 1, 1, 1, 1, 1} },
            {"num",               new int[] {0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0} },
            {"and",               new int[] {0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1} },
            {"select",            new int[] {0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1} },
            {"not",               new int[] {0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1} },
            {"or",                new int[] {0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1} },
            {"inwin",             new int[] {0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0} },
            {"hasorganization()", new int[] {0, 1, 1, 0, 0, 1, 0, 0, 1, 1, 0, 0, 0, 0, 0, 0, 0} },
            {"hasdate()",         new int[] {0, 1, 1, 0, 0, 1, 0, 0, 1, 1, 0, 0, 0, 0, 0, 0, 0} },
            {"hastime()",         new int[] {0, 1, 1, 0, 0, 1, 0, 0, 1, 1, 0, 0, 0, 0, 0, 0, 0} },
            {"hasusermentioned()",new int[] {0, 1, 1, 0, 0, 1, 0, 0, 1, 1, 0, 0, 0, 0, 0, 0, 0} },
            {"haslocation()",     new int[] {0, 1, 1, 0, 0, 1, 0, 0, 1, 1, 0, 0, 0, 0, 0, 0, 0} },
            {"byuser()",          new int[] {0, 1, 1, 0, 0, 1, 0, 0, 1, 1, 0, 0, 0, 0, 0, 0, 0} },
            {"haswordofdict()",   new int[] {0, 1, 1, 0, 0, 1, 0, 0, 1, 1, 0, 0, 0, 0, 0, 0, 0} },
        };

        #endregion

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

        #region ItemsVisibility

        private Visibility _queryTextBoxVisibility = Visibility.Visible;
        public Visibility QueryTextBoxVisibility
        {
            get => _queryTextBoxVisibility;
            set => SetValue(ref _queryTextBoxVisibility, value);
        }

        private Visibility _dictionaryEditorPanelVisibility = Visibility.Hidden;
        public Visibility DictionaryEditorPanelVisibility
        {
            get => _dictionaryEditorPanelVisibility;
            set => SetValue(ref _dictionaryEditorPanelVisibility, value);
        }

        private Visibility _importQueriesPanelVisibility = Visibility.Hidden;
        public Visibility ImportQueriesPanelVisibility
        {
            get => _importQueriesPanelVisibility;
            set => SetValue(ref _importQueriesPanelVisibility, value);
        }

        private Visibility _queryExecutionWaitingPanelVisibility = Visibility.Hidden;
        public Visibility QueryExecutionWaitingPanelVisibility
        {
            get => _queryExecutionWaitingPanelVisibility;
            set => SetValue(ref _queryExecutionWaitingPanelVisibility, value);
        }

        #endregion

        #region ButtonBackgrounds

        private readonly SolidColorBrush[] _orderedButtonBackgrounds;

        private SolidColorBrush _selectBtnBackground = new SolidColorBrush(DefaultButtonBackgroundColor);
        public SolidColorBrush SelectBtnBackground
        {
            get => _selectBtnBackground;
            private set => SetValue(ref _selectBtnBackground, value);
        }

        private SolidColorBrush _andBtnBackground = new SolidColorBrush(DefaultButtonBackgroundColor);
        public SolidColorBrush AndBtnBackground
        {
            get => _andBtnBackground;
            private set => SetValue(ref _andBtnBackground, value);
        }

        private SolidColorBrush _orBtnBackground = new SolidColorBrush(DefaultButtonBackgroundColor);
        public SolidColorBrush OrBtnBackground
        {
            get => _orBtnBackground;
            private set => SetValue(ref _orBtnBackground, value);
        }

        private SolidColorBrush _notBtnBackground = new SolidColorBrush(DefaultButtonBackgroundColor);
        public SolidColorBrush NotBtnBackground
        {
            get => _notBtnBackground;
            private set => SetValue(ref _notBtnBackground, value);
        }

        private SolidColorBrush _numBtnBackground = new SolidColorBrush(DefaultButtonBackgroundColor);
        public SolidColorBrush NumBtnBackground
        {
            get => _numBtnBackground;
            private set => SetValue(ref _numBtnBackground, value);
        }

        private SolidColorBrush _commaBtnBackground = new SolidColorBrush(DefaultButtonBackgroundColor);
        public SolidColorBrush CommaBtnBackground
        {
            get => _commaBtnBackground;
            private set => SetValue(ref _commaBtnBackground, value);
        }

        private SolidColorBrush _semicolonBtnBackground = new SolidColorBrush(DefaultButtonBackgroundColor);
        public SolidColorBrush SemicolonBtnBackground
        {
            get => _semicolonBtnBackground;
            private set => SetValue(ref _semicolonBtnBackground, value);
        }

        private SolidColorBrush _leftBracketBtnBackground = new SolidColorBrush(DefaultButtonBackgroundColor);
        public SolidColorBrush LeftBracketBtnBackground
        {
            get => _leftBracketBtnBackground;
            private set => SetValue(ref _leftBracketBtnBackground, value);
        }

        private SolidColorBrush _rightBracketBtnBackground = new SolidColorBrush(DefaultButtonBackgroundColor);
        public SolidColorBrush RightBracketBtnBackground
        {
            get => _rightBracketBtnBackground;
            private set => SetValue(ref _rightBracketBtnBackground, value);
        }

        private SolidColorBrush _hasWordOfDictBtnBackground = new SolidColorBrush(DefaultButtonBackgroundColor);
        public SolidColorBrush HasWordOfDictBtnBackground
        {
            get => _hasWordOfDictBtnBackground;
            private set => SetValue(ref _hasWordOfDictBtnBackground, value);
        }

        private SolidColorBrush _hasUserMentionedBtnBackground = new SolidColorBrush(DefaultButtonBackgroundColor);
        public SolidColorBrush HasUserMentionedBtnBackground
        {
            get => _hasUserMentionedBtnBackground;
            private set => SetValue(ref _hasUserMentionedBtnBackground, value);
        }

        private SolidColorBrush _byUserBtnBackground = new SolidColorBrush(DefaultButtonBackgroundColor);
        public SolidColorBrush ByUserBtnBackground
        {
            get => _byUserBtnBackground;
            private set => SetValue(ref _byUserBtnBackground, value);
        }

        private SolidColorBrush _inwinBtnBackground = new SolidColorBrush(DefaultButtonBackgroundColor);
        public SolidColorBrush InwinBtnBackground
        {
            get => _inwinBtnBackground;
            private set => SetValue(ref _inwinBtnBackground, value);
        }

        private SolidColorBrush _hasOrganizationBtnBackground = new SolidColorBrush(DefaultButtonBackgroundColor);
        public SolidColorBrush HasOrganizationBtnBackground
        {
            get => _hasOrganizationBtnBackground;
            private set => SetValue(ref _hasOrganizationBtnBackground, value);
        }

        private SolidColorBrush _hasTimeBtnBackground = new SolidColorBrush(DefaultButtonBackgroundColor);
        public SolidColorBrush HasTimeBtnBackground
        {
            get => _hasTimeBtnBackground;
            private set => SetValue(ref _hasTimeBtnBackground, value);
        }

        private SolidColorBrush _hasLocationBtnBackground = new SolidColorBrush(DefaultButtonBackgroundColor);
        public SolidColorBrush HasLocationBtnBackground
        {
            get => _hasLocationBtnBackground;
            private set => SetValue(ref _hasLocationBtnBackground, value);
        }

        private SolidColorBrush _hasDateBtnBackground = new SolidColorBrush(DefaultButtonBackgroundColor);
        public SolidColorBrush HasDateBtnBackground
        {
            get => _hasDateBtnBackground;
            private set => SetValue(ref _hasDateBtnBackground, value);
        }

        #endregion

        #region QueryCreatorCommands

        public ICommand SwitchModeCommand { get; }
        public bool CanSwitchModeCommandExecute(object parameter)
        {
            return QueryExecutionState != OperationState.InProcess;
        }
        public void OnSwitchModeCommandExecuted(object parameter)
        {
            if (!CanSwitchModeCommandExecute(parameter))
                return;

            QueryTextBoxVisibility = QueryTextBoxVisibility == Visibility.Hidden
                ? Visibility.Visible
                : Visibility.Hidden;
        }

        public ICommand AddElementToQueryTextCommand { get; }
        public bool CanAddElementToQueryTextCommandExecute(object parameter)
        {
            return QueryExecutionState != OperationState.InProcess;
        }
        public void OnAddElementToQueryTextCommandExecuted(object parameter)
        {
            if (!CanAddElementToQueryTextCommandExecute(parameter))
                return;

            if (!(parameter is Button btn))
                return;

            string content = btn.Content.ToString();

            if (content == "," || content == ";" ||
                content == "(" || content == ")" ||
                content == "select")
            {
                QueryText += content;
            }
            else
            {
                QueryText += " " + content;
            }
        }

        public ICommand ClearQueryCommand { get; }
        public bool CanClearQueryCommandExecute(object parameter)
        {
            return QueryExecutionState != OperationState.InProcess;
        }
        public void OnClearQueryCommandExecuted(object parameter)
        {
            if (!CanClearQueryCommandExecute(parameter))
                return;

            if (QueryTextBoxVisibility == Visibility.Visible)
            {
                QueryText = string.Empty;
            }
            else
            {
                QueryItems.Clear();
                LastQueryItemChanged?.Invoke();
            }
        }

        public ICommand RunQueryCommand { get; }
        public bool CanRunQueryCommandExecute(object parameter)
        {
            return QueryExecutionState != OperationState.InProcess;
        }
        public void OnRunQueryCommandExecuted(object parameter)
        {
            if (!CanRunQueryCommandExecute(parameter))
                return;

            QueryExecutionState = OperationState.InProcess;

            string query = QueryTextBoxVisibility == Visibility.Hidden
                ? CreateQueryFromItems()
                : QueryText;

            SwitchQueryExecutionWaitingPanelVisibility();

            var queryParsingTask = Task.Run(delegate
            {
                _queryResult = QueryParser.Parse(query);
            });

            var resultDisplayingTask = queryParsingTask.ContinueWith(DisplayQueryResult,
                TaskContinuationOptions.ExecuteSynchronously);

            resultDisplayingTask.ContinueWith(t => SwitchQueryExecutionWaitingPanelVisibility(),
                TaskContinuationOptions.ExecuteSynchronously);
        }

        #endregion

        #region ImportQueriesCommands

        public ICommand ImportQueriesCommand { get; }
        public bool CanImportQueriesCommandExecute(object parameter)
        {
            return true;
        }
        public void OnImportQueriesCommandExecuted(object parameter)
        {
            if (!CanImportQueriesCommandExecute(parameter))
                return;

            DialogProvider.GetQueriesFilePath(out string path);

            if (string.IsNullOrEmpty(path))
                return;

            string[] lines;

            try
            {
                lines = File.ReadAllLines(path);
            }
            catch (Exception ex)
            {
                new QuickMessage(ex.Message).ShowError();
                return;
            }

            if (lines.IsNullOrEmpty())
            {
                new QuickMessage("File is empty.").ShowInformation();
                return;
            }

            var queries = new List<IImportedQuery>();

            foreach (var line in lines)
            {
                if (string.IsNullOrEmpty(line))
                    continue;

                var query = ImportedQuery.Parse(line, ':');
                queries.Add(query);
            }

            ImportedQueries = new ObservableCollection<IImportedQuery>(queries);
            OnPropertyChanged(nameof(ImportedQueries));

            ImportQueriesPanelVisibility = Visibility.Visible;
        }

        public ICommand LoadImportedQueryCommand { get; }
        public bool CanLoadImportedQueryCommandExecute(object parameter)
        {
            return SelectedImportedQuery != null;
        }
        public void OnLoadImportedQueryCommandExecuted(object parameter)
        {
            if (!CanLoadImportedQueryCommandExecute(parameter))
                return;

            QueryText = SelectedImportedQuery.Content;
        }

        public ICommand RemoveImportedQueryCommand { get; }
        public bool CanRemoveImportedQueryCommandExecute(object parameter)
        {
            return SelectedImportedQuery != null;
        }
        public void OnRemoveImportedQueryCommandExecuted(object parameter)
        {
            if (!CanRemoveImportedQueryCommandExecute(parameter))
                return;

            int index = ImportedQueries.IndexOf(SelectedImportedQuery);
            ImportedQueries.Remove(SelectedImportedQuery);

            SelectedImportedQuery = ImportedQueries.Count == 0 || index == 0
                ? ImportedQueries.FirstOrDefault()
                : ImportedQueries[index - 1];
        }

        public ICommand RemoveAllImportedQueriesCommand { get; }
        public bool CanRemoveAllImportedQueriesCommandExecute(object parameter)
        {
            return true;
        }
        public void OnRemoveAllImportedQueriesCommandExecuted(object parameter)
        {
            if (!CanRemoveAllImportedQueriesCommandExecute(parameter))
                return;

            ImportedQueries.Clear();
        }

        public ICommand SwitchImportQueriesPanelVisibilityCommand { get; }
        public bool CanSwitchImportQueriesPanelVisibilityCommandExecute(object parameter)
        {
            return true;
        }
        public void OnSwitchImportQueriesPanelVisibilityCommandExecuted(object parameter)
        {
            if (!CanSwitchImportQueriesPanelVisibilityCommandExecute(parameter))
                return;

            ImportQueriesPanelVisibility = ImportQueriesPanelVisibility == Visibility.Hidden
                ? Visibility.Visible
                : Visibility.Hidden;
        }

        #endregion

        #region UserDictionaryCommands

        public ICommand SwitchDictionaryEditorVisibilityCommand { get; }
        public bool CanSwitchDictionaryEditorVisibilityCommandExecute(object parameter)
        {
            return QueryExecutionState != OperationState.InProcess;
        }
        public void OnSwitchDictionaryEditorVisibilityCommandExecuted(object parameter)
        {
            if (!CanSwitchDictionaryEditorVisibilityCommandExecute(parameter))
                return;

            DictionaryEditorPanelVisibility = DictionaryEditorPanelVisibility == Visibility.Hidden
                ? Visibility.Visible
                : Visibility.Hidden;
        }

        public ICommand ExportUserDictionaryCommand { get; }
        public bool CanExportUserDictionaryCommandExecute(object parameter)
        {
            return QueryExecutionState != OperationState.InProcess;
        }
        public void OnExportUserDictionaryCommandExecuted(object parameter)
        {
            if (!CanExportUserDictionaryCommandExecute(parameter))
                return;

            if (!DialogProvider.SaveUserDictFile(out string path))
                return;

            if (!UserDictsIndex.GetInstance().TryExportIndex(path))
                new QuickMessage($"Failed to export dictionary.");
        }

        public ICommand ImportUserDictionaryCommand { get; }
        public bool CanImportUserDictionaryCommandExecute(object parameter)
        {
            return QueryExecutionState != OperationState.InProcess;
        }
        public void OnImportUserDictionaryCommandExecuted(object parameter)
        {
            if (!CanImportUserDictionaryCommandExecute(parameter))
                return;

            if (!DialogProvider.GetUserDictFilePath(out string path))
                return;

            if (!UserDictsIndex.GetInstance().TryImportIndex(path))
            {
                new QuickMessage($"Failed to import dictionary.");
                return;
            }

            var userDictItems = LoadUserDictionaryItems(false);
            UserDictionary = new ObservableCollection<IUserDictionaryItem>(userDictItems);

            OnPropertyChanged(nameof(UserDictionary));

            SelectedUserDictItem = UserDictionary.FirstOrDefault();
        }

        public ICommand RemoveDictItemCommand { get; }
        public bool CanRemoveDictItemCommandExecute(object parameter)
        {
            return SelectedUserDictItem != null;
        }
        public void OnRemoveDictItemCommandExecuted(object parameter)
        {
            if (!CanRemoveDictItemCommandExecute(parameter))
                return;

            UserDictsIndex.GetInstance().DeleteIndexEntry(SelectedUserDictItem.Name);

            int removedItemIndex = UserDictionary.IndexOf(SelectedUserDictItem);
            int selectedItemIndex = removedItemIndex >= 1 ? removedItemIndex - 1 : 0;

            UserDictionary.Remove(SelectedUserDictItem);

            SelectedUserDictItem = UserDictionary.Count > 0
                ? UserDictionary[selectedItemIndex]
                : null;
        }

        public ICommand AddUserDictItemCommand { get; }
        public bool CanAddUserDictItemCommandExecute(object parameter)
        {
            return !string.IsNullOrEmpty(DictEditorConsoleText) &&
                   !UserDictionary.Any(t => t.Name == DictEditorConsoleText);
        }
        public void OnAddUserDictItemCommandExecuted(object parameter)
        {
            if (!CanAddUserDictItemCommandExecute(parameter))
                return;

            var item = new UserDictionaryItem(DictEditorConsoleText);

            UserDictionary.Add(item);
            UserDictsIndex.GetInstance().AddIndexEntry(item.Name, item.Words.ToList());

            SelectedUserDictItem = item;
            DictEditorConsoleText = string.Empty;
        }

        public ICommand AddWordToUserDictItemCommand { get; }
        public bool CanAddWordToUserDictItemCommandExecute(object parameter)
        {
            return !string.IsNullOrEmpty(DictEditorConsoleText) &&
                   SelectedUserDictItem != null &&
                   SelectedUserDictItem.CanAddWordToContent(DictEditorConsoleText);
        }
        public void OnAddWordToUserDictItemCommandExecuted(object parameter)
        {
            if (!CanAddWordToUserDictItemCommandExecute(parameter))
                return;

            UserDictsIndex.GetInstance().AddWordToIndexEntry(SelectedUserDictItem.Name,
                DictEditorConsoleText);

            SelectedUserDictItem.AddWordToContent(DictEditorConsoleText);
            CurrentUserDictItemWords.Add(DictEditorConsoleText);

            DictEditorConsoleText = string.Empty;
        }

        public ICommand RemoveWordFromUserDictItemCommand { get; }
        public bool CanRemoveWordFromUserDictItemCommandExecute(object parameter)
        {
            return !string.IsNullOrEmpty(SelectedUserDictItemWord);
        }
        public void OnRemoveWordFromUserDictItemCommandExecuted(object parameter)
        {
            if (!CanRemoveWordFromUserDictItemCommandExecute(parameter))
                return;

            UserDictsIndex.GetInstance().RemoveWordFromIndexEntry(SelectedUserDictItem.Name,
                SelectedUserDictItemWord);

            int removedWordIndex = CurrentUserDictItemWords.IndexOf(SelectedUserDictItemWord);
            int selectedWordIndex = removedWordIndex >= 1 ? removedWordIndex - 1 : 0;

            SelectedUserDictItem.RemoveWordFromContent(SelectedUserDictItemWord);
            CurrentUserDictItemWords.Remove(SelectedUserDictItemWord);

            SelectedUserDictItemWord = CurrentUserDictItemWords.Count > 0
                ? CurrentUserDictItemWords[selectedWordIndex]
                : null;
        }

        #endregion

        #region SuggestionsCommands

        public ICommand ShowPreviousSuggestionCommand { get; }
        public bool CanShowPreviousSuggestionCommandExecute(object parameter)
        {
            return CurrentSuggestionIndex > 1;
        }
        public void OnShowPreviousSuggestionCommandExecuted(object parameter)
        {
            if (!CanShowPreviousSuggestionCommandExecute(parameter))
                return;

            CurrentSuggestionIndex--;
            DisplaySituation(CurrentSuggestionIndex - 1);
        }

        public ICommand ShowNextSuggestionCommand { get; }
        public bool CanShowNextSuggestionCommandExecute(object parameter)
        {
            return CurrentSuggestionIndex < SuggestionsCount;
        }
        public void OnShowNextSuggestionCommandExecuted(object parameter)
        {
            if (!CanShowNextSuggestionCommandExecute(parameter))
                return;

            CurrentSuggestionIndex++;
            DisplaySituation(CurrentSuggestionIndex - 1);
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

            if (QueryTextBoxVisibility == Visibility.Visible)
            {
                AddElementToQueryTextCommand.Execute(parameter);
                return;
            }

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

            AddButtonToQueryItems(sourceBtn);
        }

        #endregion

        #region SystemCommands

        public ICommand EndTasksCommand { get; }
        public bool CanEndTasksCommandExecute(object parameter)
        {
            return parameter is CancelEventArgs;
        }
        public void OnEndTasksCommandExecuted(object parameter)
        {
            if (!CanEndTasksCommandExecute(parameter))
                return;

            _isWindowClosing = true;
        }

        public ICommand SaveUserDictionaryCommand { get; }
        public bool CanSaveUserDictionaryCommandExecute(object parameter)
        {
            return parameter is CancelEventArgs;
        }
        public void OnSaveUserDictionaryCommandExecuted(object parameter)
        {
            if (!CanSaveUserDictionaryCommandExecute(parameter))
                return;

            try
            {
                UserDictsIndex index = UserDictsIndex.GetInstance();

                index.FlushIndexToDisk();
                index.UnloadData();
            }
            catch (Exception ex)
            {
                var msgRes = new QuickMessage($"The dictionary could not be saved. Reason: {ex.Message}" +
                    "\n\nClose without saving?").ShowError(MessageBoxButton.YesNo);

                if (msgRes == MessageBoxResult.No)
                {
                    var eventArgs = parameter as CancelEventArgs;
                    eventArgs.Cancel = true;
                }
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
            }
            catch (Exception ex)
            {
                new QuickMessage($"Error: {ex.Message}").ShowError();
            }
        }

        #endregion

        public SuggesterWindowViewModel(MainWindowViewModel mainWindowVM)
        {
            MainWindowVM = mainWindowVM ?? throw new ArgumentNullException(nameof(mainWindowVM));

            _chatColumnCreator = new ChatColumnCreator();

            QueryItems = new ObservableCollection<Button>();
            CurrentGroupMessages = new ObservableCollection<ChatMessage>();

            var generatedColumns = _chatColumnCreator.GenerateChatColumns(ProjectInfo.Data.SelectedFields, false, false);
            MessageContainerColumns = new ObservableCollection<DataGridColumn>(generatedColumns);

            var userDictItems = LoadUserDictionaryItems(true);
            UserDictionary = new ObservableCollection<IUserDictionaryItem>(userDictItems);

            CurrentUserDictItemWords = new ObservableCollection<string>();
            SelectedUserDictItem = UserDictionary.FirstOrDefault();

            _orderedButtonBackgrounds = new SolidColorBrush[]
            {
                SemicolonBtnBackground,
                CommaBtnBackground,
                RightBracketBtnBackground,
                LeftBracketBtnBackground,
                NumBtnBackground,
                AndBtnBackground,
                SelectBtnBackground,
                NotBtnBackground,
                OrBtnBackground,
                InwinBtnBackground,
                HasOrganizationBtnBackground,
                HasDateBtnBackground,
                HasTimeBtnBackground,
                HasUserMentionedBtnBackground,
                HasLocationBtnBackground,
                ByUserBtnBackground,
                HasWordOfDictBtnBackground
            };

            LastQueryItemChanged += UpdateHighlights;

            #region InitializeCommands

            SwitchModeCommand = new RelayCommand(OnSwitchModeCommandExecuted, CanSwitchModeCommandExecute);
            AddElementToQueryTextCommand = new RelayCommand(OnAddElementToQueryTextCommandExecuted, CanAddElementToQueryTextCommandExecute);
            ClearQueryCommand = new RelayCommand(OnClearQueryCommandExecuted, CanClearQueryCommandExecute);
            RunQueryCommand = new RelayCommand(OnRunQueryCommandExecuted, CanRunQueryCommandExecute);

            ImportQueriesCommand = new RelayCommand(OnImportQueriesCommandExecuted, CanImportQueriesCommandExecute);
            LoadImportedQueryCommand = new RelayCommand(OnLoadImportedQueryCommandExecuted, CanLoadImportedQueryCommandExecute);
            RemoveImportedQueryCommand = new RelayCommand(OnRemoveImportedQueryCommandExecuted, CanRemoveImportedQueryCommandExecute);
            RemoveAllImportedQueriesCommand = new RelayCommand(OnRemoveAllImportedQueriesCommandExecuted, CanRemoveAllImportedQueriesCommandExecute);
            SwitchImportQueriesPanelVisibilityCommand = new RelayCommand(OnSwitchImportQueriesPanelVisibilityCommandExecuted, CanSwitchImportQueriesPanelVisibilityCommandExecute);

            SwitchDictionaryEditorVisibilityCommand = new RelayCommand(OnSwitchDictionaryEditorVisibilityCommandExecuted, CanSwitchDictionaryEditorVisibilityCommandExecute);
            ExportUserDictionaryCommand = new RelayCommand(OnExportUserDictionaryCommandExecuted, CanExportUserDictionaryCommandExecute);
            ImportUserDictionaryCommand = new RelayCommand(OnImportUserDictionaryCommandExecuted, CanImportUserDictionaryCommandExecute);
            RemoveDictItemCommand = new RelayCommand(OnRemoveDictItemCommandExecuted, CanRemoveDictItemCommandExecute);
            AddUserDictItemCommand = new RelayCommand(OnAddUserDictItemCommandExecuted, CanAddUserDictItemCommandExecute);
            AddWordToUserDictItemCommand = new RelayCommand(OnAddWordToUserDictItemCommandExecuted, CanAddWordToUserDictItemCommandExecute);
            RemoveWordFromUserDictItemCommand = new RelayCommand(OnRemoveWordFromUserDictItemCommandExecuted, CanRemoveWordFromUserDictItemCommandExecute);

            ShowPreviousSuggestionCommand = new RelayCommand(OnShowPreviousSuggestionCommandExecuted, CanShowPreviousSuggestionCommandExecute);
            ShowNextSuggestionCommand = new RelayCommand(OnShowNextSuggestionCommandExecuted, CanShowNextSuggestionCommandExecute);

            DragButtonCommand = new RelayCommand(OnDragButtonCommandExecuted, CanDragButtonCommandExecute);
            WrapPanelTakeDropCommand = new RelayCommand(OnWrapPanelTakeDropCommandExecuted, CanWrapPanelTakeDropCommandExecute);

            EndTasksCommand = new RelayCommand(OnEndTasksCommandExecuted, CanEndTasksCommandExecute);
            SaveUserDictionaryCommand = new RelayCommand(OnSaveUserDictionaryCommandExecuted, CanSaveUserDictionaryCommandExecute);
            DeactivateWindowCommand = new RelayCommand(OnDeactivateWindowCommandExecuted, CanDeactivateWindowCommandExecute);
            
            #endregion
        }

        #region UserDictionaryMethods

        private IEnumerable<IUserDictionaryItem> LoadUserDictionaryItems(bool readIndexFromDisk)
        {
            UserDictsIndex index = UserDictsIndex.GetInstance();

            if (readIndexFromDisk)
            {
                try
                {
                    index.ReadIndexFromDisk();
                }
                catch { }
            }

            IUserDictionaryItem[] items = index.IndexCollection
                .Select(t => new UserDictionaryItem(t.Key, t.Value.ToArray()))
                .ToArray();

            return items;
        }

        #endregion

        #region QueryMethods

        private string CreateQueryFromItems()
        {
            if (QueryItems.IsNullOrEmpty())
                return string.Empty;

            var builder = new StringBuilder();

            foreach (var btn in QueryItems)
            {
                string content = btn.Content.ToString();

                if (content == "," || content == ";" ||
                    content == "(" || content == ")" ||
                    content == "select")
                {
                    builder.Append(content);
                }
                else
                {
                    builder.Append(" " + content);
                }
            }

            return builder.ToString();
        }

        private void DisplayQueryResult(Task queryParsingTask)
        {
            if (_isWindowClosing)
                return;
            
            if (queryParsingTask.IsCanceled)
            {
                QueryExecutionState = OperationState.Aborted;
                return;
            }

            if (queryParsingTask.IsFaulted)
            {
                QueryExecutionState = OperationState.Fail;

                new QuickMessage($"Failed to execute query. " +
                    $"Reason: {queryParsingTask.Exception?.Message}").ShowError();

                return;
            }

            if (_queryResult == null)
            {
                QueryExecutionState = OperationState.Fail;
                new QuickMessage("Incorrect query").ShowError();

                return;
            }

            CurrentSuggestionIndex = 1;
            SuggestionsCount = _queryResult.Count;

            UpdateQueryResultInfo();

            try
            {
                DisplaySituation(CurrentSuggestionIndex - 1);
                QueryExecutionState = OperationState.Success;
            }
            catch (Exception ex)
            {
                QueryExecutionState = OperationState.Fail;

                new QuickMessage($"Failed to execute query. Reason: {ex.Message}")
                    .ShowError();
            }
        }

        private void UpdateQueryResultInfo()
        {
            if (_queryResult.IsNullOrEmpty())
            {
                FoundGroupsCount = 0;
                FoundSuggestionsCount = 0;
                CurrentSuggestionIndex = 0;
                SuggestionsCount = 0;
            }
            else
            {
                int foundSuggestions = 0;

                foreach (var list in _queryResult)
                {
                    foreach (var res in list)
                        foundSuggestions += res.Count;
                }

                FoundSuggestionsCount = foundSuggestions;
                FoundGroupsCount = _queryResult.Count;
            }
        }

        private void SwitchQueryExecutionWaitingPanelVisibility()
        {
            IsQueryExecutionWaitingIconSpinActive = !IsQueryExecutionWaitingIconSpinActive;

            QueryExecutionWaitingPanelVisibility = QueryExecutionWaitingPanelVisibility == Visibility.Hidden
                ? Visibility.Visible
                : Visibility.Hidden;
        }

        #endregion

        #region DisplaySituationMethods

        private void DisplaySituation(int index)
        {
            if (_queryResult.IsNullOrEmpty())
            {
                if (_queryResult == null)
                    new QuickMessage("Incorrect query.").ShowWarning();

                else if (_queryResult.Count == 0)
                    new QuickMessage("Nothing found.").ShowInformation();

                return;
            }

            var hits = new List<int>();
            var groupMessages = new List<DynamicMessage>();

            foreach (var list in _queryResult[index])
                hits.AddRange(list);

            hits.Sort();

            int contextMsgIndex1 = hits.Min() - 2;
            int contextMsgIndex2 = hits.Min() - 1;
            int contextMsgIndex3 = hits.Max() + 1;
            int contextMsgIndex4 = hits.Max() + 2;

            if (contextMsgIndex1 >= 0)
                groupMessages.Add(LuceneService.RetrieveMessageById(hits.Min() - 2));

            if (contextMsgIndex2 >= 0)
                groupMessages.Add(LuceneService.RetrieveMessageById(hits.Min() - 1));

            for (int i = hits[0]; i <= hits[hits.Count - 1]; ++i)
                groupMessages.Add(LuceneService.RetrieveMessageById(i));

            if (contextMsgIndex3 <= ProjectInteraction.LastMessageId)
                groupMessages.Add(LuceneService.RetrieveMessageById(hits.Max() + 1));

            if (contextMsgIndex4 <= ProjectInteraction.LastMessageId)
                groupMessages.Add(LuceneService.RetrieveMessageById(hits.Max() + 2));

            CurrentGroupMessages = new ObservableCollection<ChatMessage>(
                groupMessages.Select(t => new ChatMessage(t)).OrderBy(t => t.SentDate).ToArray());

            _hits = hits;

            foreach (var msg in CurrentGroupMessages)
            {
                if (_hits.Contains(msg.Source.Id))
                    msg.BackgroundBrush = Brushes.Pink;
            }
            
            OnPropertyChanged(nameof(CurrentGroupMessages));
        }

        #endregion

        #region HighlightMethods

        private void UpdateHighlights()
        {
            if (QueryItems.Count == 0)
            {
                foreach (var brush in _orderedButtonBackgrounds)
                    brush.Color = DefaultButtonBackgroundColor;

                return;
            }

            string lastOperator = QueryItems[^1].Content.ToString() ?? string.Empty;

            if (lastOperator.Contains('(') && lastOperator.Contains(')'))
            {
                int leftBracketIndex = lastOperator.IndexOf("(");
                lastOperator = lastOperator.Remove(leftBracketIndex) + "()";
            }

            if (char.IsDigit(lastOperator[0]))
                lastOperator = "num";

            for (int i = 0; i < HighlightRules[lastOperator].Length; ++i)
            {
                _orderedButtonBackgrounds[i].Color = HighlightRules[lastOperator][i] == 1
                    ? HighlightedButtonBackgroundColor
                    : DefaultButtonBackgroundColor;
            }
        }

        #endregion

        #region QueryItemMethods

        private ComboBox CreateDictSelectionComboBox(Button senderBtn)
        {
            string sourceContent = senderBtn.Content.ToString();
            int leftBracketIndex = sourceContent.IndexOf("(");

            string paramName = sourceContent.Substring(leftBracketIndex + 1);
            paramName = paramName.Remove(paramName.Length - 1);

            int selectedIndex = UserDictionary.IndexOf(
                UserDictionary.FirstOrDefault(t => t.Name == paramName));

            var comboBox = new ComboBox()
            {
                Width = 100,
                Height = 20,
                FontSize = 10,
                Padding = new Thickness(6, 0, 6, 0)
            };

            comboBox.SetItems(selectedIndex, true, UserDictionary.ToArray());

            comboBox.SelectionChanged += (object sender, SelectionChangedEventArgs e) =>
            {
                string dict = comboBox.SelectedIndex != -1
                    ? comboBox.Items[comboBox.SelectedIndex].ToString()
                    : null;

                senderBtn.Content = $"haswordofdict({dict})";
            };

            return comboBox;
        }

        private TextBox CreateParamEditingTextBox(Button senderBtn)
        {
            string sourceContent = senderBtn.Content.ToString();

            int leftBracketIndex = sourceContent.IndexOf("(");
            string funcName = sourceContent.Remove(leftBracketIndex);

            string paramName = sourceContent.Substring(leftBracketIndex + 1);
            paramName = paramName.Remove(paramName.Length - 1);

            var textBox = new TextBox()
            {
                Width = 100,
                Height = 20,
                FontSize = 12,
                Text = paramName,
                Padding = new Thickness(0, 0, 0, 0)
            };

            textBox.KeyDown += (object sender, KeyEventArgs e) =>
            {
                if (e.Key == Key.Enter)
                    IsQueryItemPopupOpen = false;
            };

            textBox.TextChanged += (object sender, TextChangedEventArgs e) =>
            {
                senderBtn.Content = $"{funcName}({textBox.Text})";
            };

            return textBox;
        }

        private TextBox CreateNumberTextBox(Button senderBtn)
        {
            string sourceContent = senderBtn.Content.ToString();
            bool isNumber = uint.TryParse(sourceContent, out _);

            var textBox = new TextBox()
            {
                Width = 100,
                Height = 20,
                FontSize = 12,
                Padding = new Thickness(0, 0, 0, 0),
                Text = isNumber ? sourceContent : string.Empty
            };

            textBox.KeyDown += (object sender, KeyEventArgs e) =>
            {
                if (e.Key == Key.Enter)
                    IsQueryItemPopupOpen = false;
            };

            textBox.TextChanged += (object sender, TextChangedEventArgs e) =>
            {
                bool isCorrect = uint.TryParse(textBox.Text, out uint num);
                senderBtn.Content = isCorrect ? num.ToString() : "num";
            };

            return textBox;
        }

        private void UpdateQueryItemPopup(Button source)
        {
            string sourceContent = source.Content.ToString();

            if (sourceContent.Contains("haswordofdict"))
                QueryItemPopupChild = CreateDictSelectionComboBox(source);

            else if (sourceContent.Contains("hasusermentioned") || sourceContent.Contains("byuser"))
                QueryItemPopupChild = CreateParamEditingTextBox(source);

            else if (sourceContent.Contains("num") || char.IsDigit(sourceContent.FirstOrDefault()))
                QueryItemPopupChild = CreateNumberTextBox(source);
        }

        private Button CreateQueryItem(Button source)
        {
            Button btn = new Button
            {
                Content = source.Content,
                Height = source.ActualHeight,
                Foreground = source.Foreground,
                BorderBrush = source.BorderBrush,
                BorderThickness = source.BorderThickness,
                FontFamily = source.FontFamily,
                FontSize = source.FontSize,

                AllowDrop = true,
                Cursor = Cursors.SizeAll,
                Margin = new Thickness(3, 3, 3, 3),
                Padding = new Thickness(4, 0, 4, 0),
                Background = new SolidColorBrush(DefaultButtonBackgroundColor)
            };

            btn.Drop += QueryItemButton_Drop;
            btn.DragEnter += QueryItemButton_DragEnter;
            btn.PreviewMouseLeftButtonDown += DragDropButton_MouseDown;
            btn.MouseRightButtonDown += QueryItemButton_MouseRightButtonDown;
            btn.MouseDoubleClick += QueryItemButton_MouseDoubleClick;

            return btn;
        }

        #endregion

        #region DragDropMethods

        private void AddButtonToQueryItems(Button source)
        {
            Button btn = CreateQueryItem(source);
            QueryItems.Add(btn);

            LastQueryItemChanged?.Invoke();
        }

        private void InsertButtonToQueryItems(Button source, int index)
        {
            Button btn = CreateQueryItem(source);
            QueryItems.Insert(index, btn);

            if (index == QueryItems.Count - 1)
                LastQueryItemChanged?.Invoke();
        }

        #endregion

        #region DragDropEventMethods

        private void QueryItemButton_Drop(object sender, DragEventArgs e)
        {
            if (e.KeyStates == DragDropKeyStates.LeftMouseButton)
                return;
            
            var data = e.Data as DataObject;
            Button sourceBtn = data.GetData(typeof(Button)) as Button;

            if (QueryItems.Contains(sourceBtn))
                return;

            int index = QueryItems.IndexOf(sender as Button);
            InsertButtonToQueryItems(sourceBtn, index);

            e.Handled = true;
        }

        private void QueryItemButton_DragEnter(object sender, DragEventArgs e)
        {
            var data = e.Data as DataObject;
            Button source = data.GetData(typeof(Button)) as Button;

            if (!QueryItems.Contains(source))
                return;

            Button senderBtn = sender as Button;

            if (senderBtn == _lastDragDropSwitch.Item1)
            {
                TimeSpan diff = DateTime.Now.Subtract(_lastDragDropSwitch.Item2);

                if (diff.TotalMilliseconds < DRAG_DROP_SWITCH_PAUSE)
                    return;
            }

            var lastItem = QueryItems[^1];
            _lastDragDropSwitch = new Tuple<Button, DateTime>(senderBtn, DateTime.Now);

            QueryItems.MoveItem(source, sender as Button);

            if (lastItem != QueryItems[^1])
                LastQueryItemChanged?.Invoke();
        }

        private void QueryItemButton_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var senderBtn = sender as Button;
            string content = senderBtn.Content.ToString();

            if (!content.Contains("haswordofdict") &&
                !content.Contains("hasusermentioned") &&
                !content.Contains("byuser") &&
                !content.Contains("num") &&
                !char.IsDigit(content.FirstOrDefault()))
            {
                return;
            }

            UpdateQueryItemPopup(senderBtn);
            IsQueryItemPopupOpen = true;
        }

        private void QueryItemButton_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var senderBtn = sender as Button;
            bool isLastItem = QueryItems.IndexOf(senderBtn) == QueryItems.Count - 1;

            QueryItems.Remove(sender as Button);

            if (isLastItem)
                LastQueryItemChanged?.Invoke();
        }

        private void DragDropButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            OnDragButtonCommandExecuted(sender as Button);
        }

        #endregion
    }
}
