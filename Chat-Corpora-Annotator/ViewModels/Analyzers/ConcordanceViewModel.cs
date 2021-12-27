using ChatCorporaAnnotator.Data.Windows;
using ChatCorporaAnnotator.Infrastructure.Commands;
using ChatCorporaAnnotator.Services.Analysers.Concordance;
using ChatCorporaAnnotator.ViewModels.Base;
using IndexEngine.Data.Paths;
using IndexEngine.Search;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace ChatCorporaAnnotator.ViewModels.Analyzers
{
    internal class ConcordanceViewModel : ViewModel
    {
        public ObservableCollection<int> CharsCountCollection { get; }

        private string _term;
        public string Term
        {
            get => _term;
            set => SetValue(ref _term, value);
        }

        private int _charsCount;
        public int CharsCount
        {
            get => _charsCount;
            set => SetValue(ref _charsCount, value);
        }

        #region Commands

        public ICommand ShowConcordanceCommand { get; set; }
        public bool CanShowConcordanceCommandExecute(object parameter)
        {
            return !string.IsNullOrEmpty(Term) &&
                   !string.IsNullOrWhiteSpace(Term) &&
                   Term.Split().Length <= 1;
        }
        public void OnShowConcordanceCommandExecuted(object parameter)
        {
            if (!CanShowConcordanceCommandExecute(parameter))
                return;

            var concordanceService = new ConcordanceService
            {
                ConQuery = LuceneService.Parser.Parse(Term)
            };

            concordanceService.FindConcordance(Term, ProjectInfo.TextFieldKey, CharsCount);

            var mainWindowInteract = new MainWindowInteract();

            mainWindowInteract.ConcordanceViewer.ReadOnly = false;

            mainWindowInteract.ClearConcordanceViewer();
            mainWindowInteract.AddLineToConcordanceViewer($"Displaying {concordanceService.Concordance.Count} matches:");

            foreach (var item in concordanceService.Concordance)
            {
                var line = PadString(item, Term, CharsCount);
                mainWindowInteract.AddLineToConcordanceViewer(line);
            }

            mainWindowInteract.ConcordanceViewer.ReadOnly = true;
        }

        #endregion

        public ConcordanceViewModel()
        {
            CharsCountCollection = new ObservableCollection<int>(Enumerable.Range(5, 26));
            CharsCount = CharsCountCollection[0];

            ShowConcordanceCommand = new RelayCommand(OnShowConcordanceCommandExecuted, CanShowConcordanceCommandExecute);
        }

        private string PadString(string line, string term, int count)
        {
            int index = line.ToLower().IndexOf(term);

            if (index == -1)
                return null;

            string left = line.Substring(0, index);
            string center = line.Substring(index, term.Length);
            string right = line.Substring(index + term.Length);

            if (!string.IsNullOrEmpty(left))
            {
                if (left.Length < count)
                {
                    left = left.PadLeft(count + 3);
                }
                else
                {
                    left = left.Remove(0, left.Length - count);
                    left = "..." + left;
                }
            }
            else
            {
                left = " ".PadLeft(count + 3);
            }

            if (!string.IsNullOrEmpty(right))
            {
                if (right.Length < count)
                {
                    right = right.PadRight(count + 3);
                }
                else
                {
                    var temp = right.Length - count;

                    right = right.Remove(right.Length - temp, temp);
                    right += "...";
                }
            }
            else
            {
                right = " ".PadRight(count);
            }

            return left + center + right;
        }
    }
}
