using ChatCorporaAnnotator.Infrastructure.AppEventArgs;
using ChatCorporaAnnotator.Infrastructure.Commands;
using ChatCorporaAnnotator.Infrastructure.Extensions;
using ChatCorporaAnnotator.Models.Chat;
using ChatCorporaAnnotator.Models.Messages;
using ChatCorporaAnnotator.ViewModels.Base;
using IndexEngine.Indexes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace ChatCorporaAnnotator.ViewModels.Chat
{
    internal class SituationsViewModel : ViewModel
    {
        private readonly MainWindowViewModel _mainWindowVM;

        public List<int> TaggedMessagesIds { get; private set; }
        public ObservableCollection<Situation> Situations { get; private set; }

        private Situation _selectedSituation;
        public Situation SelectedSituation
        {
            get => _selectedSituation;
            set => SetValue(ref _selectedSituation, value);
        }

        #region AddingAndRemovingCommands

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
            SortSituations();

            OnPropertyChanged(nameof(Situations));

            _mainWindowVM.SituationsCount = SituationIndex.GetInstance().ItemCount;
        }

        public ICommand AddSituationsCommand { get; }
        public bool CanAddSituationsCommandExecute(object parameter)
        {
            return parameter is IEnumerable<Situation> || parameter is Situation;
        }
        public void OnAddSituationsCommandExecuted(object parameter)
        {
            if (!CanAddSituationsCommandExecute(parameter))
                return;

            var addingSituations = parameter is Situation
                ? new Situation[] { parameter as Situation }
                : parameter as IEnumerable<Situation>;

            if (addingSituations.IsNullOrEmpty())
                return;

            foreach (var s in addingSituations)
                Situations.Add(s);

            SortSituations();
            _mainWindowVM.SituationsCount = SituationIndex.GetInstance().ItemCount;
        }

        public ICommand RemoveSituationsCommand { get; }
        public bool CanRemoveSituationsCommandExecute(object parameter)
        {
            return parameter is IEnumerable<Situation> || parameter is Situation;
        }
        public void OnRemoveSituationsCommandExecuted(object parameter)
        {
            if (!CanRemoveSituationsCommandExecute(parameter))
                return;

            var removingSituations = parameter is Situation
                ? new Situation[] { parameter as Situation }
                : parameter as IEnumerable<Situation>;

            if (removingSituations.IsNullOrEmpty())
                return;

            foreach (var s in removingSituations)
                Situations.Remove(s);

            SortSituations();
            _mainWindowVM.SituationsCount = SituationIndex.GetInstance().ItemCount;
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

            _mainWindowVM.IsProjectChanged = true;
        }

        public ICommand CrossMergeSituationsCommand { get; }
        public bool CanCrossMergeSituationsCommandExecute(object parameter)
        {
            return false;
        }
        public void OnCrossMergeSituationsCommandExecuted(object parameter)
        {
            if (!CanCrossMergeSituationsCommandExecute(parameter))
                return;

            _mainWindowVM.IsProjectChanged = true;
        }

        public ICommand DeleteSituationCommand { get; }
        public bool CanDeleteSituationCommandExecute(object parameter)
        {
            return SelectedSituation != null;
        }
        public void OnDeleteSituationCommandExecuted(object parameter)
        {
            if (!CanDeleteSituationCommandExecute(parameter))
                return;

            var args = new TaggerEventArgs()
            {
                Id = SelectedSituation.Id,
                Tag = SelectedSituation.Header,
                MessagesIds = new List<int>()
            };

            _mainWindowVM.ChatVM.DeleteOrEditTag(args, true);
            UpdateMessagesTags();

            _mainWindowVM.IsProjectChanged = true;
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

            _mainWindowVM.IsProjectChanged = true;
        }

        #endregion

        #region InfoCommands

        public ICommand ShowSelectedSituationInfoCommand { get; }
        public bool CanShowSelectedSituationInfoCommandExecute(object parameter)
        {
            return SelectedSituation != null;
        }
        public void OnShowSelectedSituationInfoCommandExecuted(object parameter)
        {
            if (!CanShowSelectedSituationInfoCommandExecute(parameter))
                return;

            int id = SelectedSituation.Id;
            string header = SelectedSituation.Header;

            List<int> sitMsgIds = SituationIndex.GetInstance().IndexCollection[header][id];
            sitMsgIds.Sort();

            List<Tuple<int, int>> regions = new List<Tuple<int, int>>();

            int regionStart = sitMsgIds[0];

            for (int i = 1; i < sitMsgIds.Count; ++i)
            {
                if (sitMsgIds[i] - sitMsgIds[i - 1] > 1)
                {
                    var region = new Tuple<int, int>(regionStart, sitMsgIds[i - 1]);
                    regions.Add(region);

                    regionStart = sitMsgIds[i];
                }
            }

            var lastRegion = new Tuple<int, int>(regionStart, sitMsgIds[sitMsgIds.Count - 1]);
            regions.Add(lastRegion);

            var infoBuilder = new StringBuilder("Regions:\n");

            for (int i = 0; i < regions.Count; ++i)
            {
                infoBuilder.Append($"{i + 1}. {regions[i].Item1}-{regions[i].Item2}\n");
            }

            new QuickMessage(infoBuilder.ToString()).ShowInformation();
        }

        #endregion

        #region NavigationCommands

        public ICommand MoveToSelectedSituationCommand { get; }
        public bool CanMoveToSelectedSituationCommandExecute(object parameter)
        {
            return SelectedSituation != null;
        }
        public void OnMoveToSelectedSituationCommandExecuted(object parameter)
        {
            if (!CanMoveToSelectedSituationCommandExecute(parameter))
                return;

            int id = SelectedSituation.Id;
            string header = SelectedSituation.Header;

            List<int> situationMessagesIds = SituationIndex.GetInstance().IndexCollection[header][id];
            int shiftIndex = situationMessagesIds[0] - 1;

            if (shiftIndex < 0)
                shiftIndex = 0;

            _mainWindowVM.ChatVM.ShiftChatPageCommand.Execute(shiftIndex);
            _mainWindowVM.ChatVM.MainWindowVM.MemoryCleaninigTimer.CleanNow();
        }

        #endregion

        public SituationsViewModel(MainWindowViewModel mainWindowVM)
        {
            _mainWindowVM = mainWindowVM ?? throw new ArgumentNullException(nameof(mainWindowVM));

            TaggedMessagesIds = new List<int>();
            Situations = new ObservableCollection<Situation>();

            SetSituationsCommand = new RelayCommand(OnSetSituationsCommandExecuted, CanSetSituationsCommandExecute);
            AddSituationsCommand = new RelayCommand(OnAddSituationsCommandExecuted, CanAddSituationsCommandExecute);
            RemoveSituationsCommand = new RelayCommand(OnRemoveSituationsCommandExecuted, CanRemoveSituationsCommandExecute);

            MergeSituationsCommand = new RelayCommand(OnMergeSituationsCommandExecuted, CanMergeSituationsCommandExecute);
            CrossMergeSituationsCommand = new RelayCommand(OnCrossMergeSituationsCommandExecuted, CanCrossMergeSituationsCommandExecute);
            DeleteSituationCommand = new RelayCommand(OnDeleteSituationCommandExecuted, CanDeleteSituationCommandExecute);
            ChangeSituationTagCommand = new RelayCommand(OnChangeSituationTagCommandExecuted, CanChangeSituationTagCommandExecute);

            ShowSelectedSituationInfoCommand = new RelayCommand(OnShowSelectedSituationInfoCommandExecuted, CanShowSelectedSituationInfoCommandExecute);
            MoveToSelectedSituationCommand = new RelayCommand(OnMoveToSelectedSituationCommandExecuted, CanMoveToSelectedSituationCommandExecute);
        }

        #region DataMethods

        public void ClearData()
        {
            TaggedMessagesIds.Clear();
            Situations.Clear();
        }

        public void SortSituations()
        {
            var sortedData = Situations.OrderBy(t => t.Header).ThenBy(t => t.Id).ToArray();
            Situations = new ObservableCollection<Situation>(sortedData);

            OnPropertyChanged(nameof(Situations));
        }

        #endregion

        #region ViewMethods

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

                    msg.UpdateBackgroundBrush(_mainWindowVM.ChatVM.TagsVM.CurrentTagset);
                }
            }
        }

        #endregion

        #region DataGettingMethods

        private IEnumerable<Situation> GetSituations()
        {
            SituationIndex.GetInstance().ReadIndexFromDisk();

            var situationSet = new HashSet<Situation>();

            foreach (var kvp in SituationIndex.GetInstance().IndexCollection)
            {
                foreach (var situationPresenter in kvp.Value)
                {
                    int key = situationPresenter.Key;
                    string header = kvp.Key;

                    var situation = new Situation(key, header);
                    situationSet.Add(situation);
                }
            }

            TaggedMessagesIds = SituationIndex.GetInstance().InvertedIndex.Keys.ToList();
            TaggedMessagesIds.Sort();

            return situationSet;
        }

        #endregion
    }
}
