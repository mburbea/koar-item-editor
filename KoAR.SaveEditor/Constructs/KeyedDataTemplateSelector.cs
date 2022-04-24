using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;

namespace KoAR.SaveEditor.Constructs;

public sealed class KeyedDataTemplateSelector : DataTemplateSelector, IDictionary
{
    private readonly Dictionary<object, DataTemplate> _dictionary = new();

    public int Count => this._dictionary.Count;

    bool IDictionary.IsFixedSize => false;

    bool IDictionary.IsReadOnly => false;

    bool ICollection.IsSynchronized => false;

    public IReadOnlyCollection<object> Keys => this._dictionary.Keys;

    ICollection IDictionary.Keys => this._dictionary.Keys;

    object ICollection.SyncRoot => this._dictionary;

    ICollection IDictionary.Values => this._dictionary.Values;

    [MaybeNull]
    public DataTemplate this[object key]
    {
        get => this.GetTemplate(key);
        set => this._dictionary[key] = value;
    }

    [MaybeNull]
    object? IDictionary.this[object key]
    {
        get => this.GetTemplate(key);
        set => this._dictionary[key] = (DataTemplate)value!;
    }

    public void Add(object key, DataTemplate value) => this._dictionary.Add(key, value);

    void IDictionary.Add(object key, object? value) => this._dictionary.Add(key, (DataTemplate)value!);

    public void Clear() => this._dictionary.Clear();

    public bool Contains(object key) => this._dictionary.ContainsKey(key);

    void ICollection.CopyTo(Array array, int index) => ((ICollection)this._dictionary).CopyTo(array, index);

    IEnumerator IEnumerable.GetEnumerator() => this._dictionary.GetEnumerator();

    IDictionaryEnumerator IDictionary.GetEnumerator() => this._dictionary.GetEnumerator();

    public void Remove(object key) => this._dictionary.Remove(key);

    [return: MaybeNull]
    public override DataTemplate SelectTemplate(object item, DependencyObject container) => this.GetTemplate(item);

    private DataTemplate? GetTemplate(object key) => key != null ? this._dictionary.GetValueOrDefault(key) : null;
}
