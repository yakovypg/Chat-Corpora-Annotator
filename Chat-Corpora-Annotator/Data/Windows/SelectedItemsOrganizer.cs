using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace ChatCorporaAnnotator.Data.Windows
{
    internal class SelectedItemsOrganizer
    {
        public void ChangeSelectedItems<T>(IList<T> source, SelectionChangedEventArgs e)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (e.RemovedItems != null)
            {
                foreach (var item in e.RemovedItems)
                {
                    var removingItem = (T)item;

                    if (source.Contains(removingItem))
                        source.Remove(removingItem);
                }
            }

            if (e.AddedItems != null)
            {
                foreach (var item in e.AddedItems)
                {
                    var addingItem = (T)item;

                    if (!source.Contains(addingItem))
                        source.Add(addingItem);
                }
            }
        }
    }
}
