using System.Windows.Controls;

namespace ChatCorporaAnnotator.Infrastructure.Extensions.Controls
{
    internal static class ComboBoxExtensions
    {
        public static void SetItems<T>(this ComboBox comboBox, params T[] items)
        {
            SetItems(comboBox, -1, false, items);
        }

        public static void SetItems<T>(this ComboBox comboBox, int selectedIndex, bool clearPastItems, params T[] items)
        {
            if (clearPastItems)
                comboBox.Items.Clear();

            foreach (T item in items)
            {
                comboBox.Items.Add(item);
            }

            comboBox.SelectedIndex = selectedIndex;
        }
    }
}
