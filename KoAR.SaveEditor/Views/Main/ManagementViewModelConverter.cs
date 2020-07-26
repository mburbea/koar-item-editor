using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace KoAR.SaveEditor.Views.Main
{
    public sealed class ManagementViewModelConverter : IValueConverter
    {
        [return: MaybeNull]
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter is MainWindowViewModel mainWindowViewModel && value is ManagementMode mode)
            {
                return mode == ManagementMode.Inventory ? (object?)mainWindowViewModel.InventoryManager : mainWindowViewModel.StashManager;
            }
            return DependencyProperty.UnsetValue;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
