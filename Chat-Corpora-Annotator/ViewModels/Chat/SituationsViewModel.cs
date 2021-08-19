using ChatCorporaAnnotator.Infrastructure.Commands;
using ChatCorporaAnnotator.Infrastructure.Extensions;
using ChatCorporaAnnotator.Models.Chat;
using ChatCorporaAnnotator.ViewModels.Base;
using IndexEngine.Indexes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace ChatCorporaAnnotator.ViewModels.Chat
{
    internal class SituationsViewModel : ViewModel
    {
        private readonly MainWindowViewModel _mainWindowVM;

        public List<int> TaggedIds { get; private set; }
        public ObservableCollection<Situation> Situations { get; private set; }

        #region Commands

        public ICommand SetSituationsCommand { get; }
        public bool CanSetSituationsCommandExecute(object parameter)
        {
            return true;
        }
        public void OnSetSituationsCommandExecuted(object parameter)
        {
            if (!CanSetSituationsCommandExecute(parameter))
                return;

            var newSituations = parameter is IEnumerable<Situation> situations
                ? situations
                : GetSituations();

            if (newSituations.IsNullOrEmpty())
            {
                TaggedIds.Clear();
                Situations.Clear();
                return;
            }

            Situations = new ObservableCollection<Situation>(newSituations);
            OnPropertyChanged(nameof(Situations));

            _mainWindowVM.UpdateSituationCountCommand?.Execute(SituationIndex.GetInstance().ItemCount);
        }

        public ICommand AddSituationsCommand { get; }
        public bool CanAddSituationsCommandExecute(object parameter)
        {
            return parameter is IEnumerable<Situation>;
        }
        public void OnAddSituationsCommandExecuted(object parameter)
        {
            if (!CanAddSituationsCommandExecute(parameter))
                return;

            var addingSituations = parameter as IEnumerable<Situation>;

            if (addingSituations.IsNullOrEmpty())
                return;

            Situations = new ObservableCollection<Situation>(Situations.Concat(addingSituations));
            OnPropertyChanged(nameof(Situations));

            _mainWindowVM.UpdateSituationCountCommand?.Execute(SituationIndex.GetInstance().ItemCount);
        }

        #endregion

        public SituationsViewModel(MainWindowViewModel mainWindowVM)
        {
            _mainWindowVM = mainWindowVM ?? throw new ArgumentNullException(nameof(mainWindowVM));

            TaggedIds = new List<int>();
            Situations = new ObservableCollection<Situation>();

            SetSituationsCommand = new RelayCommand(OnSetSituationsCommandExecuted, CanSetSituationsCommandExecute);
            AddSituationsCommand = new RelayCommand(OnAddSituationsCommandExecuted, CanAddSituationsCommandExecute);
        }

        private IEnumerable<Situation> GetSituations()
        {
            SituationIndex.GetInstance().ReadIndexFromDisk();

            var situationSet = new HashSet<Situation>();

            foreach (var kvp in SituationIndex.GetInstance().IndexCollection)
            {
                foreach (var situationPresenter in kvp.Value)
                {
                    int key = situationPresenter.Key;
                    string header = $"{kvp.Key} {key}";
                    var situation = new Situation(key, header);

                    situationSet.Add(situation);
                }
            }

            TaggedIds = SituationIndex.GetInstance().InvertedIndex.Keys.ToList();
            TaggedIds.Sort();

            return situationSet;
        }
    }
}
