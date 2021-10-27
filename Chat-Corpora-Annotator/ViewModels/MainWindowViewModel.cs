using ChatCorporaAnnotator.Data.Dialogs;
using ChatCorporaAnnotator.Data.Indexing;
using ChatCorporaAnnotator.Data.Windows;
using ChatCorporaAnnotator.Data.Windows.UI;
using ChatCorporaAnnotator.Infrastructure.AppEventArgs;
using ChatCorporaAnnotator.Infrastructure.Commands;
using ChatCorporaAnnotator.Infrastructure.Enums;
using ChatCorporaAnnotator.Infrastructure.Exceptions.Indexing;
using ChatCorporaAnnotator.Infrastructure.Extensions;
using ChatCorporaAnnotator.Models.Indexing;
using ChatCorporaAnnotator.Models.Messages;
using ChatCorporaAnnotator.Models.Serialization;
using ChatCorporaAnnotator.Models.Timers;
using ChatCorporaAnnotator.Services;
using ChatCorporaAnnotator.ViewModels.Base;
using ChatCorporaAnnotator.ViewModels.Chat;
using ChatCorporaAnnotator.Views.Windows;
using IndexEngine;
using IndexEngine.Indexes;
using IndexEngine.Paths;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ChatCorporaAnnotator.ViewModels
{
    internal class MainWindowViewModel : ViewModel
    {
        private IndexFileWindow _indexFileWindow;

        public SavingTimer ProjectStateSavingTimer { get; }
        public MemoryCleaninigTimer MemoryCleaninigTimer { get; }

        public ChatViewModel ChatVM { get; }

        #region StaticData

        private static readonly string[] ProjectStateHeaders = new string[]
        {
            "saving changes",
            "all changes saved",
            "changes not saved"
        };

        #endregion

        #region States

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
                TagsetIndex.GetInstance().FlushIndexToDisk();
                UserDictsIndex.GetInstance().FlushIndexToDisk();
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

        #endregion

        #region ImportCommands

        public ICommand ImportXmlCommand { get; }
        public bool CanImportXmlCommandExecute(object parameter)
        {
            return IsFileLoaded;
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
            return false;
        }
        public void OnShowSuggesterCommandExecuted(object parameter)
        {
            if (!CanShowSuggesterCommandExecute(parameter))
                return;
        }

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

        #endregion

        #region TopBarCommands

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
                indexFileWindowVM = new IndexFileWindowViewModel(path)
                {
                    FinishAction = () => OnFileLoaded(),
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
            return true;
        }
        public void OnOpenCorpusCommandExecuted(object parameter)
        {
            if (!CanOpenCorpusCommandExecute(parameter))
                return;

            if (!DialogProvider.GetFolderPath(out string path))
                return;

            try
            {
                SituationIndex.GetInstance().UnloadData();
                ProjectInfo.LoadProject(path);

                if (!LuceneService.OpenIndex())
                {
                    new QuickMessage("No index").ShowError();
                    return;
                }

                string dirName = System.IO.Path.GetDirectoryName(path);
                ProjectInteraction.ProjectInfo = new ProjectInformation(dirName, path);

                OnFileLoaded();
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

        #region ExtractFileCommands

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

            ProjectStateSavingTimer.Stop();
            MemoryCleaninigTimer.Stop();

            CloseIndexFileWindowCommand.Execute(null);
            CloseMessageExplorerWindowsCommand.Execute(null);

            if (IsProjectChanged)
            {
                var msgRes = MessageBox.Show("The project has been changed. Save changes?",
                    "Question", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

                if (msgRes == MessageBoxResult.Cancel)
                {
                    args.Cancel = true;
                    return;
                }

                if (msgRes == MessageBoxResult.Yes)
                    SavePresentStateCommand.Execute(null);
            }
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

            _indexFileWindow.Close();
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

            MemoryCleaninigTimer = new MemoryCleaninigTimer();
            ProjectStateSavingTimer = new SavingTimer();

            InitProjectStateSavingTimer();

            #region CommandsInitialization

            IndexNewFileCommand = new RelayCommand(OnIndexNewFileCommandExecuted, CanIndexNewFileCommandExecute);
            OpenCorpusCommand = new RelayCommand(OnOpenCorpusCommandExecuted, CanOpenCorpusCommandExecute);

            ShowPlotCommand = new RelayCommand(OnShowPlotCommandExecuted, CanShowPlotCommandExecute);
            ShowHeatmapCommand = new RelayCommand(OnShowHeatmapCommandExecuted, CanShowHeatmapCommandExecute);
            ExtractFileCommand = new RelayCommand(OnExtractFileCommandExecuted, CanExtractFileCommandExecute);

            ExportXmlCommand = new RelayCommand(OnExportXmlCommandExecuted, CanExportXmlCommandExecute);
            SavePresentStateCommand = new RelayCommand(OnSavePresentStateCommandExecuted, CanSavePresentStateCommandExecute);
            SavePresentStateByButtonCommand = new RelayCommand(OnSavePresentStateByButtonCommandExecuted, CanSavePresentStateByButtonCommandExecute);

            ImportXmlCommand = new RelayCommand(OnImportXmlCommandExecuted, CanImportXmlCommandExecute);

            ShowSuggesterCommand = new RelayCommand(OnShowSuggesterCommandExecuted, CanShowSuggesterCommandExecute);
            ShowTagsetEditorCommand = new RelayCommand(OnShowTagsetEditorCommandExecuted, CanShowTagsetEditorCommandExecute);

            MainWindowLoadedCommand = new RelayCommand(OnMainWindowLoadedCommandExecuted, CanMainWindowLoadedCommandExecute);
            MainWindowClosingCommand = new RelayCommand(OnMainWindowClosingCommandExecuted, CanMainWindowClosingCommandExecute);
            CloseIndexFileWindowCommand = new RelayCommand(OnCloseIndexFileWindowCommandExecuted, CanCloseIndexFileWindowCommandExecute);
            CloseMessageExplorerWindowsCommand = new RelayCommand(OnCloseMessageExplorerWindowsCommandExecuted, CanCloseMessageExplorerWindowsCommandExecute);

            #endregion
        }

        #region InitializationMethods

        private void InitProjectStateSavingTimer()
        {
            var window = new WindowFinder().Find(typeof(MainWindow));

            ProjectStateSavingTimer.Tick += delegate
            {
                window?.Dispatcher.Invoke(() => SavePresentStateCommand.Execute(null));
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

        #region FileLoadMethods

        private void OnFileLoaded()
        {
            ProjectFileLoadState = FileLoadState.InProcess;

            ChatVM.ResetDataCommand.Execute(null);
            ChatVM.SituationsVM.UpdateMessagesTags();

            ProjectFileLoadState = FileLoadState.Loaded;

            ProjectStateSavingTimer.Start();
            MemoryCleaninigTimer.Start();
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
