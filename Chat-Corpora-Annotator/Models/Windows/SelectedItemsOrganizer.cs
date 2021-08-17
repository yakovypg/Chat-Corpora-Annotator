using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace ChatCorporaAnnotator.Models.Windows
{
    internal class SelectedItemsOrganizer
    {
        public void ChangeSelectedItems(IList<object> source, SelectionChangedEventArgs e)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (e.RemovedItems != null)
            {
                foreach (var item in e.RemovedItems)
                {
                    if (source.Contains(item))
                        source.Remove(item);
                }
            }

            if (e.AddedItems != null)
            {
                foreach (var item in e.AddedItems)
                {
                    if (!source.Contains(item))
                        source.Add(item);
                }
            }
        }
    }
}
