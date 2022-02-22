using ChatCorporaAnnotator.Data.Windows.Controls;
using ChatCorporaAnnotator.Infrastructure.Extensions;
using IndexEngine.Containers;
using IndexEngine.Data.Paths;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace ChatCorporaAnnotator.Models.Chat
{
    internal class ChatColumnCreator
    {
        public const string TAG_COLUMN_HEADER = "Tag";

        public double TextFontSize { get; set; }
        public Thickness TextPadding { get; set; }

        public ChatColumnCreator() : this(14.0, new Thickness(5, 3, 5, 3)) 
        {
        }

        public ChatColumnCreator(double textFontSize, Thickness textPadding)
        {
            TextFontSize = textFontSize;
            TextPadding = textPadding;
        }

        public DataGridTemplateColumn[] GenerateChatColumns(List<string> selectedFields, bool insertTagColumn, bool makeTextColumnHighlightable)
        {
            if (selectedFields.IsNullOrEmpty())
                return new DataGridTemplateColumn[0];

            var fields = new List<string>(selectedFields);

            if (fields.Remove(ProjectInfo.TextFieldKey))
                fields.Insert(0, ProjectInfo.TextFieldKey);

            if (fields.Remove(ProjectInfo.DateFieldKey))
                fields.Insert(0, ProjectInfo.DateFieldKey);

            if (fields.Remove(ProjectInfo.SenderFieldKey))
                fields.Insert(0, ProjectInfo.SenderFieldKey);

            if (insertTagColumn)
                fields.Insert(0, TAG_COLUMN_HEADER);

            var columns = new DataGridTemplateColumn[fields.Count];

            for (int i = 0; i < fields.Count; ++i)
            {
                string currField = fields[i];

                var column = currField == ProjectInfo.TextFieldKey && makeTextColumnHighlightable
                    ? CreateHighlightedChatColumn(currField, string.Empty, false, Brushes.Black)
                    : CreateDefaultChatColumn(currField);

                columns[i] = column;
            }

            return columns;
        }

        public DataGridTemplateColumn CreateDefaultChatColumn(string fieldKey)
        {
            var textBlockFactory = new FrameworkElementFactory(typeof(TextBlock));
            textBlockFactory.SetValue(TextBlock.PaddingProperty, TextPadding);
            textBlockFactory.SetValue(TextBlock.FontSizeProperty, TextFontSize);
            textBlockFactory.SetValue(TextBlock.TextWrappingProperty, TextWrapping.Wrap);

            if (fieldKey != TAG_COLUMN_HEADER)
                textBlockFactory.SetBinding(TextBlock.TextProperty, new Binding($"Source.Contents[{fieldKey}]"));
            else
                textBlockFactory.SetBinding(TextBlock.TextProperty, new Binding($"TagsPresenter"));

            if (fieldKey == ProjectInfo.SenderFieldKey)
                textBlockFactory.SetBinding(TextBlock.ForegroundProperty, new Binding($"SenderColor"));

            var columnDataTemplate = new DataTemplate(typeof(DynamicMessage))
            {
                VisualTree = textBlockFactory
            };

            var column = new DataGridTemplateColumn()
            {
                Header = fieldKey,
                CellTemplate = columnDataTemplate
            };

            return column;
        }

        public DataGridTemplateColumn CreateHighlightedChatColumn(string fieldKey, string highlightText, bool ignoreCase, Brush highlightBrush)
        {
            var textBlockFactory = new FrameworkElementFactory(typeof(HighlightedTextBlock));

            textBlockFactory.SetValue(Control.PaddingProperty, TextPadding);
            textBlockFactory.SetValue(Control.FontSizeProperty, TextFontSize);
            textBlockFactory.SetValue(HighlightedTextBlock.TextWrappingProperty, TextWrapping.Wrap);

            textBlockFactory.SetValue(HighlightedTextBlock.IgnoreCaseProperty, ignoreCase);
            textBlockFactory.SetValue(HighlightedTextBlock.HighlightBrushProperty, highlightBrush);
            textBlockFactory.SetValue(HighlightedTextBlock.HighlightedTextProperty, highlightText);

            if (fieldKey != TAG_COLUMN_HEADER)
                textBlockFactory.SetBinding(HighlightedTextBlock.TextProperty, new Binding($"Source.Contents[{fieldKey}]"));

            if (fieldKey == ProjectInfo.SenderFieldKey)
                textBlockFactory.SetBinding(Control.ForegroundProperty, new Binding($"SenderColor"));

            var columnDataTemplate = new DataTemplate(typeof(DynamicMessage))
            {
                VisualTree = textBlockFactory
            };

            var column = new DataGridTemplateColumn()
            {
                Header = fieldKey,
                CellTemplate = columnDataTemplate
            };

            return column;
        }
    }
}
