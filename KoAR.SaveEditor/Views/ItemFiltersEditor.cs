using System.Windows;
using System.Windows.Controls;

namespace KoAR.SaveEditor.Views
{
    public sealed class ItemFiltersEditor : Control
    {
        public static readonly DependencyProperty ItemFiltersProperty = DependencyProperty.Register(nameof(ItemFiltersEditor.ItemFilters), typeof(ItemFilters), typeof(ItemFiltersEditor));

        static ItemFiltersEditor() => FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(ItemFiltersEditor), new FrameworkPropertyMetadata(typeof(ItemFiltersEditor)));

        public ItemFilters? ItemFilters
        {
            get => (ItemFilters?)this.GetValue(ItemFiltersEditor.ItemFiltersProperty);
            set => this.SetValue(ItemFiltersEditor.ItemFiltersProperty, value);
        }
    }
}
