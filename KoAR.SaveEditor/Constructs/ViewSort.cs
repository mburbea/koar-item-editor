using System;
using System.Collections;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;

namespace KoAR.SaveEditor.Constructs
{
    public static class ViewSort
    {
        public static readonly DependencyProperty ComparerProperty = DependencyProperty.RegisterAttached("Comparer", typeof(IComparer), typeof(ViewSort),
            new(ViewSort.ComparerProperty_PropertyChanged));

        private static readonly DependencyPropertyDescriptor _viewDescriptor = DependencyPropertyDescriptor.FromProperty(CollectionViewSource.ViewProperty, typeof(CollectionViewSource));

        public static IComparer? GetComparer(CollectionViewSource viewSource) => (IComparer?)viewSource?.GetValue(ViewSort.ComparerProperty);

        public static void SetComparer(CollectionViewSource source, IComparer? value) => source?.SetValue(ViewSort.ComparerProperty, value);

        private static void ComparerProperty_PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not CollectionViewSource source)
            {
                return;
            }
            if (e.OldValue == null)
            {
                ViewSort._viewDescriptor.AddValueChanged(source, ViewSort.Source_ViewChanged);
            }
            else if (e.NewValue == null)
            {
                ViewSort._viewDescriptor.RemoveValueChanged(source, ViewSort.Source_ViewChanged);
            }
            ViewSort.TrySetComparer(source, (IComparer?)e.NewValue);
        }

        private static void Source_ViewChanged(object? sender, EventArgs e)
        {
            CollectionViewSource source = (CollectionViewSource)sender!;
            ViewSort.TrySetComparer(source, ViewSort.GetComparer(source));
        }

        private static void TrySetComparer(CollectionViewSource source, IComparer? comparer)
        {
            if (source.View is ListCollectionView listView)
            {
                listView.CustomSort = comparer;
            }
        }
    }
}
