using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace KoAR.SaveEditor.Views.Main
{
    public sealed class ManagementViewModelConverter : IMultiValueConverter
    {
        [return: MaybeNull]
        object IMultiValueConverter.Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length >= 2 && values[0] is MainWindowViewModel viewModel && values[1] is ManagementMode mode)
            {
                return mode == ManagementMode.Inventory ? (object?)viewModel.InventoryManager : viewModel.StashManager;
            }
            return DependencyProperty.UnsetValue;
        }

        object[] IMultiValueConverter.ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
