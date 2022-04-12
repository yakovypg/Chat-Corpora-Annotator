using ChatCorporaAnnotator.Data.Dialogs;
using ChatCorporaAnnotator.Infrastructure.Commands;
using ChatCorporaAnnotator.Models.Messages;
using ChatCorporaAnnotator.ViewModels.Base;
using CoreNLPEngine.Diagnostics;
using CoreNLPEngine.Extraction;
using IndexEngine.Data.Paths;
using System;
using System.Diagnostics;
using System.Windows.Input;
using System.Windows.Media;

namespace ChatCorporaAnnotator.ViewModels.Windows
{
    internal class ExtractorWindowViewModel : ViewModel
    {
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

               Extractor.Config.CoreNLPPath = value;
               IsCoreNLPInstalled = ExtractComponentsVerifier.IsCoreNLPInstalled();
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

                JavaInfoBackground = ExtractComponentsVerifier.IsJavaInstalled()
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

                CoreNLPInfoBackground = ExtractComponentsVerifier.IsCoreNLPInstalled()
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

        #endregion

        #region ExtractCommands

        public ICommand ResetConfigCommand { get; }
        public bool CanResetConfigCommandExecute(object parameter)
        {
            return true;
        }
        public void OnResetConfigCommandExecuted(object parameter)
        {
            if (!CanResetConfigCommandExecute(parameter))
                return;

            Extractor.Config = ExtractConfig.Default;

            CoreNLPPath = Extractor.Config.CoreNLPPath;
            CoreNLPClientMemoryText = Extractor.Config.CoreNLPClientMemory.ToString();
            CoreNLPClientTimeoutText = Extractor.Config.CoreNLPClientTimeout.ToString();
        }

        public ICommand SetCoreNLPPathCommand { get; }
        public bool CanSetCoreNLPPathCommandExecute(object parameter)
        {
            return true;
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
            return IsJavaInstalled && IsCoreNLPInstalled;
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

            Extractor.Config.CoreNLPClientMemory = memory;
            Extractor.Config.CoreNLPClientTimeout = timeout;

            //Extractor.Extract();
            Extractor.ExtractAsync();
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

            Extractor.Config.TrySaveConfigToDisk();
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
                new QuickMessage(ex.Message).ShowError();
            }
        }

        #endregion

        public ExtractorWindowViewModel()
        {
            Extractor.Config.LoadConfigFromDisk();

            CoreNLPPath = Extractor.Config.CoreNLPPath;
            CoreNLPClientMemoryText = Extractor.Config.CoreNLPClientMemory.ToString();
            CoreNLPClientTimeoutText = Extractor.Config.CoreNLPClientTimeout.ToString();

            IsJavaInstalled = ExtractComponentsVerifier.IsJavaInstalled();
            IsCoreNLPInstalled = ExtractComponentsVerifier.IsCoreNLPInstalled();

            #region CommandsInitialization

            ResetConfigCommand = new RelayCommand(OnResetConfigCommandExecuted, CanResetConfigCommandExecute);
            SetCoreNLPPathCommand = new RelayCommand(OnSetCoreNLPPathCommandExecuted, CanSetCoreNLPPathCommandExecute);
            ExtractCommand = new RelayCommand(OnExtractCommandExecuted, CanExtractCommandExecute);

            OpenComponentSitesCommand = new RelayCommand(OnOpenComponentSitesCommandExecuted, CanOpenComponentSitesCommandExecute);

            DeactivateWindowCommand = new RelayCommand(OnDeactivateWindowCommandExecuted, CanDeactivateWindowCommandExecute);
            SaveExtractConfigCommand = new RelayCommand(OnSaveExtractConfigCommandExecuted, CanSaveExtractConfigCommandExecute);

            #endregion
        }
    }
}
