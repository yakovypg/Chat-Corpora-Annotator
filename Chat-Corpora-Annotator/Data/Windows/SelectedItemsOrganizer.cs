using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace ChatCorporaAnnotator.Data.Windows
{
    internal class SelectedItemsOrganizer
    {
        /// <summary>
        /// Changes the collection of selected items. 
        /// </summary>
        /// <typeparam name="T">The type of source items.</typeparam>
        /// <param name="source">The collection to which the elements will be written or deleted from.</param>
        /// <param name="e">The source event args.</param>
        /// <remarks>The type of removed and added items must match with T (e.RemovedItems and e.AddedItems).</remarks>
        public void ChangeSelectedItems<T>(IList<T> source, SelectionChangedEventArgs e)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (e?.RemovedItems != null)
            {
                foreach (var item in e.RemovedItems)
                {
                    var removingItem = (T)item;

                    if (source.Contains(removingItem))
                        source.Remove(removingItem);
                }
            }

            if (e?.AddedItems != null)
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
