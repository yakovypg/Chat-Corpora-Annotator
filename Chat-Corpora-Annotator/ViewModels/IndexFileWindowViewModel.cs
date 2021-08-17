using ChatCorporaAnnotator.Infrastructure.Commands;
using ChatCorporaAnnotator.Models.Windows;
using ChatCorporaAnnotator.ViewModels.Base;
using ChatCorporaAnnotator.Views.Windows;
using System;
using System.Windows;
using System.Windows.Input;

namespace ChatCorporaAnnotator.ViewModels
{
    internal class IndexFileWindowViewModel : ViewModel
    {
        private readonly string _filePath;
        private readonly IPageToggler _pageToggler;
        private readonly MainWindowViewModel _mainWindowVM;

        private string _title = $"Step 1 of {PagesCount}";
        public string Title
        {
            get => _title;
            set => SetValue(ref _title, value);
        }

        #region Data

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
            return CurrentPageIndex >= 0 && CurrentPageIndex <= MaxToggleablePageIndex;
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

        public IndexFileWindowViewModel(MainWindowViewModel mainWindowVM, string filePath)
        {
            if (!System.IO.File.Exists(filePath))
                throw new System.IO.FileNotFoundException(filePath);

            _filePath = filePath;
            _mainWindowVM = mainWindowVM ?? throw new ArgumentNullException(nameof(mainWindowVM));

            _pageToggler = new PageToggler(new Action<Visibility>[]
            {
                t => SelectDelimiterPageVisibility = t,
                t => SelectColumnsPageVisibility = t,
                t => SelectSpecifiedKeysPageVisibility = t,
                t => WaitPageVisibility = t,
                t => FinishPageVisibility = t
            });

            SetBackPageCommand = new RelayCommand(OnSetBackPageCommandExecuted, CanSetBackPageCommandExecute);
            SetNextPageCommand = new RelayCommand(OnSetNextPageCommandExecuted, CanSetNextPageCommandExecute);
            FinishFileIndexingCommand = new RelayCommand(OnFinishFileIndexingCommandExecuted, CanFinishFileIndexingCommandExecute);

            CloseWindowCommand = new RelayCommand(OnCloseWindowCommandExecuted, CanCloseWindowCommandExecute);
            DeactivateWindowCommand = new RelayCommand(OnDeactivateWindowCommandExecuted, CanDeactivateWindowCommandExecute);
        }
    }
}
