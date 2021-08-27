using ChatCorporaAnnotator.Data.Indexing;
using ChatCorporaAnnotator.Data.Windows;
using ChatCorporaAnnotator.Infrastructure.Commands;
using ChatCorporaAnnotator.Infrastructure.Enums;
using ChatCorporaAnnotator.Infrastructure.Exceptions.Indexing;
using ChatCorporaAnnotator.Infrastructure.Extensions;
using ChatCorporaAnnotator.Models.Indexing;
using ChatCorporaAnnotator.Models.Messages;
using ChatCorporaAnnotator.Models.Windows;
using ChatCorporaAnnotator.Services;
using ChatCorporaAnnotator.ViewModels.Base;
using ChatCorporaAnnotator.Views.Windows;
using IndexEngine;
using IndexEngine.Paths;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace ChatCorporaAnnotator.ViewModels
{
    internal class IndexFileWindowViewModel : ViewModel
    {
        private const bool HEADER_PARAM = false;
        private const int WAIT_PAGE_TIMER_TICK_INTERVAL = 3000;

        private readonly IProject _project;
        private IPageSwitcher _pageSwitcher;
        private DispatcherTimer _waitPageTimer;

        private bool _isFileReaded = false;
        private FileProcessingResult _fileProcessingResult = FileProcessingResult.InProcess;

        public Action FinishAction { get; set; }
        public Action DeactivateAction { get; set; }

        public ObservableCollection<FileColumn> FileColumns { get; private set; }
        public ObservableCollection<FileColumn> SelectedFileColumns { get; private set; }

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

        private static readonly int MaxSwitchablePageIndex = 2;
        private static readonly int PagesCount = PageHints.Length;
        private static readonly string FailedToLoadDataHint = "Failed to load data";

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

        private string _title = $"Step 1 of {PagesCount}";
        public string Title
        {
            get => _title;
            set => SetValue(ref _title, value);
        }

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

        #region ButtonsEnables

        private bool _finishButtonEnabled = false;
        public bool FinishButtonEnabled
        {
            get => _finishButtonEnabled;
            set => SetValue(ref _finishButtonEnabled, value);
        }

        #endregion

        #region ImagesVisibilities

        private Visibility _successfulFinishImageVisibility = Visibility.Visible;
        public Visibility SuccessfulFinishImageVisibility
        {
            get => _successfulFinishImageVisibility;
            set => SetValue(ref _successfulFinishImageVisibility, value);
        }

        private Visibility _failedFinishImageVisibility = Visibility.Hidden;
        public Visibility FailedFinishImageVisibility
        {
            get => _failedFinishImageVisibility;
            set => SetValue(ref _failedFinishImageVisibility, value);
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
            return CurrentPageIndex > 0 && CurrentPageIndex <= MaxSwitchablePageIndex;
        }
        public void OnSetBackPageCommandExecuted(object parameter)
        {
            if (!CanSetBackPageCommandExecute(parameter))
                return;

            CurrentPageIndex = _pageSwitcher.BackPage();
        }

        public ICommand SetNextPageCommand { get; }
        public bool CanSetNextPageCommandExecute(object parameter)
        {
            return CurrentPageIndex >= 0 &&
                   CurrentPageIndex <= MaxSwitchablePageIndex &&
                   IsPageDataSelected(CurrentPageIndex);
        }
        public void OnSetNextPageCommandExecuted(object parameter)
        {
            if (!CanSetNextPageCommandExecute(parameter))
                return;

            CurrentPageIndex = _pageSwitcher.NextPage();
        }

        public ICommand FinishFileIndexingCommand { get; }
        public bool CanFinishFileIndexingCommandExecute(object parameter)
        {
            return true;
        }
        public void OnFinishFileIndexingCommandExecuted(object parameter)
        {
            if (!CanFinishFileIndexingCommandExecute(parameter))
                return;

            if (_fileProcessingResult == FileProcessingResult.Success)
                FinishAction?.Invoke();

            CloseWindowCommand?.Execute(parameter);
        }

        #endregion

        #region SelectedItemsCommands

        public ICommand CheckAllColumnsCommand { get; }
        public bool CanCheckAllColumnsCommandExecute(object parameter)
        {
            return !FileColumns.IsNullOrEmpty();
        }
        public void OnCheckAllColumnsCommandExecuted(object parameter)
        {
            if (!CanCheckAllColumnsCommandExecute(parameter))
                return;

            var selectedItemsOrganizer = new SelectedItemsOrganizer();
            selectedItemsOrganizer.SelectAll(FileColumns);
        }

        public ICommand UncheckAllColumnsCommand { get; }
        public bool CanUncheckAllColumnsCommandExecute(object parameter)
        {
            return !FileColumns.IsNullOrEmpty();
        }
        public void OnUncheckAllColumnsCommandExecuted(object parameter)
        {
            if (!CanUncheckAllColumnsCommandExecute(parameter))
                return;

            var selectedItemsOrganizer = new SelectedItemsOrganizer();
            selectedItemsOrganizer.DeselectAll(FileColumns);
        }

        public ICommand ChangeSelectedColumnsCommand { get; }
        public bool CanChangeSelectedColumnsCommandExecute(object parameter)
        {
            return parameter is SelectionChangedEventArgs;
        }
        public void OnChangeSelectedColumnsCommandExecute(object parameter)
        {
            if (!CanChangeSelectedColumnsCommandExecute(parameter))
                return;

            var eventArgs = parameter as SelectionChangedEventArgs;
            var selectedItemOrganizer = new SelectedItemsOrganizer();

            selectedItemOrganizer.SelectAddedItems<FileColumn>(eventArgs);
        }

        #endregion

        #region SystemCommands

        public ICommand CloseWindowCommand { get; }
        public bool CanCloseWindowCommandExecute(object parameter)
        {
            return true;
        }
        public void OnCloseWindowCommandExecuted(object parameter)
        {
            if (!CanCloseWindowCommandExecute(parameter))
                return;

            if (new WindowFinder().Find(typeof(IndexFileWindow)) is IndexFileWindow indexFileWindow)
                indexFileWindow.Close();
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

            DeactivateAction?.Invoke();
        }

        #endregion

        #region FileParametersCommands

        public ICommand ResetFileReadedParamCommand { get; }
        public bool CanResetFileReadedParamCommandExecute(object parameter)
        {
            return true;
        }
        public void OnResetFileReadedParamCommandExecuted(object parameter)
        {
            if (!CanResetFileReadedParamCommandExecute(parameter))
                return;

            _isFileReaded = false;
        }

        #endregion

        public IndexFileWindowViewModel(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException(filePath);

            var project = new Project(filePath);

            if (ProjectInteraction.ProjectInfo != null)
                throw new OneProjectOnlyException("The project has already been uploaded.", project, null);

            _project = project;
            _project.Initialize();

            InitializeFields();

            SetBackPageCommand = new RelayCommand(OnSetBackPageCommandExecuted, CanSetBackPageCommandExecute);
            SetNextPageCommand = new RelayCommand(OnSetNextPageCommandExecuted, CanSetNextPageCommandExecute);
            FinishFileIndexingCommand = new RelayCommand(OnFinishFileIndexingCommandExecuted, CanFinishFileIndexingCommandExecute);

            CheckAllColumnsCommand = new RelayCommand(OnCheckAllColumnsCommandExecuted, CanCheckAllColumnsCommandExecute);
            UncheckAllColumnsCommand = new RelayCommand(OnUncheckAllColumnsCommandExecuted, CanUncheckAllColumnsCommandExecute);
            ChangeSelectedColumnsCommand = new RelayCommand(OnChangeSelectedColumnsCommandExecute, CanChangeSelectedColumnsCommandExecute);

            CloseWindowCommand = new RelayCommand(OnCloseWindowCommandExecuted, CanCloseWindowCommandExecute);
            DeactivateWindowCommand = new RelayCommand(OnDeactivateWindowCommandExecuted, CanDeactivateWindowCommandExecute);

            ResetFileReadedParamCommand = new RelayCommand(OnResetFileReadedParamCommandExecuted, CanResetFileReadedParamCommandExecute);
        }

        #region DataMethods

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

                default:
                    return true;
            }
        }

        private void InitializeFields()
        {
            _selectedDelimiter = Delimiters[0];
            SelectedFileColumns = new ObservableCollection<FileColumn>();

            FileColumn.AcceptRepeatedColumns = false;
            FileColumn.SelectedColumns = SelectedFileColumns;

            _waitPageTimer = new DispatcherTimer(DispatcherPriority.Background)
            {
                Interval = new TimeSpan(0, 0, 0, 0, WAIT_PAGE_TIMER_TICK_INTERVAL)
            };

            _waitPageTimer.Tick += WaitPageTimerTick;

            _pageSwitcher = new PageSwitcher(new PageExplorerStep[]
            {
                new PageExplorerStep("SelectDelimiterPage")
                {
                    Hide = LoadFileColumns,
                    SetVisibility = t => SelectDelimiterPageVisibility = t
                },
                new PageExplorerStep("SelectColumnsPageVisibility")
                {
                    Hide = SetFieldsToProject,
                    SetVisibility = t => SelectColumnsPageVisibility = t
                },
                new PageExplorerStep("SelectSpecifiedKeysPageVisibility")
                {
                    SetVisibility = t => SelectSpecifiedKeysPageVisibility = t
                },
                new PageExplorerStep("WaitPageVisibility")
                {
                    Show = StartIndexFileTask,
                    SetVisibility = t => WaitPageVisibility = t
                },
                new PageExplorerStep("FinishPageVisibility")
                {
                    SetVisibility = t => FinishPageVisibility = t
                }
            });
        }

        #endregion

        #region TimerMethods

        private void WaitPageTimerTick(object sender, EventArgs e)
        {
            if (_fileProcessingResult == FileProcessingResult.InProcess)
                return;

            FinishButtonEnabled = true;
            CurrentPageIndex = _pageSwitcher.NextPage();

            if (_fileProcessingResult == FileProcessingResult.Fail)
            {
                SuccessfulFinishImageVisibility = Visibility.Hidden;
                FailedFinishImageVisibility = Visibility.Visible;
                CurrentPageHint = FailedToLoadDataHint;
            }

            _waitPageTimer.Stop();
        }

        #endregion

        #region CsvFileMethods

        private CancelEventArgs SetFieldsToProject()
        {
            ProjectInfo.Data.SelectedFields = SelectedFileColumns.Select(t => t.Header).ToList();
            return new CancelEventArgs(false);
        }

        private CancelEventArgs LoadFileColumns()
        {
            while (!_isFileReaded)
            {
                var columnReader = new CsvColumnReadService();

                string filePath = _project.FilePath;
                string delimiter = SelectedDelimiter.Source;

                _isFileReaded = columnReader.TryGetColumns(filePath, delimiter, out FileColumn[] columns);

                if (_isFileReaded)
                {
                    FileColumns = new ObservableCollection<FileColumn>(columns);
                    OnPropertyChanged(nameof(FileColumns));

                    return new CancelEventArgs(false);
                }

                var msgResult = new QuickMessage("Failed to parse the file. Try again?").ShowWarning(MessageBoxButton.YesNo);

                if (msgResult == MessageBoxResult.No)
                    return new CancelEventArgs(true);
            }

            return new CancelEventArgs(false);
        }

        private EventArgs StartIndexFileTask()
        {
            Task.Run(delegate
            {
                IndexFile();
            });

            _waitPageTimer.Start();
            ProjectInteraction.ProjectInfo = _project.GetInfo();

            return EventArgs.Empty;
        }

        private void IndexFile()
        {
            string filePath = _project.FilePath;
            string projectPath = _project.WorkingDirectory;

            string dateColumn = SelectedDateColumn.Header;
            string senderColumn = SelectedSenderColumn.Header;
            string textColumn = SelectedTextColumn.Header;

            var columns = FileColumns.Select(t => t.Header).ToArray();
            var selectedColumns = SelectedFileColumns.Select(t => t.Header).ToList();

            FileProcessingResult fileProcessingResult;

            try
            {
                ProjectInfo.CreateNewProject(projectPath, dateColumn, senderColumn, textColumn, selectedColumns);
                LuceneService.OpenNewIndex();

                //Counting the number of lines is necessary here, but it is not clear why and how it works
                var reader = new CsvReadService();
                reader.GetLineCount(_project.FilePath, HEADER_PARAM);

                int result = IndexHelper.PopulateIndex(filePath, columns, HEADER_PARAM);
                LuceneService.OpenReader();

                fileProcessingResult = result == 1
                    ? FileProcessingResult.Success
                    : FileProcessingResult.Fail;
            }
            catch
            {
                fileProcessingResult = FileProcessingResult.Fail;
            }

            _fileProcessingResult = fileProcessingResult;
        }

        #endregion
    }
}
