using ChatCorporaAnnotator.Infrastructure.Extensions;
using ChatCorporaAnnotator.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Controls;

namespace ChatCorporaAnnotator.Data.Windows.UI
{
    internal static class SelectedItemsOrganizer
    {
        /// <summary>
        /// Changes the collection of selected items. 
        /// </summary>
        /// <typeparam name="T">The type of source items.</typeparam>
        /// <param name="source">The collection to which the elements will be written or deleted from.</param>
        /// <param name="e">The source event args.</param>
        /// <param name="ignoreItemsPredicate">The predicate that removes some elements from the list of added elements (e.AddedItems).</param>
        /// <remarks>The type of removed and added items must match with T (e.RemovedItems and e.AddedItems).</remarks>
        public static void ChangeSelectedItems<T>(IList<T>? source, SelectionChangedEventArgs? e, Predicate<T>? ignoreItemsPredicate = null)
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

            List<T> addedItems = e.AddedItems.ToGenericList<T>();

            if (ignoreItemsPredicate != null)
                addedItems.RemoveAll(t => ignoreItemsPredicate(t));

            foreach (var item in addedItems)
            {
                if (!source.Contains(item))
                    source.Add(item);
            }
        }

        public static void InvertAddedItemsSelection<T>(SelectionChangedEventArgs? e) where T : ISelectable
        {
            InvertItemsSelection<T>(e?.AddedItems);
        }

        public static void InvertRemovedItemsSelection<T>(SelectionChangedEventArgs? e) where T : ISelectable
        {
            InvertItemsSelection<T>(e?.RemovedItems);
        }

        public static void InvertItemsSelection<T>(SelectionChangedEventArgs? e) where T : ISelectable
        {
            InvertAddedItemsSelection<T>(e);
            InvertRemovedItemsSelection<T>(e);
        }

        public static void InvertItemsSelection<T>(IEnumerable? items) where T : ISelectable
        {
            if (items == null)
                return;

            foreach (var item in items)
            {
                T currItem = (T)item;
                currItem.IsSelected = !currItem.IsSelected;
            }
        }

        public static void ChangeItemsSelection<T>(SelectionChangedEventArgs? e) where T : ISelectable
        {
            SelectAddedItems<T>(e);
            DeselectRemovedItems<T>(e);
        }

        public static void SelectAddedItems<T>(SelectionChangedEventArgs? e) where T : ISelectable
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

        public static void DeselectRemovedItems<T>(SelectionChangedEventArgs? e) where T : ISelectable
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

        public static void SelectAll<T>(IEnumerable<T>? items) where T : ISelectable
        {
            if (items == null)
                return;

            foreach (var item in items)
            {
                if (!item.IsSelected)
                    item.IsSelected = true;
            }
        }

        public static void DeselectAll<T>(IEnumerable<T>? items) where T : ISelectable
        {
            if (items == null)
                return;

            foreach (var item in items)
            {
                if (item.IsSelected)
                    item.IsSelected = false;
            }
        }
    }
}
