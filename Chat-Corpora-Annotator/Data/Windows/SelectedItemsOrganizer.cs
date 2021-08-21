using ChatCorporaAnnotator.Models;
using System;
using System.Collections;
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

            if (e == null)
                return;

            foreach (var item in e.RemovedItems)
            {
                var removingItem = (T)item;

                if (source.Contains(removingItem))
                    source.Remove(removingItem);
            }

            foreach (var item in e.AddedItems)
            {
                var addingItem = (T)item;

                if (!source.Contains(addingItem))
                    source.Add(addingItem);
            }
        }

        public void InvertAddedItemsSelection<T>(SelectionChangedEventArgs e) where T : ISelectable
        {
            InvertItemsSelection<T>(e?.AddedItems);
        }

        public void InvertRemovedItemsSelection<T>(SelectionChangedEventArgs e) where T : ISelectable
        {
            InvertItemsSelection<T>(e?.RemovedItems);
        }

        public void InvertItemsSelection<T>(SelectionChangedEventArgs e) where T : ISelectable
        {
            InvertAddedItemsSelection<T>(e);
            InvertRemovedItemsSelection<T>(e);
        }

        public void InvertItemsSelection<T>(IEnumerable items) where T : ISelectable
        {
            if (items == null)
                return;

            foreach (var item in items)
            {
                T currItem = (T)item;
                currItem.IsSelected = !currItem.IsSelected;
            }
        }

        public void ChangeItemsSelection<T>(SelectionChangedEventArgs e) where T : ISelectable
        {
            SelectAddedItems<T>(e);
            DeselectRemovedItems<T>(e);
        }

        public void SelectAddedItems<T>(SelectionChangedEventArgs e) where T : ISelectable
        {
            if (e == null)
                return;

            foreach (var item in e.AddedItems)
            {
                T addedItem = (T)item;

                if (!addedItem.IsSelected)
                    addedItem.IsSelected = true;
            }
        }

        public void DeselectRemovedItems<T>(SelectionChangedEventArgs e) where T : ISelectable
        {
            if (e == null)
                return;

            foreach (var item in e.RemovedItems)
            {
                T removedItem = (T)item;

                if (removedItem.IsSelected)
                    removedItem.IsSelected = false;
            }
        }

        public void SelectAll<T>(IEnumerable<T> items) where T : ISelectable
        {
            foreach (var item in items)
            {
                if (!item.IsSelected)
                    item.IsSelected = true;
            }
        }

        public void DeselectAll<T>(IEnumerable<T> items) where T : ISelectable
        {
            foreach (var item in items)
            {
                if (item.IsSelected)
                    item.IsSelected = false;
            }
        }
    }
}
