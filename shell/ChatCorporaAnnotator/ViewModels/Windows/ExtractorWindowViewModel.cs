using ChatCorporaAnnotator.Data.Dialogs;
using ChatCorporaAnnotator.Data.Indexing;
using ChatCorporaAnnotator.Data.Windows;
using ChatCorporaAnnotator.Infrastructure.Commands;
using ChatCorporaAnnotator.Models.Messages;
using ChatCorporaAnnotator.ViewModels.Base;
using ChatCorporaAnnotator.Views.Windows;
using CoreNLPEngine.Diagnostics;
using CoreNLPEngine.Extraction;
using CoreNLPEngine.Search;
using IndexEngine.Data.Paths;
using System;
using System.Diagnostics;
using System.Windows.Input;
using System.Windows.Media;

namespace ChatCorporaAnnotator.ViewModels.Windows
{
    internal class ExtractorWindowViewModel : ViewModel
    {
        private readonly Extractor _extractor;
        
        public Action? DeactivateAction { get; set; }

        #region ConfigItems

        private string _coreNLPClientMemoryText = string.Empty;
        public string CoreNLPClientMemoryText
        {
            get => _coreNLPClientMemoryText;
            set => SetValue(ref _coreNLPClientMemoryText, value);
        }

        private string _coreNLPClientTimeoutText = string.Empty;
        public string CoreNLPClientTimeoutText
        {
            get => _coreNLPClientTimeoutText;
            set => SetValue(ref _coreNLPClientTimeoutText, value);
        }

        private string _coreNLPPath = string.Empty;
        public string CoreNLPPath
        {
            get => _coreNLPPath;
            set
            {
                if (!SetValue(ref _coreNLPPath, value))
                    return;

                _extractor.Config.CoreNLPPath = value;
                _extractor.UpdateStopwords();

               IsCoreNLPInstalled = ExtractComponentsVerifier.IsCoreNLPInstalled(_extractor.Config.CoreNLPPath);
               IsSRParserModelsInstalled = ExtractComponentsVerifier.ContainsEnglishModelsForSRParser(_extractor.Config.CoreNLPPath);
            }
        }

        #endregion

        #region ComponentsInfo

        private bool _isJavaInstalled = false;
        public bool IsJavaInstalled
        {
            get => _isJavaInstalled;
            private set
            {
                if (!SetValue(ref _isJavaInstalled, value))
                    return;

                JavaInfoBackground = value
                    ? Brushes.LightGreen
                    : Brushes.Pink;
            }
        }

        private bool _isCoreNLPInstalled = false;
        public bool IsCoreNLPInstalled
        {
            get => _isCoreNLPInstalled;
            private set
            {
                if (!SetValue(ref _isCoreNLPInstalled, value))
                    return;

                CoreNLPInfoBackground = value
                    ? Brushes.LightGreen
                    : Brushes.Pink;
            }
        }

        private bool _isSRParserModelsInstalled = false;
        public bool IsSRParserModelsInstalled
        {
            get => _isSRParserModelsInstalled;
            private set
            {
                if (!SetValue(ref _isSRParserModelsInstalled, value))
                    return;

                SRParserModelsInfoBackground = value
                    ? Brushes.LightGreen
                    : Brushes.Pink;
            }
        }

        #endregion

        #region BackgroundItems

        private SolidColorBrush _javaInfoBackground = Brushes.LightPink;
        public SolidColorBrush JavaInfoBackground
        {
            get => _javaInfoBackground;
            private set => SetValue(ref _javaInfoBackground, value);
        }

        private SolidColorBrush _coreNLPInfoBackground = Brushes.LightPink;
        public SolidColorBrush CoreNLPInfoBackground
        {
            get => _coreNLPInfoBackground;
            private set => SetValue(ref _coreNLPInfoBackground, value);
        }

        private SolidColorBrush _srParserModelsInfoBackground = Brushes.LightPink;
        public SolidColorBrush SRParserModelsInfoBackground
        {
            get => _srParserModelsInfoBackground;
            private set => SetValue(ref _srParserModelsInfoBackground, value);
        }

        #endregion

        #region ProgressBarItems

        private const int DEFAULT_PROGRESS_UPDATE_INTERVAL = 10;

        private bool _isExtractionActive = false;
        public bool IsExtractionActive
        {
            get => _isExtractionActive;
            private set => SetValue(ref _isExtractionActive, value);
        }

        private double _progressBarMinimum = 0;
        public double ProgressBarMinimum
        {
            get => _progressBarMinimum;
            set => SetValue(ref _progressBarMinimum, value);
        }

        private double _progressBarMaximum = 100;
        public double ProgressBarMaximum
        {
            get => _progressBarMaximum;
            set => SetValue(ref _progressBarMaximum, value);
        }

        private double _progressBarCurrentValue = 0;
        public double ProgressBarCurrentValue
        {
            get => _progressBarCurrentValue;
            set => SetValue(ref _progressBarCurrentValue, value);
        }

        #endregion

        #region ExtractCommands

        public ICommand ResetConfigCommand { get; }
        public bool CanResetConfigCommandExecute(object parameter)
        {
            return !IsExtractionActive;
        }
        public void OnResetConfigCommandExecuted(object parameter)
        {
            if (!CanResetConfigCommandExecute(parameter))
                return;

            _extractor.Config = ExtractConfig.Default;

            CoreNLPPath = _extractor.Config.CoreNLPPath;
            CoreNLPClientMemoryText = _extractor.Config.CoreNLPClientMemory.ToString();
            CoreNLPClientTimeoutText = _extractor.Config.CoreNLPClientTimeout.ToString();
        }

        public ICommand SetCoreNLPPathCommand { get; }
        public bool CanSetCoreNLPPathCommandExecute(object parameter)
        {
            return !IsExtractionActive;
        }
        public void OnSetCoreNLPPathCommandExecuted(object parameter)
        {
            if (!CanSetCoreNLPPathCommandExecute(parameter))
                return;

            if (!DialogProvider.GetFolderPath(out string path) || string.IsNullOrEmpty(path))
                return;

            CoreNLPPath = path;
        }

        public ICommand ExtractCommand { get; }
        public bool CanExtractCommandExecute(object parameter)
        {
            return IsJavaInstalled && IsCoreNLPInstalled && !IsExtractionActive;
        }
        public void OnExtractCommandExecuted(object parameter)
        {
            if (!CanExtractCommandExecute(parameter))
                return;

            if (!int.TryParse(CoreNLPClientMemoryText, out int memory) ||
                !int.TryParse(CoreNLPClientTimeoutText, out int timeout))
            {
                new QuickMessage("Incorrect configuration parameters have been entered.").ShowError();
                return;
            }

            IsExtractionActive = true;
            ProgressBarCurrentValue = 0;

            ProgressBarMinimum = 0;
            ProgressBarMaximum = ProjectInteraction.MessagesCount;

            _extractor.Config.CoreNLPClientMemory = memory;
            _extractor.Config.CoreNLPClientTimeout = timeout;

            if (IsSRParserModelsInstalled)
                _extractor.Config.CoreNLPClientProperties = ExtractConfig.GetCoreNLPClientDefaultProperties();

            _ = _extractor.ExtractAsync();
        }

        #endregion

        #region ComponentsCommands

        public ICommand OpenComponentSitesCommand { get; }
        public bool CanOpenComponentSitesCommandExecute(object parameter)
        {
            return true;
        }
        public void OnOpenComponentSitesCommandExecuted(object parameter)
        {
            if (!CanOpenComponentSitesCommandExecute(parameter))
                return;

            if (!ToolInfo.TryReadExtractorComponentSites(out string[] sites))
                return;

            foreach (string site in sites)
            {
                string prefix = "https://";

                if (string.IsNullOrEmpty(site) ||
                    site.Length <= prefix.Length ||
                    site.Remove(prefix.Length) != prefix)
                {
                    continue;
                }

                try
                {
                    var process = new Process();
                    process.StartInfo.FileName = site;
                    process.StartInfo.UseShellExecute = true;
                    
                    process.Start();
                }
                catch { }
            }
        }

        public ICommand UpdateJavaInfoCommand { get; }
        public bool CanUpdateJavaInfoCommandExecute(object parameter)
        {
            return true;
        }
        public void OnUpdateJavaInfoCommandExecuted(object parameter)
        {
            if (!CanUpdateJavaInfoCommandExecute(parameter))
                return;

            IsJavaInstalled = ExtractComponentsVerifier.IsJavaInstalled();
        }

        public ICommand UpdateCoreNLPInfoCommand { get; }
        public bool CanUpdateCoreNLPInfoCommandExecute(object parameter)
        {
            return true;
        }
        public void OnUpdateCoreNLPInfoCommandExecuted(object parameter)
        {
            if (!CanUpdateCoreNLPInfoCommandExecute(parameter))
                return;

            IsCoreNLPInstalled = ExtractComponentsVerifier.IsCoreNLPInstalled(_extractor.Config.CoreNLPPath);
        }

        public ICommand UpdateSRParserModelsInfoCommand { get; }
        public bool CanUpdateSRParserModelsInfoCommandExecute(object parameter)
        {
            return true;
        }
        public void OnUpdateSRParserModelsInfoCommandExecuted(object parameter)
        {
            if (!CanUpdateSRParserModelsInfoCommandExecute(parameter))
                return;

            IsSRParserModelsInstalled = ExtractComponentsVerifier.ContainsEnglishModelsForSRParser(_extractor.Config.CoreNLPPath);
        }

        #endregion

        #region SystemCommands

        public ICommand SaveExtractConfigCommand { get; }

        public bool CanSaveExtractConfigCommandExecute(object parameter)
        {
            return true;
        }
        public void OnSaveExtractConfigCommandExecuted(object parameter)
        {
            if (!CanSaveExtractConfigCommandExecute(parameter))
                return;

            _extractor.Config.TrySaveConfigToDisk();
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

            _extractor.StopExtraction();

            try
            {
                DeactivateAction?.Invoke();
            }
            catch (Exception ex)
            {
                new QuickMessage(ex.Message).ShowError();
            }
        }

        #endregion

        public ExtractorWindowViewModel()
        {
            _extractor = new Extractor() { ProgressUpdateInterval = DEFAULT_PROGRESS_UPDATE_INTERVAL };
            _extractor.Config.LoadConfigFromDisk(false);

            var mainWindowDispatcher = new WindowFinder().Find(typeof(MainWindow)).Dispatcher;

            _extractor.ProgressChanged += (delta, currValue) =>
            {
                mainWindowDispatcher.Invoke(() => ProgressBarCurrentValue = currValue);
            };
            _extractor.FailedExtraction += () =>
            {
                mainWindowDispatcher.Invoke(() =>
                    new QuickMessage("Extraction was failed.").ShowInformation()
                );
            };
            _extractor.SuccessfulExtraction += () =>
            {
                IsExtractionActive = false;
                RetrieversSearch.Extractor = _extractor;

                mainWindowDispatcher.Invoke(() =>
                    new QuickMessage("Extraction was successful.").ShowInformation()
                );
            };

            CoreNLPPath = _extractor.Config.CoreNLPPath;
            CoreNLPClientMemoryText = _extractor.Config.CoreNLPClientMemory.ToString();
            CoreNLPClientTimeoutText = _extractor.Config.CoreNLPClientTimeout.ToString();

            IsJavaInstalled = ExtractComponentsVerifier.IsJavaInstalled();
            IsCoreNLPInstalled = ExtractComponentsVerifier.IsCoreNLPInstalled(_extractor.Config.CoreNLPPath);
            IsSRParserModelsInstalled = ExtractComponentsVerifier.ContainsEnglishModelsForSRParser(_extractor.Config.CoreNLPPath);

            #region CommandsInitialization

            ResetConfigCommand = new RelayCommand(OnResetConfigCommandExecuted, CanResetConfigCommandExecute);
            SetCoreNLPPathCommand = new RelayCommand(OnSetCoreNLPPathCommandExecuted, CanSetCoreNLPPathCommandExecute);
            ExtractCommand = new RelayCommand(OnExtractCommandExecuted, CanExtractCommandExecute);

            OpenComponentSitesCommand = new RelayCommand(OnOpenComponentSitesCommandExecuted, CanOpenComponentSitesCommandExecute);
            UpdateJavaInfoCommand = new RelayCommand(OnUpdateJavaInfoCommandExecuted, CanUpdateJavaInfoCommandExecute);
            UpdateCoreNLPInfoCommand = new RelayCommand(OnUpdateCoreNLPInfoCommandExecuted, CanUpdateCoreNLPInfoCommandExecute);
            UpdateSRParserModelsInfoCommand = new RelayCommand(OnUpdateSRParserModelsInfoCommandExecuted, CanUpdateSRParserModelsInfoCommandExecute);

            DeactivateWindowCommand = new RelayCommand(OnDeactivateWindowCommandExecuted, CanDeactivateWindowCommandExecute);
            SaveExtractConfigCommand = new RelayCommand(OnSaveExtractConfigCommandExecuted, CanSaveExtractConfigCommandExecute);

            #endregion
        }
    }
}
