using ChatCorporaAnnotator.Data.Imaging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;

namespace ChatCorporaAnnotator.Models.Chat
{
    internal class Tag : ITag, INotifyPropertyChanged, ICloneable, IComparable<Tag>
    {
        public Brush BackgroundBrush => new SolidColorBrush(ColorTransformer.ToWindowsColor(BackgroundColor));

        private string _header;
        public string Header
        {
            get => _header;
            set
            {
                _header = value;
                OnPropertyChanged(nameof(Header));
            }
        }

        private System.Drawing.Color _backgroundColor;
        public System.Drawing.Color BackgroundColor
        {
            get => _backgroundColor;
            set
            {
                _backgroundColor = value;

                OnPropertyChanged(nameof(BackgroundColor));
                OnPropertyChanged(nameof(BackgroundBrush));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string property = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        public Tag(string header) : this(header, System.Drawing.Color.Transparent)
        {
        }

        public Tag(string header, System.Drawing.Color color)
        {
            Header = header;
            BackgroundColor = color;
        }

        public object Clone()
        {
            return new Tag(Header, BackgroundColor);
        }

        public int CompareTo(Tag other)
        {
            return Header.CompareTo(other?.Header);
        }

        public override string ToString()
        {
            return Header;
        }

        public override bool Equals(object obj)
        {
            return obj is Tag other &&
                   Header == other.Header &&
                   BackgroundColor.Equals(other.BackgroundColor);
        }

        public override int GetHashCode()
        {
            int hashCode = -1869182061;

            hashCode = hashCode * -1521134295 + EqualityComparer<Brush>.Default.GetHashCode(BackgroundBrush);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(_header);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Header);
            hashCode = hashCode * -1521134295 + _backgroundColor.GetHashCode();
            hashCode = hashCode * -1521134295 + BackgroundColor.GetHashCode();

            return hashCode;
        }
    }
}
