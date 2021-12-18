using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace KoAR.SaveEditor.Constructs;

public static class ListViewAutoSize
{
    public static readonly RoutedUICommand AutoSizeCommand = new("AutoSize Columns", nameof(ListViewAutoSize.AutoSizeCommand), typeof(ListViewAutoSize));

    public static readonly DependencyProperty SkipAutoSizeProperty = DependencyProperty.RegisterAttached("SkipAutoSize", typeof(bool), typeof(ListViewAutoSize),
        new PropertyMetadata(BooleanBoxes.False));

    static ListViewAutoSize() => CommandManager.RegisterClassCommandBinding(typeof(ListView), new(ListViewAutoSize.AutoSizeCommand, ListViewAutoSize.AutoSizeCommand_Executed));

    public static bool GetSkipAutoSize(GridViewColumn column) => column != null && (bool)column.GetValue(ListViewAutoSize.SkipAutoSizeProperty);

    public static void SetSkipAutoSize(GridViewColumn column, bool value) => column?.SetValue(ListViewAutoSize.SkipAutoSizeProperty, BooleanBoxes.GetBox(value));

    public static void AutoSizeColumns(ListView? listView)
    {
        if (listView?.View is GridView view)
        {
            ListViewAutoSize.AutoSizeColumns(view);
        }
    }

    private static void AutoSizeColumns(GridView view)
    {
        foreach (GridViewColumn column in view.Columns)
        {
            if (ListViewAutoSize.GetSkipAutoSize(column) || column.Width == 0d)
            {
                continue;
            }
            if (double.IsNaN(column.Width))
            {
                column.Width = column.ActualWidth;
            }
            column.Width = double.NaN;
        }
    }

    private static void AutoSizeCommand_Executed(object sender, ExecutedRoutedEventArgs e) => ListViewAutoSize.AutoSizeColumns((ListView)sender);
}
