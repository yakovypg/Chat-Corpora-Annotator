﻿using ChatCorporaAnnotator.Data.Imaging;
using ChatCorporaAnnotator.Infrastructure.Extensions;
using IndexEngine;
using IndexEngine.Paths;
using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;

namespace ChatCorporaAnnotator.Models.Chat
{
    internal class ChatMessage : IChatMessage, INotifyPropertyChanged
    {
        public DynamicMessage Source { get; }

        public string Text => Source.Contents.TryGetValue(ProjectInfo.TextFieldKey, out var text)
            ? text.ToString()
            : string.Empty;

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

        public ChatMessage(DynamicMessage source)
        {
            Source = source ?? throw new ArgumentNullException(nameof(source));
            //Source.Contents[ProjectInfo.TextFieldKey] = (Source.Id + 1).ToString();
        }

        public void AddSituation(ISituation situation, IEnumerable<Tag> tagset = null)
        {
            if (situation == null)
                return;

            Source.AddSituation(situation.Header, situation.Id);

            if (!tagset.IsNullOrEmpty() && Source.Situations.Count == 1)
            {
                string tagHeader = Source.Situations.First().Key;
                Tag tag = tagset.FirstOrDefault(t => t.Header == tagHeader);

                if (tag != null)
                    BackgroundBrush = tag.BackgroundBrush;
            }
        }

        public bool RemoveSituation(string situationKey, IEnumerable<Tag> tagset = null)
        {
            if (string.IsNullOrEmpty(situationKey))
                return false;

            if (!Source.RemoveSituation(situationKey))
                return false;

            if (Source.Situations.Count == 0)
            {
                BackgroundBrush = Brushes.White;
            }
            else if (!tagset.IsNullOrEmpty())
            {
                string tagHeader = Source.Situations.First().Key;
                Tag tag = tagset.FirstOrDefault(t => t.Header == tagHeader);

                if (tag != null)
                    BackgroundBrush = tag.BackgroundBrush;
            }

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
