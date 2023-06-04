using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace KoAR.SaveEditor.Views.Main;

public sealed class ManagerViewModelConverter : IMultiValueConverter
{
    [return: MaybeNull]
    object IMultiValueConverter.Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values is not [MainWindowViewModel viewModel, Mode mode, ..])
        {
            return DependencyProperty.UnsetValue;
        }
        return mode == Mode.Inventory ? viewModel.InventoryManager : viewModel.StashManager;
    }

    object[] IMultiValueConverter.ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
