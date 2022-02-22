using ChatCorporaAnnotator.Infrastructure.Commands;
using ChatCorporaAnnotator.Services.Analysers.Concordance;
using ChatCorporaAnnotator.ViewModels.Base;
using IndexEngine.Data.Paths;
using IndexEngine.Search;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace ChatCorporaAnnotator.ViewModels.Analyzers
{
    internal class ConcordanceViewModel : ViewModel
    {
        public ObservableCollection<int> CharsCountCollection { get; }

        private string _concordanceViewerText;
        public string ConcordanceViewerText
        {
            get => _concordanceViewerText;
            set => SetValue(ref _concordanceViewerText, value);
        }

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

            string infoLine = $"Displaying {concordanceService.Concordance.Count} matches:\n";
            var builder = new StringBuilder(infoLine);

            foreach (var item in concordanceService.Concordance)
            {
                var line = PadString(item, Term, CharsCount);
                builder.AppendLine(line);
            }

            ConcordanceViewerText = builder.ToString();
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

            left = SetNavigationLines(left);

            return left + center + right;
        }

        private string SetNavigationLines(string text, int segmentLength = 5, char navigationSymbol = '|')
        {
            int spaceCounter = 0;
            char[] symbols = text.ToCharArray();

            for (int i = 0; i < symbols.Length; ++i)
            {
                if (symbols[i] != ' ')
                    break;

                if (symbols[i] == ' ')
                    spaceCounter++;

                if (spaceCounter == segmentLength)
                {
                    spaceCounter = 1;
                    symbols[i] = navigationSymbol;
                }
            }

            return new string(symbols);
        }
    }
}
