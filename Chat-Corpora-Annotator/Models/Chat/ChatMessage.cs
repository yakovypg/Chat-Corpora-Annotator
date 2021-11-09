using ChatCorporaAnnotator.Data.Imaging;
using ChatCorporaAnnotator.Infrastructure.Extensions;
using IndexEngine.Containers;
using IndexEngine.Data.Paths;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Media;

namespace ChatCorporaAnnotator.Models.Chat
{
    internal class ChatMessage : IChatMessage, INotifyPropertyChanged
    {
        public DynamicMessage Source { get; }

        public string Text => Source.Contents.TryGetValue(ProjectInfo.TextFieldKey, out var text)
            ? text.ToString()
            : string.Empty;

        public bool IsFake { get; }

        public string TagsPresenter
        {
            get
            {
                if (Source.Situations.Count == 0)
                    return string.Empty;

                var presenter = new StringBuilder();

                foreach (var situation in Source.Situations)
                {
                    presenter.Append(situation.Key + " ");
                }

                return presenter.ToString().TrimEnd();
            }
        }

        public Brush SenderColor
        {
            get
            {
                string senderKey = ProjectInfo.SenderFieldKey;

                if (!Source.Contents.TryGetValue(senderKey, out var user))
                    return Brushes.Black;

                string userKey = user.ToString();

                return ProjectInfo.Data.UserColors.TryGetValue(userKey, out var color)
                    ? new SolidColorBrush(ColorTransformer.ToWindowsColor(color))
                    : Brushes.Black;
            }
        }

        private Brush _backgroundBrush = Brushes.White;
        public Brush BackgroundBrush
        {
            get => _backgroundBrush;
            set
            {
                _backgroundBrush = value ?? Brushes.White;
                OnPropertyChanged(nameof(BackgroundBrush));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string property = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        /// <summary>
        /// Initializes a new instance of the ChatMessage class which is a fake message.
        /// </summary>
        public ChatMessage()
        {
            IsFake = true;
            Source = new DynamicMessage(new List<string>(), new List<string>(), string.Empty, -1);
        }

        /// <summary>
        /// Initializes a new instance of the ChatMessage class.
        /// </summary>
        /// <param name="source">The message containing data.</param>
        public ChatMessage(DynamicMessage source)
        {
            Source = source ?? throw new ArgumentNullException(nameof(source));
            //Source.Contents[ProjectInfo.TextFieldKey] = (Source.Id + 0).ToString();
        }

        public void AddSituation(ISituation situation, IEnumerable<Tag> tagset = null)
        {
            if (situation == null)
                return;

            if (Source.Situations.ContainsKey(situation.Header))
                return;

            Source.AddSituation(situation.Header, situation.Id);

            UpdateBackgroundBrush(tagset);
            OnPropertyChanged(nameof(TagsPresenter));
        }

        public void RemoveAllSituations()
        {
            Source.RemoveAllSituations();

            UpdateBackgroundBrush();
            OnPropertyChanged(nameof(TagsPresenter));
        }

        public bool RemoveSituation(string situationKey, IEnumerable<Tag> tagset = null)
        {
            if (string.IsNullOrEmpty(situationKey))
                return false;

            if (!Source.RemoveSituation(situationKey))
                return false;

            UpdateBackgroundBrush(tagset);
            OnPropertyChanged(nameof(TagsPresenter));

            return true;
        }

        public bool TryGetSender(out string sender)
        {
            if (!Source.Contents.TryGetValue(ProjectInfo.SenderFieldKey, out object senderObj))
            {
                sender = null;
                return false;
            }

            sender = senderObj?.ToString() ?? string.Empty;
            return true;
        }

        public bool TryGetSentDate(out DateTime sentDate)
        {
            if (!Source.Contents.TryGetValue(ProjectInfo.DateFieldKey, out object dateObj))
            {
                sentDate = DateTime.MinValue;
                return false;
            }

            try
            {
                sentDate = DateTime.Parse(dateObj.ToString());
                return true;
            }
            catch
            {
                sentDate = DateTime.MinValue;
                return false;
            }
        }

        public void UpdateBackgroundBrush(IEnumerable<Tag> tagset = null)
        {
            if (Source.Situations.Count == 0)
            {
                BackgroundBrush = Brushes.White;
                return;
            }

            if (tagset.IsNullOrEmpty())
                return;

            string tagHeader = Source.Situations.First().Key;
            Tag tag = tagset.FirstOrDefault(t => t.Header == tagHeader);

            if (tag != null)
                BackgroundBrush = tag.BackgroundBrush;
        }

        public void UpdateTagPresenter()
        {
            OnPropertyChanged(nameof(TagsPresenter));
        }

        public override string ToString()
        {
            return Source.Contents.TryGetValue(ProjectInfo.TextFieldKey, out object text)
                ? text?.ToString()
                : base.ToString();
        }

        public override bool Equals(object obj)
        {
            return obj is ChatMessage other && Source.Id == other.Source.Id;
        }

        public override int GetHashCode()
        {
            return Source.Id;
        }
    }
}
