using ChatCorporaAnnotator.Data;
using ChatCorporaAnnotator.Infrastructure.Commands;
using ChatCorporaAnnotator.Models.Messages;
using ChatCorporaAnnotator.ViewModels.Base;
using ChatCorporaAnnotator.Views.Windows;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace ChatCorporaAnnotator.ViewModels
{
    internal class MainWindowViewModel : ViewModel
    {
        public ChatViewModel ChatVM { get; }

        public IndexFileWindow IndexFileWindow { get; set; }

        public ObservableCollection<object> CurrentTags { get; private set; }
        public ObservableCollection<object> CurrentUserToggles { get; private set; }

        private object _selectedTagset;
        public object SelectedTagset
        {
            get => _selectedTagset;
            set => SetValue(ref _selectedTagset, value);
        }

        private bool _isFileLoaded = false;
        public bool IsFileLoaded
        {
            get => _isFileLoaded;
            set
            {
                if (!SetValue(ref _isFileLoaded, value))
                    return;

                TabControlGridsVisibility = Visibility.Visible;
            }
        }

        #region FinderItems

        private string _finderQuery = string.Empty;
        public string FinderQuery
        {
            get => _finderQuery;
            set => SetValue(ref _finderQuery, value);
        }

        private bool _isFinderStartDateChecked = false;
        public bool IsFinderStartDateChecked
        {
            get => _isFinderStartDateChecked;
            set => SetValue(ref _isFinderStartDateChecked, value);
        }

        private bool _isFinderEndDateChecked = false;
        public bool IsFinderEndDateChecked
        {
            get => _isFinderEndDateChecked;
            set => SetValue(ref _isFinderEndDateChecked, value);
        }

        private DateTime _finderStartDate = DateTime.Today;
        public DateTime FinderStartDate
        {
            get => _finderStartDate;
            set => SetValue(ref _finderStartDate, value);
        }

        private DateTime _finderEndDate = DateTime.Today;
        public DateTime FinderEndDate
        {
            get => _finderEndDate;
            set => SetValue(ref _finderEndDate, value);
        }

        #endregion

        #region TabControlItems

        private Visibility _tabControlGridsVisibility = Visibility.Hidden;
        public Visibility TabControlGridsVisibility
        {
            get => _tabControlGridsVisibility;
            set => SetValue(ref _tabControlGridsVisibility, value);
        }

        #endregion

        #region FieldsVisibility

        private Visibility _currentTagsetVisibility = Visibility.Hidden;
        public Visibility CurrentTagsetVisibility
        {
            get => _currentTagsetVisibility;
            set => SetValue(ref _currentTagsetVisibility, value);
        }

        #endregion

        #region BottomBarItems

        private string _loadedFileInfo = "Not loaded";
        public string LoadedFileInfo
        {
            get => _loadedFileInfo;
            set => SetValue(ref _loadedFileInfo, value);
        }

        private int _messagesCount = 0;
        public int MessagesCount
        {
            get => _messagesCount;
            set
            {
                if (!SetValue(ref _messagesCount, value) || !IsFileLoaded)
                    return;

                LoadedFileInfo = $"{_messagesCount} messages";
            }
        }

        private int _tagsetCount = 0;
        public int TagsetCount
        {
            get => _tagsetCount;
            set => SetValue(ref _tagsetCount, value);
        }

        private int _situationsCount = 0;
        public int SituationsCount
        {
            get => _situationsCount;
            set => SetValue(ref _situationsCount, value);
        }

        private Visibility _bottomMenuVisibility = Visibility.Hidden;
        public Visibility BottomMenuVisibility
        {
            get => _bottomMenuVisibility;
            set => SetValue(ref _bottomMenuVisibility, value);
        }

        #endregion

        #region BottomBarCommands

        #region FilterCommands

        public ICommand ChooseTagForFilterCommand { get; }
        public bool CanChooseTagForFilterCommandExecute(object parameter)
        {
            return false;
        }
        public void OnChooseTagForFilterCommandExecuted(object parameter)
        {
            if (!CanChooseTagForFilterCommandExecute(parameter))
                return;
        }

        public ICommand SetTaggedOnlyParamForFilterCommand { get; }
        public bool CanSetTaggedOnlyParamForFilterCommandExecute(object parameter)
        {
            return false;
        }
        public void OnSetTaggedOnlyParamForFilterCommandExecuted(object parameter)
        {
            if (!CanSetTaggedOnlyParamForFilterCommandExecute(parameter))
                return;
        }

        #endregion

        #region SaveFileCommands

        public ICommand SaveFileCommand { get; }
        public bool CanSaveFileCommandExecute(object parameter)
        {
            return false;
        }
        public void OnSaveFileCommandExecuted(object parameter)
        {
            if (!CanSaveFileCommandExecute(parameter))
                return;
        }

        public ICommand WriteFileToDiskCommand { get; }
        public bool CanWriteFileToDiskCommandExecute(object parameter)
        {
            return false;
        }
        public void OnWriteFileToDiskCommandExecuted(object parameter)
        {
            if (!CanWriteFileToDiskCommandExecute(parameter))
                return;
        }

        #endregion

        #region SuggesterCommands

        public ICommand ShowSuggesterCommand { get; }
        public bool CanShowSuggesterCommandExecute(object parameter)
        {
            return false;
        }
        public void OnShowSuggesterCommandExecuted(object parameter)
        {
            if (!CanShowSuggesterCommandExecute(parameter))
                return;
        }

        #endregion

        #region TagsetEditorCommands

        public ICommand ShowTagsetEditorCommand { get; }
        public bool CanShowTagsetEditorCommandExecute(object parameter)
        {
            return false;
        }
        public void OnShowTagsetEditorCommandExecuted(object parameter)
        {
            if (!CanShowTagsetEditorCommandExecute(parameter))
                return;
        }

        #endregion

        #region EditSituationsCommands

        public ICommand MergeSituationsCommand { get; }
        public bool CanMergeSituationsCommandExecute(object parameter)
        {
            return false;
        }
        public void OnMergeSituationsCommandExecuted(object parameter)
        {
            if (!CanMergeSituationsCommandExecute(parameter))
                return;
        }

        public ICommand DeleteSituationCommand { get; }
        public bool CanDeleteSituationCommandExecute(object parameter)
        {
            return false;
        }
        public void OnDeleteSituationCommandExecuted(object parameter)
        {
            if (!CanDeleteSituationCommandExecute(parameter))
                return;
        }

        public ICommand ChangeSituationTagCommand { get; }
        public bool CanChangeSituationTagCommandExecute(object parameter)
        {
            return false;
        }
        public void OnChangeSituationTagCommandExecuted(object parameter)
        {
            if (!CanChangeSituationTagCommandExecute(parameter))
                return;
        }

        #endregion

        #endregion

        #region TopBarCommands

        public ICommand IndexNewFileCommand { get; }
        public bool CanIndexNewFileCommandExecute(object parameter)
        {
            return true;
        }
        public void OnIndexNewFileCommandExecuted(object parameter)
        {
            if (!CanIndexNewFileCommandExecute(parameter))
                return;

            if (IndexFileWindow == null)
            {
                if (!DialogProvider.GetCsvFilePath(out string path))
                    return;

                IndexFileWindowViewModel indexFileWindowVM;

                try
                {
                    indexFileWindowVM = new IndexFileWindowViewModel(this, path);
                }
                catch (Exception ex)
                {
                    new QuickMessage($"Failed to upload the file.\nComment: {ex.Message}").ShowError();
                    return;
                }

                IndexFileWindow = new IndexFileWindow(indexFileWindowVM);
                IndexFileWindow.Show();

                return;
            }

            if (IndexFileWindow.WindowState == WindowState.Minimized)
                IndexFileWindow.WindowState = WindowState.Normal;

            IndexFileWindow.Activate();
            IndexFileWindow.Topmost = true;
            IndexFileWindow.Topmost = false;
            IndexFileWindow.Focus();
        }

        public ICommand OpenCorpusCommand { get; }
        public bool CanOpenCorpusCommandExecute(object parameter)
        {
            return false;
        }
        public void OnOpenCorpusCommandExecuted(object parameter)
        {
            if (!CanOpenCorpusCommandExecute(parameter))
                return;
        }

        public ICommand ShowPlotCommand { get; }
        public bool CanShowPlotCommandExecute(object parameter)
        {
            return false;
        }
        public void OnShowPlotCommandExecuted(object parameter)
        {
            if (!CanShowPlotCommandExecute(parameter))
                return;
        }

        public ICommand ShowHeatmapCommand { get; }
        public bool CanShowHeatmapCommandExecute(object parameter)
        {
            return false;
        }
        public void OnShowHeatmapCommandExecuted(object parameter)
        {
            if (!CanShowHeatmapCommandExecute(parameter))
                return;
        }

        public ICommand ExtractFileCommand { get; }
        public bool CanExtractFileCommandExecute(object parameter)
        {
            return false;
        }
        public void OnExtractFileCommandExecuted(object parameter)
        {
            if (!CanExtractFileCommandExecute(parameter))
                return;
        }

        #endregion

        #region Commands

        public ICommand CloseIndexFileWindowCommand { get; }
        public bool CanCloseIndexFileWindowCommandExecute(object parameter)
        {
            return IndexFileWindow != null;
        }
        public void OnCloseIndexFileWindowCommandExecuted(object parameter)
        {
            if (!CanCloseIndexFileWindowCommandExecute(parameter))
                return;

            IndexFileWindow.Close();
        }

        #endregion

        public MainWindowViewModel()
        {
            ChatVM = new ChatViewModel(this);

            CurrentTags = new ObservableCollection<object>();
            CurrentUserToggles = new ObservableCollection<object>();

            IndexNewFileCommand = new RelayCommand(OnIndexNewFileCommandExecuted, CanIndexNewFileCommandExecute);
            OpenCorpusCommand = new RelayCommand(OnOpenCorpusCommandExecuted, CanOpenCorpusCommandExecute);

            ShowPlotCommand = new RelayCommand(OnShowPlotCommandExecuted, CanShowPlotCommandExecute);
            ShowHeatmapCommand = new RelayCommand(OnShowHeatmapCommandExecuted, CanShowHeatmapCommandExecute);
            ExtractFileCommand = new RelayCommand(OnExtractFileCommandExecuted, CanExtractFileCommandExecute);

            ChooseTagForFilterCommand = new RelayCommand(OnChooseTagForFilterCommandExecuted, CanChooseTagForFilterCommandExecute);
            SetTaggedOnlyParamForFilterCommand = new RelayCommand(OnSetTaggedOnlyParamForFilterCommandExecuted, CanSetTaggedOnlyParamForFilterCommandExecute);
            WriteFileToDiskCommand = new RelayCommand(OnWriteFileToDiskCommandExecuted, CanWriteFileToDiskCommandExecute);
            SaveFileCommand = new RelayCommand(OnSaveFileCommandExecuted, CanSaveFileCommandExecute);
            ShowSuggesterCommand = new RelayCommand(OnShowSuggesterCommandExecuted, CanShowSuggesterCommandExecute);
            ShowTagsetEditorCommand = new RelayCommand(OnShowTagsetEditorCommandExecuted, CanShowTagsetEditorCommandExecute);
            MergeSituationsCommand = new RelayCommand(OnMergeSituationsCommandExecuted, CanMergeSituationsCommandExecute);
            DeleteSituationCommand = new RelayCommand(OnDeleteSituationCommandExecuted, CanDeleteSituationCommandExecute);
            ChangeSituationTagCommand = new RelayCommand(OnChangeSituationTagCommandExecuted, CanChangeSituationTagCommandExecute);

            CloseIndexFileWindowCommand = new RelayCommand(OnCloseIndexFileWindowCommandExecuted, CanCloseIndexFileWindowCommandExecute);
        }
    }
}
