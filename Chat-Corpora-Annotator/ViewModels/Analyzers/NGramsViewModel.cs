using ChatCorporaAnnotator.Infrastructure.Commands;
using ChatCorporaAnnotator.Models.Messages;
using ChatCorporaAnnotator.Models.NGrams;
using ChatCorporaAnnotator.Services.Analysers.NGrams;
using ChatCorporaAnnotator.ViewModels.Base;
using CSharpTest.Net.Collections;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ChatCorporaAnnotator.ViewModels.Analyzers
{
    internal class NGramsViewModel : ViewModel
    {
        /*
            TODO:
            1. ОБЕЗОПАСИТЬ МНОГОПОТОЧКУ!
            2. СИНХРОНИЗАЦИЯ С Statistics?
            3. ReadOnlyMode как в StatisticsVM?
        */
        
        private NGramService _nGramService;

        public ObservableCollection<NGramItem> Bigrams { get; private set; }
        public ObservableCollection<NGramItem> Trigrams { get; private set; }
        public ObservableCollection<NGramItem> FourGrams { get; private set; }
        public ObservableCollection<NGramItem> FiveGrams { get; private set; }

        private string _nGram;
        public string NGram
        {
            get => _nGram;
            set => SetValue(ref _nGram, value);
        }

        private bool _isIndexBuilded;
        public bool IsIndexBuilded
        {
            get => _isIndexBuilded;
            private set => SetValue(ref _isIndexBuilded, value);
        }

        private bool _isIndexBuilingStarted;
        public bool IsIndexBuilingStarted
        {
            get => _isIndexBuilingStarted;
            private set => SetValue(ref _isIndexBuilingStarted, value);
        }

        #region Commands

        public ICommand SearchNGramsCommand { get; }
        public bool CanSearchNGramsCommandExecute(object parameter)
        {
            return IsIndexBuilded &&
                   !string.IsNullOrEmpty(NGram) &&
                   !string.IsNullOrWhiteSpace(NGram) &&
                   NGram.Split().Length <= 1;
        }
        public void OnSearchNGramsCommandExecuted(object parameter)
        {
            if (!CanSearchNGramsCommandExecute(parameter))
                return;

            if (!_nGramService.IndexIsRead)
            {
                if (!ReadIndex())
                    return;
            }

            var result = _nGramService.GetReadableResultsForTerm(NGram);
            DisplayNGrams(result);
        }

        public ICommand BuildIndexCommand { get; }
        public bool CanBuildIndexCommandExecute(object parameter)
        {
            return !IsIndexBuilingStarted;
        }
        public void OnBuildIndexCommandExecuted(object parameter)
        {
            if (!CanBuildIndexCommandExecute(parameter))
                return;

            IsIndexBuilded = false;
            IsIndexBuilingStarted = true;

            var task = Task.Run(delegate
            {
                try
                {
                    _nGramService.BuildFullIndex();
                    IsIndexBuilded = true;
                }
                catch { }

                IsIndexBuilingStarted = false;
            });
        }

        #endregion

        public NGramsViewModel()
        {
            _nGramService = new NGramService();

            Bigrams = new ObservableCollection<NGramItem>();
            Trigrams = new ObservableCollection<NGramItem>();
            FourGrams = new ObservableCollection<NGramItem>();
            FiveGrams = new ObservableCollection<NGramItem>();

            SearchNGramsCommand = new RelayCommand(OnSearchNGramsCommandExecuted, CanSearchNGramsCommandExecute);
            BuildIndexCommand = new RelayCommand(OnBuildIndexCommandExecuted, CanBuildIndexCommandExecute);
        }

        public void ClearData()
        {
            NGram = string.Empty;

            IsIndexBuilded = false;
            IsIndexBuilingStarted = false;

            Bigrams.Clear();
            Trigrams.Clear();
            FourGrams.Clear();
            FiveGrams.Clear();

            _nGramService = new NGramService();

            if (_nGramService.IndexExists)
                IsIndexBuilded = true;
        }

        private bool ReadIndex()
        {
            try
            {
                _nGramService.ReadIndexFromDisk();
                return true;
            }
            catch (Exception ex)
            {
                new QuickMessage(ex.Message).ShowError();
                return false;
            }
        }

        private void DisplayNGrams(List<BTreeDictionary<string, int>> grams)
        {
            Bigrams = new ObservableCollection<NGramItem>(grams[0].Select(t => new NGramItem(t.Key, t.Value)));
            Trigrams = new ObservableCollection<NGramItem>(grams[1].Select(t => new NGramItem(t.Key, t.Value)));
            FourGrams = new ObservableCollection<NGramItem>(grams[2].Select(t => new NGramItem(t.Key, t.Value)));
            FiveGrams = new ObservableCollection<NGramItem>(grams[3].Select(t => new NGramItem(t.Key, t.Value)));

            OnPropertyChanged(nameof(Bigrams));
            OnPropertyChanged(nameof(Trigrams));
            OnPropertyChanged(nameof(FourGrams));
            OnPropertyChanged(nameof(FiveGrams));
        }
    }
}
