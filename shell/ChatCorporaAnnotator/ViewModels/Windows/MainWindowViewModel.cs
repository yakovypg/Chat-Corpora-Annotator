﻿using ChatCorporaAnnotator.Controlling.Timers;
using ChatCorporaAnnotator.Data.Dialogs;
using ChatCorporaAnnotator.Data.Indexing;
using ChatCorporaAnnotator.Data.Windows;
using ChatCorporaAnnotator.Data.Windows.UI;
using ChatCorporaAnnotator.Infrastructure.AppEventArgs;
using ChatCorporaAnnotator.Infrastructure.Commands;
using ChatCorporaAnnotator.Infrastructure.Enums;
using ChatCorporaAnnotator.Infrastructure.Exceptions.Indexing;
using ChatCorporaAnnotator.Infrastructure.Extensions;
using ChatCorporaAnnotator.Models.History;
using ChatCorporaAnnotator.Models.Indexing;
using ChatCorporaAnnotator.Models.Messages;
using ChatCorporaAnnotator.Models.Serialization;
using ChatCorporaAnnotator.Services.Csv;
using ChatCorporaAnnotator.Services.Xml;
using ChatCorporaAnnotator.ViewModels.Analyzers;
using ChatCorporaAnnotator.ViewModels.Base;
using ChatCorporaAnnotator.ViewModels.Chat;
using ChatCorporaAnnotator.Views.Windows;
using IndexEngine.Data.Paths;
using IndexEngine.Indexes;
using IndexEngine.Search;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ChatCorporaAnnotator.ViewModels.Windows
{
    internal class MainWindowViewModel : ViewModel
    {
        private IndexFileWindow? _indexFileWindow;
        private ExtractorWindow? _extractorWindow;
        private TagsetEditorWindow? _tagsetEditorWindow;
        private SuggesterWindow? _suggesterWindow;

        private CsvExportService _csvExportService;

        public SavingTimer ProjectStateSavingTimer { get; }
        public MemoryCleaninigTimer MemoryCleaninigTimer { get; }

        public ChatViewModel ChatVM { get; }
        public StatisticsViewModel StatisticsVM { get; }
        public ConcordanceViewModel ConcordanceVM { get; }
        public NGramsViewModel NGramsVM { get; }

        public RecentProjectProvider RecentProjectProvider { get; }

        #region StaticData

        private static readonly string[] ProjectStateHeaders = new string[]
        {
            "saving changes",
            "all changes saved",
            "changes not saved"
        };

        #endregion

        #region States

        public bool IsIndexFileWindowOpen => _indexFileWindow != null;
        public bool IsTagsetEditorWindowOpen => _tagsetEditorWindow != null;

        public bool IsCsvFileExportingNotActive => !IsCsvFileExportingActive;

        private bool _isCsvFileExportingActive = false;
        public bool IsCsvFileExportingActive
        {
            get => _isCsvFileExportingActive;
            private set
            {
                SetValue(ref _isCsvFileExportingActive, value);
                OnPropertyChanged(nameof(IsCsvFileExportingNotActive));
            }
        }

        private bool _isFileLoaded = false;
        public bool IsFileLoaded
        {
            get => _isFileLoaded;
            private set
            {
                if (!SetValue(ref _isFileLoaded, value))
                    return;

                if (!value)
                    LoadedFileInfo = "Not loaded";
                else
                    MessagesCount = ProjectInfo.Data.LineCount;
            }
        }

        private bool _isProjectChanged = false;
        public bool IsProjectChanged
        {
            get => _isProjectChanged;
            set
            {
                if (!IsFileLoaded)
                    return;

                SetValue(ref _isProjectChanged, value);

                ProjectStateSavingTimer.SavingState = value
                        ? SaveProjectState.ChangesNotSaved
                        : SaveProjectState.ChangesSaved;
            }
        }

        private FileLoadState _projectFileLoadState = FileLoadState.NotLoaded;
        public FileLoadState ProjectFileLoadState
        {
            get => _projectFileLoadState;
            set
            {
                if (!SetValue(ref _projectFileLoadState, value))
                    return;

                TabControlGridsVisibility = _projectFileLoadState == FileLoadState.Loaded
                    ? Visibility.Visible
                    : Visibility.Hidden;

                BottomMenuVisibility = TabControlGridsVisibility;
                CurrentTagsetVisibility = TabControlGridsVisibility;

                IsFileLoaded = value == FileLoadState.Loaded;
            }
        }

        #endregion

        #region ItemsVisibilities

        private Visibility _currentTagsetVisibility = Visibility.Hidden;
        public Visibility CurrentTagsetVisibility
        {
            get => _currentTagsetVisibility;
            set => SetValue(ref _currentTagsetVisibility, value);
        }

        private Visibility _bottomMenuVisibility = Visibility.Hidden;
        public Visibility BottomMenuVisibility
        {
            get => _bottomMenuVisibility;
            set => SetValue(ref _bottomMenuVisibility, value);
        }

        private Visibility _tabControlGridsVisibility = Visibility.Hidden;
        public Visibility TabControlGridsVisibility
        {
            get => _tabControlGridsVisibility;
            set => SetValue(ref _tabControlGridsVisibility, value);
        }

        #endregion

        #region SaveProjectStatePresenterItems

        private string _saveProjectStateHeader = ProjectStateHeaders[(int)SaveProjectState.ChangesSaved];
        public string SaveProjectStateHeader
        {
            get => _saveProjectStateHeader;
            private set => SetValue(ref _saveProjectStateHeader, value);
        }

        private Visibility _changesSavingInPrecessImageVisibility = Visibility.Hidden;
        public Visibility ChangesSavingInProcessImageVisibility
        {
            get => _changesSavingInPrecessImageVisibility;
            private set
            {
                if (value == Visibility.Visible)
                {
                    ChangesSavedImageVisibility = Visibility.Hidden;
                    ChangesNotSavedImageVisibility = Visibility.Hidden;
                }

                if (SetValue(ref _changesSavingInPrecessImageVisibility, value))
                    SaveProjectStateHeader = ProjectStateHeaders[(int)SaveProjectState.InProcess];
            }
        }

        private Visibility _changesSavedImageVisibility = Visibility.Visible;
        public Visibility ChangesSavedImageVisibility
        {
            get => _changesSavedImageVisibility;
            private set
            {
                if (value == Visibility.Visible)
                {
                    ChangesNotSavedImageVisibility = Visibility.Hidden;
                    ChangesSavingInProcessImageVisibility = Visibility.Hidden;
                }

                if (SetValue(ref _changesSavedImageVisibility, value))
                    SaveProjectStateHeader = ProjectStateHeaders[(int)SaveProjectState.ChangesSaved];
            }
        }

        private Visibility _changesNotSavedImageVisibility = Visibility.Hidden;
        public Visibility ChangesNotSavedImageVisibility
        {
            get => _changesNotSavedImageVisibility;
            private set
            {
                if (value == Visibility.Visible)
                {
                    ChangesSavedImageVisibility = Visibility.Hidden;
                    ChangesSavingInProcessImageVisibility = Visibility.Hidden;
                }

                if (SetValue(ref _changesNotSavedImageVisibility, value))
                    SaveProjectStateHeader = ProjectStateHeaders[(int)SaveProjectState.ChangesNotSaved];
            }
        }

        #endregion

        #region BottomBarItems

        private string _loadedFileInfo = "Not loaded";
        public string LoadedFileInfo
        {
            get => _loadedFileInfo;
            private set => SetValue(ref _loadedFileInfo, value);
        }

        private string _tagsetName = "No tagset";
        public string TagsetName
        {
            get => _tagsetName;
            set => SetValue(ref _tagsetName, value);
        }

        private int _messagesCount = 0;
        public int MessagesCount
        {
            get => _messagesCount;
            set
            {
                if (!IsFileLoaded || !SetValue(ref _messagesCount, value))
                    return;

                LoadedFileInfo = $"{_messagesCount} messages";
            }
        }

        private int _situationsCount = 0;
        public int SituationsCount
        {
            get => _situationsCount;
            set => SetValue(ref _situationsCount, value);
        }

        #endregion

        #region BottomBarCommands

        #region SaveFileCommands

        public ICommand SavePresentStateByButtonCommand { get; }
        public bool CanSavePresentStateByButtonCommandExecute(object parameter)
        {
            return IsFileLoaded;
        }
        public void OnSavePresentStateByButtonCommandExecuted(object parameter)
        {
            if (!CanSavePresentStateByButtonCommandExecute(parameter))
                return;

            ProjectStateSavingTimer.SaveNow();
        }

        public ICommand SavePresentStateCommand { get; }
        public bool CanSavePresentStateCommandExecute(object parameter)
        {
            return IsFileLoaded;
        }
        public void OnSavePresentStateCommandExecuted(object parameter)
        {
            if (!CanSavePresentStateCommandExecute(parameter))
                return;

            try
            {
                SituationIndex.GetInstance().FlushIndexToDisk();
            }
            catch (Exception ex)
            {
                new QuickMessage($"Error: {ex.Message}").ShowError();
            }
        }

        public ICommand ExportXmlCommand { get; }
        public bool CanExportXmlCommandExecute(object parameter)
        {
            return IsFileLoaded;
        }
        public void OnExportXmlCommandExecuted(object parameter)
        {
            if (!CanExportXmlCommandExecute(parameter))
                return;

            try
            {
                TagFileWriter writer = new TagFileWriter();
                writer.OpenWriter();

                foreach (var kvp in SituationIndex.GetInstance().IndexCollection)
                {
                    foreach (var pair in kvp.Value)
                        writer.WriteSituation(pair.Value, kvp.Key, pair.Key);
                }

                writer.CloseWriter();
            }
            catch (Exception ex)
            {
                new QuickMessage($"Error: {ex.Message}").ShowError();
            }
        }

        public ICommand ExportCsvCommand { get; }
        public bool CanExportCsvCommandExecute(object parameter)
        {
            return IsFileLoaded && !StatisticsVM.IsStatisticsCaulculatingActive;
        }
        public void OnExportCsvCommandExecuted(object parameter)
        {
            if (!CanExportCsvCommandExecute(parameter))
                return;

            _csvExportService = new CsvExportService()
            {
                SuccessfulExportingAction = () => IsCsvFileExportingActive = false,
                FailedExportingAction = t => new QuickMessage("Failed to export .csv file").ShowError()
            };

            if (_csvExportService.StartExporting(ProjectInfo.OutputCsvFilePath))
                IsCsvFileExportingActive = true;
        }

        #endregion

        #region ImportCommands

        public ICommand ImportXmlCommand { get; }
        public bool CanImportXmlCommandExecute(object parameter)
        {
            return IsFileLoaded && !StatisticsVM.IsStatisticsCaulculatingActive;
        }
        public void OnImportXmlCommandExecuted(object parameter)
        {
            if (!CanImportXmlCommandExecute(parameter))
                return;

            if (!DialogProvider.GetXmlFilePath(out string path))
                return;

            try
            {
                TagFileReader reader = new TagFileReader();
                reader.OpenReader(path);

                List<SituationData> situations = reader.ReadAllSituations();

                RecoverData(situations);
                SyncData(situations);

                ChatVM.RemoveAllTagsCommand.Execute(null);

                foreach (var sit in situations)
                {
                    ChatVM.AddTagCommand.Execute(sit);
                }

                int[] taggedMsgIds = SituationIndex.GetInstance().InvertedIndex.Keys.ToArray();

                ChatVM.SituationsVM.TaggedMessagesIds.Clear();
                ChatVM.SituationsVM.TaggedMessagesIds.AddRange(taggedMsgIds);
                ChatVM.SituationsVM.TaggedMessagesIds.Sort();

                ChatVM.SituationsVM.UpdateMessagesTags();

                reader.CloseReader();
            }
            catch (Exception ex)
            {
                new QuickMessage($"Error: {ex.Message}").ShowError();
            }
        }

        #endregion

        #region AuxiliaryComponentsCommands

        public ICommand ShowSuggesterCommand { get; }
        public bool CanShowSuggesterCommandExecute(object parameter)
        {
            return !StatisticsVM.IsStatisticsCaulculatingActive;
        }
        public void OnShowSuggesterCommandExecuted(object parameter)
        {
            if (!CanShowSuggesterCommandExecute(parameter))
                return;

            if (_suggesterWindow != null)
            {
                new WindowInteract(_suggesterWindow).MoveToForeground();
                return;
            }

            var vm = new SuggesterWindowViewModel(this)
            {
                DeactivateAction = () => _suggesterWindow = null
            };

            _suggesterWindow = new SuggesterWindow(vm);
            _suggesterWindow.Show();
        }

        public ICommand ShowTagsetEditorCommand { get; }
        public bool CanShowTagsetEditorCommandExecute(object parameter)
        {
            return !StatisticsVM.IsStatisticsCaulculatingActive;
        }
        public void OnShowTagsetEditorCommandExecuted(object parameter)
        {
            if (!CanShowTagsetEditorCommandExecute(parameter))
                return;

            if (_tagsetEditorWindow != null)
            {
                new WindowInteract(_tagsetEditorWindow).MoveToForeground();
                return;
            }

            var vm = new TagsetEditorWindowViewModel(this)
            {
                DeactivateAction = () => _tagsetEditorWindow = null
            };

            _tagsetEditorWindow = new TagsetEditorWindow(vm);
            _tagsetEditorWindow.Show();
        }

        #endregion

        #endregion

        #region TopBarCommands

        #region HistoryCommands

        public ICommand ClearRecentProjectsCommand { get; }
        public bool CanClearRecentProjectsCommandExecute(object parameter)
        {
            return true;
        }
        public void OnClearRecentProjectsCommandExecuted(object parameter)
        {
            if (!CanClearRecentProjectsCommandExecute(parameter))
                return;

            RecentProjectProvider.Clear();
        }

        #endregion

        #region IndexFileCommands

        public ICommand IndexNewFileCommand { get; }
        public bool CanIndexNewFileCommandExecute(object parameter)
        {
            return true;
        }
        public void OnIndexNewFileCommandExecuted(object parameter)
        {
            if (!CanIndexNewFileCommandExecute(parameter))
                return;

            if (_indexFileWindow != null)
            {
                new WindowInteract(_indexFileWindow).MoveToForeground();
                return;
            }

            if (!DialogProvider.GetCsvFilePath(out string path))
                return;

            IndexFileWindowViewModel indexFileWindowVM;

            try
            {
                indexFileWindowVM = new IndexFileWindowViewModel(this, path)
                {
                    FinishAction = t => OnFileLoaded(new ProjectInformation(t.Name, t.WorkingDirectory)),
                    DeactivateAction = () => _indexFileWindow = null
                };
            }
            catch (OneProjectOnlyException ex)
            {
                new QuickMessage(ex.Message).ShowInformation();
                return;
            }
            catch (Exception ex)
            {
                new QuickMessage($"Failed to upload the file.\nComment: {ex.Message}").ShowError();
                return;
            }

            _indexFileWindow = new IndexFileWindow(indexFileWindowVM);
            _indexFileWindow.Show();
        }

        public ICommand OpenCorpusCommand { get; }
        public bool CanOpenCorpusCommandExecute(object parameter)
        {
            return !StatisticsVM.IsStatisticsCaulculatingActive;
        }
        public void OnOpenCorpusCommandExecuted(object parameter)
        {
            if (!CanOpenCorpusCommandExecute(parameter))
                return;

            if (IsProjectChanged)
            {
                var msgRes = RequestProjectSaving();

                if (msgRes == MessageBoxResult.Cancel)
                    return;

                if (msgRes == MessageBoxResult.Yes)
                    ProjectStateSavingTimer.SaveAndWait();
            }

            if (!DialogProvider.GetCcaFilePath(out string path))
                return;

            LoadProjectCommand.Execute(path);
        }

        public ICommand LoadProjectCommand { get; }
        public bool CanLoadProjectCommandExecute(object parameter)
        {
            return parameter is string && !StatisticsVM.IsStatisticsCaulculatingActive;
        }
        public void OnLoadProjectCommandExecuted(object parameter)
        {
            if (!CanLoadProjectCommandExecute(parameter))
                return;

            string path = parameter as string;

            CloseSuggesterWindowCommand.Execute(null);

            try
            {
                SituationIndex.GetInstance().UnloadData();
                ProjectInfo.LoadProject(path);

                if (!LuceneService.OpenIndex())
                {
                    new QuickMessage("No index").ShowError();
                    return;
                }

                string projectName = Directory.GetParent(path).Name;
                string workingDirectory = Path.GetDirectoryName(path);
                var projectInfo = new ProjectInformation(projectName, workingDirectory);

                OnFileLoaded(projectInfo);
            }
            catch
            {
                new QuickMessage($"Failed to open corpus.").ShowError();
                return;
            }
        }

        #endregion

        #region PlotCommands

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

        #endregion

        #region HeatmapCommands

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

        #endregion

        #region ExtractorCommands

        public ICommand ShowExtractorCommand { get; }
        public bool CanShowExtractorCommandExecute(object parameter)
        {
            return IsFileLoaded && !StatisticsVM.IsStatisticsCaulculatingActive;
        }
        public void OnShowExtractorCommandExecuted(object parameter)
        {
            if (!CanShowExtractorCommandExecute(parameter))
                return;

            if (_extractorWindow != null)
            {
                new WindowInteract(_extractorWindow).MoveToForeground();
                return;
            }

            var vm = new ExtractorWindowViewModel()
            {
                DeactivateAction = () => _extractorWindow = null
            };

            _extractorWindow = new ExtractorWindow(vm);
            _extractorWindow.Show();
        }

        #endregion

        #endregion

        #region WindowsCommands

        public ICommand MainWindowLoadedCommand { get; }
        public bool CanMainWindowLoadedCommandExecute(object parameter)
        {
            return true;
        }
        public void OnMainWindowLoadedCommandExecuted(object parameter)
        {
            if (!CanMainWindowLoadedCommandExecute(parameter))
                return;

            var window = new WindowFinder().Find(typeof(MainWindow));
            var chatDataGrid = UIHelper.FindChildren<DataGrid>(window).FirstOrDefault(t => t.Name == "ChatDataGrid");
            var scrollViewer = UIHelper.FindChildren<ScrollViewer>(chatDataGrid).FirstOrDefault();

            scrollViewer.ScrollChanged += ChatVM.Scroller.ScrollChanged;
        }

        public ICommand MainWindowClosingCommand { get; }
        public bool CanMainWindowClosingCommandExecute(object parameter)
        {
            return parameter is CancelEventArgs;
        }
        public void OnMainWindowClosingCommandExecuted(object parameter)
        {
            if (!CanMainWindowClosingCommandExecute(parameter))
                return;

            var args = parameter as CancelEventArgs;

            if (IsProjectChanged)
            {
                var msgRes = RequestProjectSaving();

                if (msgRes == MessageBoxResult.Cancel)
                {
                    args.Cancel = true;
                    return;
                }

                if (msgRes == MessageBoxResult.Yes)
                    SavePresentStateCommand.Execute(null);
            }

            ProjectStateSavingTimer.Stop();
            MemoryCleaninigTimer.Stop();

            CloseIndexFileWindowCommand.Execute(null);
            CloseExtractorWindowCommand.Execute(null);
            CloseTagsetEditorWindowCommand.Execute(null);
            CloseSuggesterWindowCommand.Execute(null);
            CloseMessageExplorerWindowsCommand.Execute(null);

            SaveRecentProjects(RecentProjectProvider.RecentProjects);

            if (_csvExportService != null && _csvExportService.ExportingState == OperationState.InProcess)
                _csvExportService.StopExportingAndWait(1000);
        }

        public ICommand CloseIndexFileWindowCommand { get; }
        public bool CanCloseIndexFileWindowCommandExecute(object parameter)
        {
            return _indexFileWindow != null;
        }
        public void OnCloseIndexFileWindowCommandExecuted(object parameter)
        {
            if (!CanCloseIndexFileWindowCommandExecute(parameter))
                return;

            _indexFileWindow?.Close();
        }

        public ICommand CloseExtractorWindowCommand { get; }
        public bool CanCloseExtractorWindowCommandExecute(object parameter)
        {
            return _extractorWindow != null;
        }
        public void OnCloseExtractorWindowCommandExecuted(object parameter)
        {
            if (!CanCloseExtractorWindowCommandExecute(parameter))
                return;

            _extractorWindow?.Close();
        }

        public ICommand CloseTagsetEditorWindowCommand { get; }
        public bool CanCloseTagsetEditorWindowCommandExecute(object parameter)
        {
            return _tagsetEditorWindow != null;
        }
        public void OnCloseTagsetEditorWindowCommandExecuted(object parameter)
        {
            if (!CanCloseTagsetEditorWindowCommandExecute(parameter))
                return;

            _tagsetEditorWindow?.Close();
        }

        public ICommand CloseSuggesterWindowCommand { get; }
        public bool CanCloseSuggesterWindowCommandExecute(object parameter)
        {
            return _suggesterWindow != null;
        }
        public void OnCloseSuggesterWindowCommandExecuted(object parameter)
        {
            if (!CanCloseSuggesterWindowCommandExecute(parameter))
                return;

            _suggesterWindow?.Close();
        }

        public ICommand CloseMessageExplorerWindowsCommand { get; }
        public bool CanCloseMessageExplorerWindowsCommandExecute(object parameter)
        {
            return true;
        }
        public void OnCloseMessageExplorerWindowsCommandExecuted(object parameter)
        {
            if (!CanCloseMessageExplorerWindowsCommandExecute(parameter))
                return;

            MessageExplorerWindowViewModel.CloseAllExplorers();
        }

        #endregion

        public MainWindowViewModel()
        {
            ChatVM = new ChatViewModel(this);
            StatisticsVM = new StatisticsViewModel(this);
            ConcordanceVM = new ConcordanceViewModel();
            NGramsVM = new NGramsViewModel();

            MemoryCleaninigTimer = new MemoryCleaninigTimer();
            ProjectStateSavingTimer = new SavingTimer() { ChangeSavingStateAfterSuccessfulIteration = false };

            var recentProjects = LoadRecentProjects();
            RecentProjectProvider = new RecentProjectProvider(recentProjects);

            InitProjectStateSavingTimer();

            #region CommandsInitialization

            ClearRecentProjectsCommand = new RelayCommand(OnClearRecentProjectsCommandExecuted, CanClearRecentProjectsCommandExecute);

            IndexNewFileCommand = new RelayCommand(OnIndexNewFileCommandExecuted, CanIndexNewFileCommandExecute);
            OpenCorpusCommand = new RelayCommand(OnOpenCorpusCommandExecuted, CanOpenCorpusCommandExecute);
            LoadProjectCommand = new RelayCommand(OnLoadProjectCommandExecuted, CanLoadProjectCommandExecute);

            ShowPlotCommand = new RelayCommand(OnShowPlotCommandExecuted, CanShowPlotCommandExecute);
            ShowHeatmapCommand = new RelayCommand(OnShowHeatmapCommandExecuted, CanShowHeatmapCommandExecute);
            ShowExtractorCommand = new RelayCommand(OnShowExtractorCommandExecuted, CanShowExtractorCommandExecute);

            ExportXmlCommand = new RelayCommand(OnExportXmlCommandExecuted, CanExportXmlCommandExecute);
            ExportCsvCommand = new RelayCommand(OnExportCsvCommandExecuted, CanExportCsvCommandExecute);
            SavePresentStateCommand = new RelayCommand(OnSavePresentStateCommandExecuted, CanSavePresentStateCommandExecute);
            SavePresentStateByButtonCommand = new RelayCommand(OnSavePresentStateByButtonCommandExecuted, CanSavePresentStateByButtonCommandExecute);

            ImportXmlCommand = new RelayCommand(OnImportXmlCommandExecuted, CanImportXmlCommandExecute);

            ShowSuggesterCommand = new RelayCommand(OnShowSuggesterCommandExecuted, CanShowSuggesterCommandExecute);
            ShowTagsetEditorCommand = new RelayCommand(OnShowTagsetEditorCommandExecuted, CanShowTagsetEditorCommandExecute);

            MainWindowLoadedCommand = new RelayCommand(OnMainWindowLoadedCommandExecuted, CanMainWindowLoadedCommandExecute);
            MainWindowClosingCommand = new RelayCommand(OnMainWindowClosingCommandExecuted, CanMainWindowClosingCommandExecute);
            CloseIndexFileWindowCommand = new RelayCommand(OnCloseIndexFileWindowCommandExecuted, CanCloseIndexFileWindowCommandExecute);
            CloseExtractorWindowCommand = new RelayCommand(OnCloseExtractorWindowCommandExecuted, CanCloseExtractorWindowCommandExecute);
            CloseTagsetEditorWindowCommand = new RelayCommand(OnCloseTagsetEditorWindowCommandExecuted, CanCloseTagsetEditorWindowCommandExecute);
            CloseSuggesterWindowCommand = new RelayCommand(OnCloseSuggesterWindowCommandExecuted, CanCloseSuggesterWindowCommandExecute);
            CloseMessageExplorerWindowsCommand = new RelayCommand(OnCloseMessageExplorerWindowsCommandExecuted, CanCloseMessageExplorerWindowsCommandExecute);

            #endregion
        }

        #region UserInteractionMethods

        public MessageBoxResult RequestProjectSaving()
        {
            return new QuickMessage("The project has been changed. Save changes?").ShowQuestion(MessageBoxButton.YesNoCancel);
        }

        #endregion

        #region InitializationMethods

        private void InitProjectStateSavingTimer()
        {
            ProjectStateSavingTimer.Tick += delegate
            {
                SavePresentStateCommand.Execute(null);
            };

            ProjectStateSavingTimer.SuccessfulIteration += delegate
            {
                if (ProjectStateSavingTimer.SavingState == SaveProjectState.InProcess)
                    IsProjectChanged = false;
            };

            ProjectStateSavingTimer.SavingStateChanged += delegate (ProjectSavingStateEventArgs e)
            {
                switch (e.NewState)
                {
                    case SaveProjectState.InProcess:
                        ChangesSavingInProcessImageVisibility = Visibility.Visible;
                        break;

                    case SaveProjectState.ChangesSaved:
                        ChangesSavedImageVisibility = Visibility.Visible;
                        break;

                    case SaveProjectState.ChangesNotSaved:
                        ChangesNotSavedImageVisibility = Visibility.Visible;
                        break;
                }
            };
        }

        #endregion

        #region DataMethods

        private HashSet<RecentProject> LoadRecentProjects()
        {
            if (!ToolInfo.TryReadRecentProjectsFile(out string[] data))
                return new HashSet<RecentProject>();

            RecentProjectParser.TryParse(data, out HashSet<RecentProject> projects);
            projects?.RemoveWhere(t => !File.Exists(t.Path));

            return projects;
        }

        private bool SaveRecentProjects(IEnumerable<IRecentProject> recentProjects)
        {
            string[] data = RecentProjectParser.Save(recentProjects);
            return ToolInfo.TryWriteRecentProjectsFile(data);
        }

        #endregion

        #region FileLoadMethods

        private void OnFileLoaded(IProjectInformation projectInfo)
        {
            ProjectFileLoadState = FileLoadState.InProcess;

            StatisticsVM.ClearData();
            NGramsVM.ClearData();

            ChatVM.ResetDataCommand.Execute(null);
            ChatVM.SituationsVM.UpdateMessagesTags();

            ProjectFileLoadState = FileLoadState.Loaded;

            IsProjectChanged = false;
            ProjectStateSavingTimer.SavingState = SaveProjectState.ChangesSaved;

            ProjectStateSavingTimer.Start();
            MemoryCleaninigTimer.Start();

            ProjectInteraction.ProjectInfo = new ProjectInformation(projectInfo.Name, projectInfo.WorkingDirectory);
            RecentProject recentProject = new RecentProject(projectInfo.Name, projectInfo.ConfigFilePath);

            RecentProjectProvider.AddProject(recentProject);
        }

        #endregion

        #region ImportMethods

        private void RecoverData(List<SituationData> data)
        {
            SituationData[] recoveredData = data.Distinct().ToArray();

            if (data.Count != recoveredData.Length)
                data.Reset(recoveredData);

            data.RemoveAll(t => t.Messages.Count == 0);

            var sortedData = data.OrderBy(t => t.Header).ThenBy(t => t.Id).ToArray();
            data.Reset(sortedData);

            foreach (var situation in data)
            {
                situation.Messages.RemoveAll(id => id < 0);

                int[] recoveredMessages = situation.Messages.Distinct().ToArray();

                if (situation.Messages.Count != recoveredMessages.Length)
                    situation.Messages.Reset(recoveredMessages);

                situation.Messages.Sort();
            }

            data.RemoveAll(t => t.Messages.Count == 0);
        }

        private void SyncData(List<SituationData> unsynchronizedData)
        {
            int minMsgId = ProjectInteraction.FirstMessageId;
            int maxMsgId = ProjectInteraction.LastMessageId;

            foreach (var sitData in unsynchronizedData)
            {
                sitData.Messages.RemoveAll(id => id < minMsgId || id > maxMsgId);
            }

            unsynchronizedData.RemoveAll(t => t.Messages.Count == 0);

            if (unsynchronizedData.Count == 0)
                return;

            unsynchronizedData[0].Id = 0;

            for (int i = 1; i < unsynchronizedData.Count; ++i)
            {
                SituationData prevData = unsynchronizedData[i - 1];
                SituationData currData = unsynchronizedData[i];

                currData.Id = currData.Header == prevData.Header
                    ? prevData.Id + 1
                    : 0;
            }
        }

        #endregion
    }
}
