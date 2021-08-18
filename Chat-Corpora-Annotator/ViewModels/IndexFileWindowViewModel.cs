using ChatCorporaAnnotator.Data;
using ChatCorporaAnnotator.Data.App;
using ChatCorporaAnnotator.Infrastructure.Commands;
using ChatCorporaAnnotator.Infrastructure.Exceptions;
using ChatCorporaAnnotator.Models.Indexing;
using ChatCorporaAnnotator.Models.Messages;
using ChatCorporaAnnotator.Models.Windows;
using ChatCorporaAnnotator.Services;
using ChatCorporaAnnotator.ViewModels.Base;
using ChatCorporaAnnotator.Views.Windows;
using IndexEngine;
using IndexEngine.Paths;
using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ChatCorporaAnnotator.ViewModels
{
    internal class IndexFileWindowViewModel : ViewModel
    {
        private readonly IPageToggler _pageToggler;
        private readonly MainWindowViewModel _mainWindowVM;

        private string _filePath;
        private readonly string _projectFolderPath;

        public ObservableCollection<FileColumn> FileColumns { get; private set; }
        public ObservableCollection<FileColumn> SelectedFileColumns { get; private set; }

        private string _title = $"Step 1 of {PagesCount}";
        public string Title
        {
            get => _title;
            set => SetValue(ref _title, value);
        }

        private bool _isFileReaded = false;
        public bool IsFileReaded
        {
            get => _isFileReaded;
            set => SetValue(ref _isFileReaded, value);
        }

        private bool _isFileProcessed = false;
        public bool IsFileProcessed
        {
            get => _isFileProcessed;
            set => SetValue(ref _isFileProcessed, value);
        }

        #region Data

        public Delimiter[] Delimiters { get; } = new Delimiter[]
        {
            new Delimiter("Comma", ","),
            new Delimiter("Tab", @"    "),
            new Delimiter("Pipe", "|"),
            new Delimiter("Colon", ";")
        };

        #endregion

        #region StaticData

        private static readonly string[] PageHints = new string[]
        {
            "Select file delimiter",
            "Select the columns to be uploaded",
            "Select specified keys",
            "Please wait",
            "Data loaded",
        };

        private static readonly int MaxToggleablePageIndex = 2;
        private static readonly int PagesCount = PageHints.Length;

        #endregion

        #region SelectedItems

        private Delimiter _selectedDelimiter;
        public Delimiter SelectedDelimiter
        {
            get => _selectedDelimiter;
            set => SetValue(ref _selectedDelimiter, value);
        }

        private FileColumn _selectedDateColumn;
        public FileColumn SelectedDateColumn
        {
            get => _selectedDateColumn;
            set => SetValue(ref _selectedDateColumn, value);
        }

        private FileColumn _selectedSenderColumn;
        public FileColumn SelectedSenderColumn
        {
            get => _selectedSenderColumn;
            set => SetValue(ref _selectedSenderColumn, value);
        }

        private FileColumn _selectedTextColumn;
        public FileColumn SelectedTextColumn
        {
            get => _selectedTextColumn;
            set => SetValue(ref _selectedTextColumn, value);
        }

        #endregion

        #region PageItems

        private string _currentPageHint = PageHints[0];
        public string CurrentPageHint
        {
            get => _currentPageHint;
            set => SetValue(ref _currentPageHint, value);
        }

        private int _currentPageIndex = 0;
        public int CurrentPageIndex
        {
            get => _currentPageIndex;
            set
            {
                SetValue(ref _currentPageIndex, value);

                CurrentPageHint = PageHints[value];
                Title = $"Step {value + 1} of {PagesCount}";
            }
        }

        #endregion

        #region PagesVisibilities

        private Visibility _selectDelimiterPageVisibility = Visibility.Visible;
        public Visibility SelectDelimiterPageVisibility
        {
            get => _selectDelimiterPageVisibility;
            set => SetValue(ref _selectDelimiterPageVisibility, value);
        }

        private Visibility _selectColumnsPageVisibility = Visibility.Hidden;
        public Visibility SelectColumnsPageVisibility
        {
            get => _selectColumnsPageVisibility;
            set => SetValue(ref _selectColumnsPageVisibility, value);
        }

        private Visibility _selectSpecifiedKeysPageVisibility = Visibility.Hidden;
        public Visibility SelectSpecifiedKeysPageVisibility
        {
            get => _selectSpecifiedKeysPageVisibility;
            set => SetValue(ref _selectSpecifiedKeysPageVisibility, value);
        }

        private Visibility _waitPageVisibility = Visibility.Hidden;
        public Visibility WaitPageVisibility
        {
            get => _waitPageVisibility;
            set => SetValue(ref _waitPageVisibility, value);
        }

        private Visibility _finishPageVisibility = Visibility.Hidden;
        public Visibility FinishPageVisibility
        {
            get => _finishPageVisibility;
            set => SetValue(ref _finishPageVisibility, value);
        }

        #endregion

        #region PageToggleCommands

        public ICommand SetBackPageCommand { get; }
        public bool CanSetBackPageCommandExecute(object parameter)
        {
            return CurrentPageIndex > 0 && CurrentPageIndex <= MaxToggleablePageIndex;
        }
        public void OnSetBackPageCommandExecuted(object parameter)
        {
            if (!CanSetBackPageCommandExecute(parameter))
                return;

            CurrentPageIndex--;
            _pageToggler.BackPage();
        }

        public ICommand SetNextPageCommand { get; }
        public bool CanSetNextPageCommandExecute(object parameter)
        {
            return CurrentPageIndex >= 0 &&
                   CurrentPageIndex <= MaxToggleablePageIndex + 1 &&
                   IsPageDataSelected(CurrentPageIndex);
        }
        public void OnSetNextPageCommandExecuted(object parameter)
        {
            if (!CanSetNextPageCommandExecute(parameter))
                return;

            CurrentPageIndex++;
            _pageToggler.NextPage();
        }

        public ICommand FinishFileIndexingCommand { get; }
        public bool CanFinishFileIndexingCommandExecute(object parameter)
        {
            return CurrentPageIndex == PagesCount - 1;
        }
        public void OnFinishFileIndexingCommandExecuted(object parameter)
        {
            if (!CanFinishFileIndexingCommandExecute(parameter))
                return;

            CloseWindowCommand?.Execute(parameter);
        }

        #endregion

        #region SelectedItemsCommands

        public ICommand CheckAllColumnsCommand { get; }
        public bool CanCheckAllColumnsCommandExecute(object parameter)
        {
            return FileColumns != null && FileColumns.Count > 0;
        }
        public void OnCheckAllColumnsCommandExecuted(object parameter)
        {
            if (!CanCheckAllColumnsCommandExecute(parameter))
                return;

            foreach (var column in FileColumns)
                column.IsSelected = true;
        }

        public ICommand UncheckAllColumnsCommand { get; }
        public bool CanUncheckAllColumnsCommandExecute(object parameter)
        {
            return FileColumns != null && FileColumns.Count > 0;
        }
        public void OnUncheckAllColumnsCommandExecuted(object parameter)
        {
            if (!CanUncheckAllColumnsCommandExecute(parameter))
                return;

            foreach (var column in FileColumns)
                column.IsSelected = false;
        }

        public ICommand FileColumnsListSelectionChangedCommand { get; }
        public bool CanFileColumnsListSelectionChangedCommandExecute(object parameter)
        {
            return parameter is SelectionChangedEventArgs;
        }
        public void OnFileColumnsListSelectionChangedCommandExecute(object parameter)
        {
            if (!CanFileColumnsListSelectionChangedCommandExecute(parameter))
                return;

            IList addedItems = (parameter as SelectionChangedEventArgs).AddedItems;

            if (addedItems == null)
                return;

            foreach (var item in addedItems)
            {
                if (item is FileColumn addingItem && !addingItem.IsSelected)
                    addingItem.IsSelected = true;
            }
        }

        #endregion

        #region SystemCommands

        public ICommand CloseWindowCommand { get; }
        public bool CanCloseWindowCommandExecute(object parameter)
        {
            return Application.Current != null &&
                   Application.Current.Windows != null &&
                   Application.Current.Windows.Count > 0;
        }
        public void OnCloseWindowCommandExecuted(object parameter)
        {
            if (!CanCloseWindowCommandExecute(parameter))
                return;

            foreach (var obj in Application.Current.Windows)
            {
                if (obj is IndexFileWindow window)
                {
                    window.Close();
                    break;
                }
            }
        }

        public ICommand DeactivateWindowCommand { get; }
        public bool CanDeactivateWindowCommandExecute(object parameter)
        {
            return _mainWindowVM != null;
        }
        public void OnDeactivateWindowCommandExecuted(object parameter)
        {
            if (!CanDeactivateWindowCommandExecute(parameter))
                return;

            _mainWindowVM.IndexFileWindow = null;
        }

        #endregion

        #region Commands

        public ICommand ResetFileReadedParamCommand { get; }
        public bool CanResetFileReadedParamCommandExecute(object parameter)
        {
            return true;
        }
        public void OnResetFileReadedParamCommandExecuted(object parameter)
        {
            if (!CanResetFileReadedParamCommandExecute(parameter))
                return;

            IsFileReaded = false;
        }

        #endregion

        public IndexFileWindowViewModel(MainWindowViewModel mainWindowVM, string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException(filePath);

            _filePath = filePath;
            _mainWindowVM = mainWindowVM ?? throw new ArgumentNullException(nameof(mainWindowVM));

            _projectFolderPath = CreateProjectFolder(filePath);

            FileColumns = null;
            SelectedFileColumns = new ObservableCollection<FileColumn>();

            FileColumn.ChangeSelectedColumnsAction = ChangeSelectedColumns;

            _selectedDelimiter = Delimiters[0];

            _pageToggler = new PageToggler(GetPageVisibilitySetters());
            _pageToggler.SetPagesHideActions(GetPageHideActions());
            _pageToggler.SetPagesShowActions(GetPageShowActions());

            SetBackPageCommand = new RelayCommand(OnSetBackPageCommandExecuted, CanSetBackPageCommandExecute);
            SetNextPageCommand = new RelayCommand(OnSetNextPageCommandExecuted, CanSetNextPageCommandExecute);
            FinishFileIndexingCommand = new RelayCommand(OnFinishFileIndexingCommandExecuted, CanFinishFileIndexingCommandExecute);

            CheckAllColumnsCommand = new RelayCommand(OnCheckAllColumnsCommandExecuted, CanCheckAllColumnsCommandExecute);
            UncheckAllColumnsCommand = new RelayCommand(OnUncheckAllColumnsCommandExecuted, CanUncheckAllColumnsCommandExecute);
            FileColumnsListSelectionChangedCommand = new RelayCommand(OnFileColumnsListSelectionChangedCommandExecute, CanFileColumnsListSelectionChangedCommandExecute);

            CloseWindowCommand = new RelayCommand(OnCloseWindowCommandExecuted, CanCloseWindowCommandExecute);
            DeactivateWindowCommand = new RelayCommand(OnDeactivateWindowCommandExecuted, CanDeactivateWindowCommandExecute);

            ResetFileReadedParamCommand = new RelayCommand(OnResetFileReadedParamCommandExecuted, CanResetFileReadedParamCommandExecute);
        }

        #region CsvMethods

        private string CreateProjectFolder(string filePath)
        {
            string projectName = Path.GetFileNameWithoutExtension(filePath);
            string errorMsg = "Failed to create project folder. Try opening the file again.";

            return !AppDirectories.TryCreateProjectFolder(projectName, out string projectFolderPath, out ProjectFolderNotCreatedException ex)
                ? throw new ProjectFolderNotCreatedException(errorMsg, ex.FolderPath, ex.InnerException)
                : projectFolderPath;
        }

        private bool TryReadCsvFile(string path)
        {
            string[] fields = null;

            try
            {
                var csvReader = new CSVReadService();
                fields = csvReader.GetFields(path, SelectedDelimiter.Source);
            }
            catch { return false; }

            if (fields == null || fields.Length == 0)
            {
                FileColumns = null;
                return true;
            }

            var columns = fields.Select(t => new FileColumn(t));
            FileColumns = new ObservableCollection<FileColumn>(columns);

            OnPropertyChanged(nameof(FileColumns));
            return true;
        }

        #endregion

        #region PageDataMethods

        private void ChangeSelectedColumns(FileColumn column, bool addParam)
        {
            if (column == null)
                return;

            if (addParam)
            {
                SelectedFileColumns.Add(column);
            }
            else
            {
                if (SelectedFileColumns.Contains(column))
                    SelectedFileColumns.Remove(column);
            }
        }

        private bool IsPageDataSelected(int pageIndex)
        {
            switch (pageIndex)
            {
                case 0:
                    return SelectedDelimiter != null;

                case 1:
                    return SelectedFileColumns != null && SelectedFileColumns.Count > 0;

                case 2:
                    return SelectedDateColumn != null && SelectedSenderColumn != null && SelectedTextColumn != null;

                case 3:
                    return IsFileProcessed;

                default:
                    return true;
            }
        }

        #endregion

        #region PageVisibilityMethods

        private Action<Visibility>[] GetPageVisibilitySetters()
        {
            return new Action<Visibility>[]
            {
                t => SelectDelimiterPageVisibility = t,
                t => SelectColumnsPageVisibility = t,
                t => SelectSpecifiedKeysPageVisibility = t,
                t => WaitPageVisibility = t,
                t => FinishPageVisibility = t
            };
        }

        private Action[] GetPageHideActions()
        {
            return new Action[]
            {
                HideSelectDelimiterPageAction
            };
        }

        private Action[] GetPageShowActions()
        {
            return new Action[]
            {
                null,
                null,
                null,
                ShowWaitPageAction
            };
        }

        private void HideSelectDelimiterPageAction()
        {
            while (!IsFileReaded)
            {
                IsFileReaded = TryReadCsvFile(_filePath);

                if (!IsFileReaded)
                {
                    var msgResult = new QuickMessage().ShowWarning(MessageBoxButton.YesNo);

                    if (msgResult == MessageBoxResult.No)
                    {
                        CloseWindowCommand?.Execute(EventArgs.Empty);
                        return;
                    }

                    string previousPath = _filePath;

                    if (!DialogProvider.GetCsvFilePath(out _filePath))
                        _filePath = previousPath;
                }
            }
        }

        private void ShowWaitPageAction()
        {
            ProjectInfo.CreateNewProject(_projectFolderPath, SelectedDateColumn.Header, SelectedSenderColumn.Header,
                SelectedTextColumn.Header, SelectedFileColumns.Select(t => t.Header).ToList());

            LuceneService.OpenNewIndex();

            var reader = new CSVReadService();
            reader.GetLineCount(_filePath, false); //i have no fucking clue what this does

            var result = IndexHelper.PopulateIndex(_filePath, FileColumns.Select(t => t.Header).ToArray(), false);

            LuceneService.OpenReader();

            if (result != 1)
            {
                //error
                return;
            }

            var list = IndexHelper.LoadNDocumentsFromIndex(2000);
            MessageContainer.Messages = list;

            IsFileProcessed = true;

            SetNextPageCommand?.Execute(null);
        }

        #endregion
    }
}
