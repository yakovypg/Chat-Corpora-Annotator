using ChatCorporaAnnotator.Infrastructure.AppEventArgs;
using ChatCorporaAnnotator.Infrastructure.Commands;
using ChatCorporaAnnotator.Infrastructure.Extensions;
using ChatCorporaAnnotator.Models.Chat;
using ChatCorporaAnnotator.Models.Messages;
using ChatCorporaAnnotator.ViewModels.Base;
using ChatCorporaAnnotator.ViewModels.Windows;
using ChatCorporaAnnotator.Views.Windows;
using IndexEngine;
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
            return Situations.Count >= 2;
        }
        public void OnMergeSituationsCommandExecuted(object parameter)
        {
            if (!CanMergeSituationsCommandExecute(parameter))
                return;

            var viewModel = new MergeSituationsWindowViewModel(Situations)
            {
                MergeSituationsFunc = MergeSituations,
                CheckDataFunc = CanMergeSituations,

                Title = "Merge situations",
                TextBetweenComboBoxes = "to",
                PerformButtonContent = "Merge",

                Hint = "You are going to perform a merge of situations. In the first box, select the situation " +
                       "from which you want to take messages. In the second box, select the situation in which the " +
                       "messages will be moved."
            };

            var window = new MergeSituationsWindow()
            {
                DataContext = viewModel
            };

            window.ShowDialog();
        }

        public ICommand CrossMergeSituationsCommand { get; }
        public bool CanCrossMergeSituationsCommandExecute(object parameter)
        {
            return Situations.Count >= 2;
        }
        public void OnCrossMergeSituationsCommandExecuted(object parameter)
        {
            if (!CanCrossMergeSituationsCommandExecute(parameter))
                return;

            var viewModel = new MergeSituationsWindowViewModel(Situations)
            {
                MergeSituationsFunc = CrossMergeSituations,
                CheckDataFunc = CanCrossMergeSituations,

                Title = "Cross-Merge situations",
                TextBetweenComboBoxes = "with",
                PerformButtonContent = "Cross-Merge",

                Hint = "You are going to perform a cross-merge of situations. In the first box, select the first " +
                       "situation. In the second box, select the second situation."
            };

            var window = new MergeSituationsWindow()
            {
                DataContext = viewModel
            };

            window.ShowDialog();
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
            return SelectedSituation != null && _mainWindowVM.ChatVM.TagsVM.SelectedTag != null;
        }
        public void OnChangeSituationTagCommandExecuted(object parameter)
        {
            if (!CanChangeSituationTagCommandExecute(parameter))
                return;

            var args = new TaggerEventArgs
            {
                Id = SelectedSituation.Id,
                Tag = SelectedSituation.Header,
                AdditionalInfo = new Dictionary<string, object>() { { "Change", _mainWindowVM.ChatVM.TagsVM.SelectedTag.Header } }
            };

            _mainWindowVM.ChatVM.DeleteOrEditTag(args, false);
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
                msg.RemoveAllSituations();

                if (TaggedMessagesIds.Contains(msg.Source.Id))
                {
                    foreach (var situationData in invertedIndex[msg.Source.Id])
                    {
                        var situation = new Situation(situationData.Value, situationData.Key);
                        msg.AddSituation(situation, _mainWindowVM.ChatVM.TagsVM.CurrentTagset);
                    }

                    msg.UpdateTagPresenter();
                    msg.UpdateBackgroundBrush(_mainWindowVM.ChatVM.TagsVM.CurrentTagset);
                }
            }
        }

        #endregion

        #region MergeSituationsMethods

        private Tuple<bool, string> CanMergeSituations(Situation first, Situation second)
        {
            string errorMessage = null;

            if (first == null || second == null)
                errorMessage = "One of the situations is null.";

            else if (first.Equals(second))
                errorMessage = "Can not merge identical situations.";

            return new Tuple<bool, string>(errorMessage is null, errorMessage);
        }

        private Tuple<bool, string> CanCrossMergeSituations(Situation first, Situation second)
        {
            string errorMessage = null;

            if (first == null || second == null)
                errorMessage = "One of the situations is null.";

            else if (first.Header == second.Header)
                errorMessage = "Can not merge situations of the same type.";

            return new Tuple<bool, string>(errorMessage is null, errorMessage);
        }

        private bool MergeSituations(Situation first, Situation second)
        {
            List<int> firstSitMsgIds = SituationIndex.GetInstance().IndexCollection[first.Header][first.Id].ToList();
            List<int> secondSitMsgIds = SituationIndex.GetInstance().IndexCollection[second.Header][second.Id].ToList();

            SituationIndex.GetInstance().MergeItems(first.Header, first.Id, second.Header, second.Id);

            _mainWindowVM.ChatVM.ShiftSituationsIds(first.Header, first.Id + 1);

            foreach (var id in firstSitMsgIds)
            {
                MessageContainer.UpdateTagsInDynamicMessage(id, 0);
            }

            foreach (var id in secondSitMsgIds)
            {
                MessageContainer.UpdateTagsInDynamicMessage(id, 0);
            }

            Situations.Remove(first);

            UpdateMessagesTags();
            _mainWindowVM.IsProjectChanged = true;

            return true;
        }

        private bool CrossMergeSituations(Situation first, Situation second)
        {
            SituationIndex.GetInstance().CrossMergeItems(first.Header, first.Id, second.Header, second.Id);

            List<int> firstSitMsgIds = SituationIndex.GetInstance().IndexCollection[first.Header][first.Id];
            List<int> secondSitMsgIds = SituationIndex.GetInstance().IndexCollection[second.Header][second.Id];

            foreach (var id in firstSitMsgIds)
            {
                MessageContainer.InsertTagsInDynamicMessage(id, 0);
            }

            foreach (var id in secondSitMsgIds)
            {
                MessageContainer.InsertTagsInDynamicMessage(id, 0);
            }

            var firstSitMsgIdsClone = new List<int>(firstSitMsgIds);
            var secondSitMsgIdsClone = new List<int>(secondSitMsgIds);

            firstSitMsgIds.AddRange(secondSitMsgIdsClone);
            secondSitMsgIds.AddRange(firstSitMsgIdsClone);

            firstSitMsgIds.Sort();
            secondSitMsgIds.Sort();

            UpdateMessagesTags();
            _mainWindowVM.IsProjectChanged = true;

            return true;
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
