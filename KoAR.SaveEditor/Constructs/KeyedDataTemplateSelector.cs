using System;
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

        public int Count => this._dictionary.Count;

        bool IDictionary.IsFixedSize => false;

        bool IDictionary.IsReadOnly => false;

        bool ICollection.IsSynchronized => false;

        ICollection IDictionary.Keys => ((IDictionary)this._dictionary).Keys;

        public IReadOnlyCollection<object> Keys => this._dictionary.Keys;

        object ICollection.SyncRoot => ((ICollection)this._dictionary).SyncRoot;

        ICollection IDictionary.Values => ((IDictionary)this._dictionary).Values;

        public IReadOnlyCollection<DataTemplate> Values => this._dictionary.Values;

        [MaybeNull]
        public DataTemplate this[object key]
        {
            get => this.GetDataTemplate(key);
            set => this._dictionary[key] = value;
        }

        [MaybeNull]
        object IDictionary.this[object key]
        {
            get => this.GetDataTemplate(key);
            set => ((IDictionary)this._dictionary)[key] = value;
        }

        void IDictionary.Add(object key, object value) => ((IDictionary)this._dictionary).Add(key, value);

        public void Clear() => this._dictionary.Clear();

        public bool Contains(object key) => this._dictionary.ContainsKey(key);

        void ICollection.CopyTo(Array array, int index) => ((ICollection)this._dictionary).CopyTo(array, index);

        IDictionaryEnumerator IDictionary.GetEnumerator() => ((IDictionary)this._dictionary).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)this._dictionary).GetEnumerator();

        public void Remove(object key) => this._dictionary.Remove(key);

        [return: MaybeNull]
        public override DataTemplate SelectTemplate(object item, DependencyObject container) => this.GetDataTemplate(item);

        private DataTemplate? GetDataTemplate(object item)
        {
            return item != null && this._dictionary.TryGetValue(item, out DataTemplate? dataTemplate)
                ? dataTemplate
                : null;
        }
    }
}
