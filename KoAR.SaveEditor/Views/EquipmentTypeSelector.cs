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
    public sealed class EquipmentTypeSelector : Control
    {
        public static readonly IMultiValueConverter CountConverter = new EquipmentTypeCountConverter();

        public static readonly DependencyProperty EquipmentTypeProperty = DependencyProperty.Register(nameof(EquipmentTypeSelector.EquipmentType), typeof(EquipmentType?), typeof(EquipmentTypeSelector),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty ItemsProperty = DependencyProperty.Register(nameof(EquipmentTypeSelector.Items), typeof(IEnumerable<ItemModel>), typeof(EquipmentTypeSelector));

        static EquipmentTypeSelector() => FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(EquipmentTypeSelector), new FrameworkPropertyMetadata(typeof(EquipmentTypeSelector)));

        public EquipmentType? EquipmentType
        {
            get => (EquipmentType?)this.GetValue(EquipmentTypeSelector.EquipmentTypeProperty);
            set => this.SetValue(EquipmentTypeSelector.EquipmentTypeProperty, value);
        }

        public IEnumerable<ItemModel>? Items
        {
            get => (IEnumerable<ItemModel>?)this.GetValue(EquipmentTypeSelector.ItemsProperty);
            set => this.SetValue(EquipmentTypeSelector.ItemsProperty, value);
        }

        private sealed class EquipmentTypeCountConverter : IMultiValueConverter
        {
            object IMultiValueConverter.Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
            {
                return values.Length >= 2 && values[0] is EquipmentType equipmentType && values[1] is IEnumerable<ItemModel> models
                    ? models.Count(model => model.EquipmentType == equipmentType)
                    : DependencyProperty.UnsetValue;
            }

            object[] IMultiValueConverter.ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotImplementedException();
        }
    }
}
