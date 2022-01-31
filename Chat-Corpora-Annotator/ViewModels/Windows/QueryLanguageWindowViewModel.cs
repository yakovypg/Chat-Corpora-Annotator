using ChatCorporaAnnotator.Data.Parsers.Suggester;
using ChatCorporaAnnotator.Infrastructure.Commands;
using ChatCorporaAnnotator.Infrastructure.Extensions;
using ChatCorporaAnnotator.Infrastructure.Extensions.Controls;
using ChatCorporaAnnotator.Models.Chat;
using ChatCorporaAnnotator.Models.Messages;
using ChatCorporaAnnotator.Models.Suggester;
using ChatCorporaAnnotator.ViewModels.Base;
using IndexEngine.Containers;
using IndexEngine.Search;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace ChatCorporaAnnotator.ViewModels.Windows
{
    internal class QueryLanguageWindowViewModel : ViewModel
    {
        private const int DRAG_DROP_SWITCH_PAUSE = 1000;

        private readonly MainWindowViewModel _mainWindowVM;

        private delegate void LastQueryItemChangedHandler();
        private event LastQueryItemChangedHandler LastQueryItemChanged;

        public Action DeactivateAction { get; set; }
        public ObservableCollection<UserDictionaryItem> UserDictionary { get; }

        #region QueryItems

        private Tuple<Button, DateTime> _lastDragDropSwitch =
            new Tuple<Button, DateTime>(null, DateTime.MinValue);

        private List<List<List<int>>> _queryResult =
            new List<List<List<int>>>();

        private List<int> _hits = new List<int>();

        public ObservableCollection<Button> QueryItems { get; }
        public ObservableCollection<ChatMessage> CurrentSituation { get; private set; }

        private string _queryText = string.Empty;
        public string QueryText
        {
            get => _queryText;
            set => SetValue(ref _queryText, value);
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

        #region VisibilityItems

        private Visibility _queryTextBoxVisibility = Visibility.Visible;
        public Visibility QueryTextBoxVisibility
        {
            get => _queryTextBoxVisibility;
            set => SetValue(ref _queryTextBoxVisibility, value);
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

        public ICommand AddElementToQueryTextCommand { get; }
        public bool CanAddElementToQueryTextCommandExecute(object parameter)
        {
            return true;
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
            return true;
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
            return true;
        }
        public void OnRunQueryCommandExecuted(object parameter)
        {
            if (!CanRunQueryCommandExecute(parameter))
                return;

            string query = QueryTextBoxVisibility == Visibility.Hidden
                ? CreateQueryFromItems()
                : QueryText;

            _queryResult = QueryParser.Parse(query);

            CurrentSuggestionIndex = 0;
            SuggestionsCount = _queryResult.Count;

            UpdateQueryResultInfo();
            DisplaySituation(CurrentSuggestionIndex);
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
            return CurrentSuggestionIndex > 0;
        }
        public void OnShowPreviousSuggestionCommandExecuted(object parameter)
        {
            if (!CanShowPreviousSuggestionCommandExecute(parameter))
                return;

            CurrentSuggestionIndex--;
            DisplaySituation(CurrentSuggestionIndex);
        }

        public ICommand ShowNextSuggestionCommand { get; }
        public bool CanShowNextSuggestionCommandExecute(object parameter)
        {
            return CurrentSuggestionIndex < SuggestionsCount - 1;
        }
        public void OnShowNextSuggestionCommandExecuted(object parameter)
        {
            if (!CanShowNextSuggestionCommandExecute(parameter))
                return;

            CurrentSuggestionIndex++;
            DisplaySituation(CurrentSuggestionIndex);
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

            QueryItems = new ObservableCollection<Button>();
            UserDictionary = new ObservableCollection<UserDictionaryItem>();

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
            ImportQueriesCommand = new RelayCommand(OnImportQueriesCommandExecuted, CanImportQueriesCommandExecute);
            AddElementToQueryTextCommand = new RelayCommand(OnAddElementToQueryTextCommandExecuted, CanAddElementToQueryTextCommandExecute);
            ClearQueryCommand = new RelayCommand(OnClearQueryCommandExecuted, CanClearQueryCommandExecute);
            RunQueryCommand = new RelayCommand(OnRunQueryCommandExecuted, CanRunQueryCommandExecute);

            ShowDictionaryEditorCommand = new RelayCommand(OnShowDictionaryEditorCommandExecuted, CanShowDictionaryEditorCommandExecute);

            ShowPreviousSuggestionCommand = new RelayCommand(OnShowPreviousSuggestionCommandExecuted, CanShowPreviousSuggestionCommandExecute);
            ShowNextSuggestionCommand = new RelayCommand(OnShowNextSuggestionCommandExecuted, CanShowNextSuggestionCommandExecute);

            DragButtonCommand = new RelayCommand(OnDragButtonCommandExecuted, CanDragButtonCommandExecute);
            WrapPanelTakeDropCommand = new RelayCommand(OnWrapPanelTakeDropCommandExecuted, CanWrapPanelTakeDropCommandExecute);

            DeactivateWindowCommand = new RelayCommand(OnDeactivateWindowCommandExecuted, CanDeactivateWindowCommandExecute);
            
            #endregion
        }

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
            var currentSituation = new List<DynamicMessage>();

            foreach (var list in _queryResult[index])
                hits.AddRange(list);

            hits.Sort();
            _hits = hits;

            currentSituation.Add(LuceneService.RetrieveMessageById(hits.Min() - 2));
            currentSituation.Add(LuceneService.RetrieveMessageById(hits.Min() - 1));

            for (int i = hits[0]; i <= hits[hits.Count - 1]; ++i)
                currentSituation.Add(LuceneService.RetrieveMessageById(i));

            currentSituation.Add(LuceneService.RetrieveMessageById(hits.Max() + 1));
            currentSituation.Add(LuceneService.RetrieveMessageById(hits.Max() + 2));

            CurrentSituation = new ObservableCollection<ChatMessage>(
                currentSituation.Select(t => new ChatMessage(t)).ToArray());

            OnPropertyChanged(nameof(CurrentSituation));

            //SetColumns();
            //suggesterView.Sort(suggesterView.AllColumns.Find(x => x.Text.Equals(ProjectInfo.DateFieldKey)), SortOrder.Ascending);
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

            string lastOperator = QueryItems.Last().Content.ToString();

            if (lastOperator.Contains("(") && lastOperator.Contains(")"))
            {
                int leftBracketIndex = lastOperator.IndexOf("(");
                lastOperator = lastOperator.Remove(leftBracketIndex) + "()";
            }

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
                FontSize = 12
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

            var lastItem = QueryItems.Last();
            _lastDragDropSwitch = new Tuple<Button, DateTime>(senderBtn, DateTime.Now);

            QueryItems.MoveItem(source, sender as Button);

            if (lastItem != QueryItems.Last())
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
