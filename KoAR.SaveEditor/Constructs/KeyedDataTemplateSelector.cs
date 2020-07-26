﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;

namespace KoAR.SaveEditor.Constructs
{
    public sealed class KeyedDataTemplateSelector : DataTemplateSelector, IDictionary
    {
        private readonly Dictionary<object, DataTemplate> _dictionary = new Dictionary<object, DataTemplate>();

        public KeyedDataTemplateSelector()
        {
        }

        public int Count => this._dictionary.Count;

        public bool IsFixedSize => false;

        public bool IsReadOnly => false;

        public bool IsSynchronized => false;

        public ICollection Keys => ((IDictionary)this._dictionary).Keys;

        public object SyncRoot => ((ICollection)this._dictionary).SyncRoot;

        public ICollection Values => ((IDictionary)this._dictionary).Values;

        [MaybeNull]
        public object this[object key]
        {
            get => this._dictionary.TryGetValue(key, out DataTemplate? value) ? value : default;
            set => ((IDictionary)this._dictionary)[key] = value;
        }

        public void Add(object key, object value) => ((IDictionary)this._dictionary).Add(key, value);

        public void Clear() => this._dictionary.Clear();

        public bool Contains(object key) => this._dictionary.ContainsKey(key);

        public void CopyTo(Array array, int index) => ((ICollection)this._dictionary).CopyTo(array, index);

        public IDictionaryEnumerator GetEnumerator() => ((IDictionary)this._dictionary).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)this._dictionary).GetEnumerator();

        public void Remove(object key) => this._dictionary.Remove(key);

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            return this._dictionary.TryGetValue(item, out DataTemplate? value) ? value : new DataTemplate();
        }
    }
}
