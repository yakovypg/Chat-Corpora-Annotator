using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace ChatCorporaAnnotator.Data.Windows.Controls
{
    [TemplatePart(Name = "PART_Content", Type = typeof(TextBlock))]
    [StyleTypedProperty(Property = "TextStyle", StyleTargetType = typeof(TextBlock))]
    internal class HighlightedTextBlock : Control
    {
        #region TextOptions

        public string Text
        {
            get => GetValue(TextProperty) as string;
            set => SetValue(TextProperty, value);
        }

        public bool IgnoreCase
        {
            get => (bool)GetValue(IgnoreCaseProperty);
            set => SetValue(IgnoreCaseProperty, value);
        }

        public string HighlightedText
        {
            get => GetValue(HighlightedTextProperty) as string;
            set => SetValue(HighlightedTextProperty, value);
        }

        public Brush HighlightBrush
        {
            get => GetValue(HighlightBrushProperty) as Brush;
            set => SetValue(HighlightBrushProperty, value);
        }

        public Style TextStyle
        {
            get => GetValue(TextStyleProperty) as Style;
            set => SetValue(TextStyleProperty, value);
        }

        #endregion

        #region DependencyProperties

        public static DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string),
            typeof(HighlightedTextBlock), new PropertyMetadata("", OnPropertyChanged));

        public static DependencyProperty IgnoreCaseProperty = DependencyProperty.Register("IgnoreCase", typeof(bool),
            typeof(HighlightedTextBlock), new PropertyMetadata(true, OnPropertyChanged));

        public static DependencyProperty HighlightedTextProperty = DependencyProperty.Register("HighlightedText", typeof(string),
            typeof(HighlightedTextBlock), new PropertyMetadata("", OnPropertyChanged));

        public static DependencyProperty HighlightBrushProperty = DependencyProperty.Register("HighlightBrush", typeof(Brush),
            typeof(HighlightedTextBlock), new PropertyMetadata(null, OnPropertyChanged));

        public static DependencyProperty TextStyleProperty = DependencyProperty.Register("TextStyle", typeof(Style),
            typeof(HighlightedTextBlock), new PropertyMetadata(null, OnPropertyChanged));

        #endregion

        public HighlightedTextBlock()
        {
            DefaultStyleKey = typeof(HighlightedTextBlock);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (!(GetTemplateChild("PART_Content") is TextBlock textBlock))
                return;

            textBlock.Inlines.Clear();

            string text = Text;
            string highlightedText = HighlightedText;

            if (IgnoreCase)
            {
                text = text.ToLower();
                highlightedText = highlightedText.ToLower();
            }

            if (highlightedText.Length == 0 || !text.Contains(highlightedText))
            {
                textBlock.Text = Text;
                return;
            }

            int currTextPos = 0;
            int highlightedTextStartPos;

            while ((highlightedTextStartPos = text.IndexOf(highlightedText, currTextPos)) >= 0)
            {
                if (currTextPos < highlightedTextStartPos)
                {
                    string firstBlockText = Text.Substring(currTextPos, highlightedTextStartPos - currTextPos);
                    var firstBlock = new Run(firstBlockText);

                    textBlock.Inlines.Add(firstBlock);
                }

                string blockText = Text.Substring(highlightedTextStartPos, highlightedText.Length);
                var block = new Run(blockText) { Background = HighlightBrush };

                textBlock.Inlines.Add(block);
                currTextPos = highlightedTextStartPos + highlightedText.Length;
            }

            if (currTextPos < text.Length - 1)
            {
                string remainingBlockText = Text.Substring(currTextPos);
                var remainingBlock = new Run(remainingBlockText);

                textBlock.Inlines.Add(remainingBlock);
            }
        }

        private void OnPropertyChanged(DependencyProperty property)
        {
            if (property is null)
                throw new ArgumentNullException(nameof(property));

            OnApplyTemplate();
        }

        private static void OnPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var control = obj as HighlightedTextBlock;

            if (obj != null)
                control.OnPropertyChanged(e.Property);
        }
    }
}
