using System.Windows;
using System.Windows.Controls;

namespace KoAR.SaveEditor.Constructs
{
    public static class ColumnVisibility
    {
        public static readonly DependencyProperty IsHiddenProperty = DependencyProperty.RegisterAttached("IsHidden", typeof(bool), typeof(ColumnVisibility),
            new(ColumnVisibility.IsHiddenProperty_ValueChanged));

        public static bool GetIsHidden(GridViewColumn column)
        {
            return column != null && (bool)column.GetValue(ColumnVisibility.IsHiddenProperty);
        }

        public static void SetIsHidden(GridViewColumn column, bool value)
        {
            column?.SetValue(ColumnVisibility.IsHiddenProperty, BooleanBoxes.GetBox(value));
        }

        private static void IsHiddenProperty_ValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            GridViewColumn column = (GridViewColumn)d;
            if ((bool)e.NewValue)
            {
                column.Width = 0d;
            }
            else if (column.Width == 0d)
            {
                column.Width = double.NaN;
            }
        }
    }
}
