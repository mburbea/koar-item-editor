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
    public sealed class EquipmentCategorySelector : Control
    {
        public static readonly DependencyProperty CategoryProperty = DependencyProperty.Register(nameof(EquipmentCategorySelector.Category), typeof(EquipmentCategory?), typeof(EquipmentCategorySelector),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly IMultiValueConverter CountConverter = new CategoryCountConverter();

        public static readonly DependencyProperty ItemsProperty = DependencyProperty.Register(nameof(EquipmentCategorySelector.Items), typeof(IEnumerable<ItemModel>), typeof(EquipmentCategorySelector));

        static EquipmentCategorySelector() => FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(EquipmentCategorySelector), new FrameworkPropertyMetadata(typeof(EquipmentCategorySelector)));

        public EquipmentCategory? Category
        {
            get => (EquipmentCategory?)this.GetValue(EquipmentCategorySelector.CategoryProperty);
            set => this.SetValue(EquipmentCategorySelector.CategoryProperty, value);
        }

        public IEnumerable<ItemModel>? Items
        {
            get => (IEnumerable<ItemModel>?)this.GetValue(EquipmentCategorySelector.ItemsProperty);
            set => this.SetValue(EquipmentCategorySelector.ItemsProperty, value);
        }

        private sealed class CategoryCountConverter : IMultiValueConverter
        {
            object IMultiValueConverter.Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
            {
                return values.Length >= 2 && values[0] is EquipmentCategory category && values[1] is IEnumerable<ItemModel> models
                    ? models.Count(model => model.Category == category)
                    : DependencyProperty.UnsetValue;
            }

            object[] IMultiValueConverter.ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotImplementedException();
        }
    }
}
