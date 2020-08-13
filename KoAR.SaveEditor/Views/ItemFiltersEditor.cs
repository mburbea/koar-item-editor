using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using KoAR.Core;

namespace KoAR.SaveEditor.Views
{
    public sealed class ItemFiltersEditor : Control
    {
        public static readonly IMultiValueConverter ItemCountConverter = new FilteredItemCountConverter();

        public static readonly DependencyProperty ItemFiltersProperty = DependencyProperty.Register(nameof(ItemFiltersEditor.ItemFilters), typeof(ItemFilters), typeof(ItemFiltersEditor));

        public static readonly DependencyProperty ItemsProperty = DependencyProperty.Register(nameof(ItemFiltersEditor.Items), typeof(IReadOnlyCollection<ItemModelBase>), typeof(ItemFiltersEditor));

        static ItemFiltersEditor() => FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(ItemFiltersEditor), new FrameworkPropertyMetadata(typeof(ItemFiltersEditor)));

        public ItemFilters? ItemFilters
        {
            get => (ItemFilters?)this.GetValue(ItemFiltersEditor.ItemFiltersProperty);
            set => this.SetValue(ItemFiltersEditor.ItemFiltersProperty, value);
        }

        public IReadOnlyCollection<ItemModelBase>? Items
        {
            get => (IReadOnlyCollection<ItemModelBase>?)this.GetValue(ItemFiltersEditor.ItemsProperty);
            set => this.SetValue(ItemFiltersEditor.ItemsProperty, value);
        }

        private sealed class FilteredItemCountConverter : IMultiValueConverter
        {
            object IMultiValueConverter.Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
            {
                return values.Length >= 6 &&
                    values[0] is IEnumerable<ItemModelBase> items &&
                    values[1] is EquipmentCategory category &&
                    values[2] is Rarity rarity &&
                    values[3] is Element element &&
                    values[4] is ArmorType armorType &&
                    values[5] is string itemName
                    ? items.Count(item => ItemFilters.Match(item, category, rarity, element, armorType, itemName))
                    : DependencyProperty.UnsetValue;
            }

            object[] IMultiValueConverter.ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            {
                throw new NotImplementedException();
            }
        }
    }
}
