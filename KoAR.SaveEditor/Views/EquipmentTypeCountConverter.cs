using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using KoAR.Core;

namespace KoAR.SaveEditor.Views
{
    public sealed class EquipmentTypeCountConverter : IMultiValueConverter
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
