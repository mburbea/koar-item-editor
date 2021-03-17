using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading;
using System.Windows.Data;

namespace KoAR.SaveEditor.Constructs
{
    /// <summary>
    /// A class which works similar to <see cref="ObservableCollection{T}"/> that supports pausing the events.
    /// Also contains a constructor specifically designed to wrap an underlying list as opposed to copying elements from it.
    /// </summary>
    /// <typeparam name="T">Type of the items in the collection.</typeparam>
    public sealed class NotifyingCollection<T> : Collection<T>, INotifyCollectionChanged, INotifyPropertyChanged
    {
        private static readonly PropertyChangedEventArgs _countArgs = new(nameof(IList<T>.Count));
        private static readonly PropertyChangedEventArgs _indexerArgs = new(Binding.IndexerName);
        private static readonly NotifyCollectionChangedEventArgs _resetArgs = new(NotifyCollectionChangedAction.Reset);

        private int _pauseCount;

        public NotifyingCollection()
        {
        }

        public NotifyingCollection(IList<T> items)
            : base(items)
        {
        }

        public event NotifyCollectionChangedEventHandler? CollectionChanged;

        public event PropertyChangedEventHandler? PropertyChanged;

        public IDisposable CreatePauseEventsScope()
        {
            Interlocked.Increment(ref this._pauseCount);
            return new Disposable(this.DecrementPause);
        }

        public void OnReset()
        {
            this.OnCollectionChanged(NotifyingCollection<T>._resetArgs);
            this.OnPropertyChanged(NotifyingCollection<T>._countArgs);
            this.OnPropertyChanged(NotifyingCollection<T>._indexerArgs);
        }

        protected override void ClearItems()
        {
            if (this.Items.Count == 0)
            {
                return;
            }
            base.ClearItems();
            this.OnReset();
        }

        protected override void InsertItem(int index, T item)
        {
            base.InsertItem(index, item);
            this.OnCollectionChanged(new(NotifyCollectionChangedAction.Add, item, index));
            this.OnPropertyChanged(NotifyingCollection<T>._countArgs);
            this.OnPropertyChanged(NotifyingCollection<T>._indexerArgs);
        }

        protected override void RemoveItem(int index)
        {
            T oldItem = this.Items[index];
            base.RemoveItem(index);
            this.OnCollectionChanged(new(NotifyCollectionChangedAction.Remove, oldItem, index));
            this.OnPropertyChanged(NotifyingCollection<T>._countArgs);
            this.OnPropertyChanged(NotifyingCollection<T>._indexerArgs);
        }

        protected override void SetItem(int index, T item)
        {
            T oldItem = this.Items[index];
            if (EqualityComparer<T>.Default.Equals(item, oldItem))
            {
                return;
            }
            base.SetItem(index, item);
            this.OnCollectionChanged(new(NotifyCollectionChangedAction.Replace, item, oldItem, index));
            this.OnPropertyChanged(NotifyingCollection<T>._indexerArgs);
        }

        private void DecrementPause()
        {
            if (Interlocked.Decrement(ref this._pauseCount) == 0)
            {
                this.OnReset();
            }
        }

        private void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (this._pauseCount == 0)
            {
                this.CollectionChanged?.Invoke(this, e);
            }
        }

        private void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (this._pauseCount == 0)
            {
                this.PropertyChanged?.Invoke(this, e);
            }
        }
    }
}
