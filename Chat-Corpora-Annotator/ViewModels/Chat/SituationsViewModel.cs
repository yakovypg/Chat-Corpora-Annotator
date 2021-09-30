using ChatCorporaAnnotator.Data.Indexing;
using ChatCorporaAnnotator.Infrastructure.Commands;
using ChatCorporaAnnotator.Infrastructure.Extensions;
using ChatCorporaAnnotator.Models.Chat;
using ChatCorporaAnnotator.ViewModels.Base;
using IndexEngine;
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

        public List<int> TaggedMessagesIds { get; private set; }
        public ObservableCollection<Situation> Situations { get; private set; }

        #region AddingCommands

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
                TaggedMessagesIds.Clear();
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

        #region EditCommands

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

        public SituationsViewModel(MainWindowViewModel mainWindowVM)
        {
            _mainWindowVM = mainWindowVM ?? throw new ArgumentNullException(nameof(mainWindowVM));

            TaggedMessagesIds = new List<int>();
            Situations = new ObservableCollection<Situation>();

            SetSituationsCommand = new RelayCommand(OnSetSituationsCommandExecuted, CanSetSituationsCommandExecute);
            AddSituationsCommand = new RelayCommand(OnAddSituationsCommandExecuted, CanAddSituationsCommandExecute);

            MergeSituationsCommand = new RelayCommand(OnMergeSituationsCommandExecuted, CanMergeSituationsCommandExecute);
            DeleteSituationCommand = new RelayCommand(OnDeleteSituationCommandExecuted, CanDeleteSituationCommandExecute);
            ChangeSituationTagCommand = new RelayCommand(OnChangeSituationTagCommandExecuted, CanChangeSituationTagCommandExecute);
        }

        public void ClearData()
        {
            TaggedMessagesIds.Clear();
            Situations.Clear();
        }

        public void UpdateMessagesTags()
        {
            var currMessages = _mainWindowVM.ChatVM.MessagesVM.MessagesCase.CurrentMessages;

            if (currMessages.IsNullOrEmpty())
                return;

            var invertedIndex = SituationIndex.GetInstance().InvertedIndex;

            foreach (var msg in currMessages)
            {
                if (TaggedMessagesIds.Contains(msg.Source.Id))
                {
                    foreach (var situationData in invertedIndex[msg.Source.Id])
                    {
                        var situation = new Situation(situationData.Value, situationData.Key);
                        msg.AddSituation(situation, _mainWindowVM.ChatVM.TagsVM.CurrentTagset);
                    }
                }
            }
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

            TaggedMessagesIds = SituationIndex.GetInstance().InvertedIndex.Keys.ToList();
            TaggedMessagesIds.Sort();

            return situationSet;
        }
    }
}
