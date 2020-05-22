using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace KoAR.SaveEditor.Constructs
{
    public static class ListViewAutoSize
    {
        public static readonly RoutedUICommand AutoSizeCommand = new RoutedUICommand("AutoSize Columns", nameof(ListViewAutoSize.AutoSizeCommand), typeof(ListViewAutoSize));

        public static readonly DependencyProperty SkipAutoSizeProperty = DependencyProperty.RegisterAttached("SkipAutoSize", typeof(bool), typeof(ListViewAutoSize),
            new PropertyMetadata(BooleanBoxes.False));

        static ListViewAutoSize() => CommandManager.RegisterClassCommandBinding(typeof(ListView), new CommandBinding(ListViewAutoSize.AutoSizeCommand, ListViewAutoSize.AutoSizeCommand_Executed));

        public static bool GetSkipAutoSize(GridViewColumn column) => column != null && (bool)column.GetValue(ListViewAutoSize.SkipAutoSizeProperty);

        public static void SetSkipAutoSize(GridViewColumn column, bool value) => column?.SetValue(ListViewAutoSize.SkipAutoSizeProperty, BooleanBoxes.GetBox(value));

        private static void AutoSizeColumns(GridView view)
        {
            foreach (GridViewColumn column in view.Columns)
            {
                if (ListViewAutoSize.GetSkipAutoSize(column))
                {
                    continue;
                }
                if (!double.IsNaN(column.Width))
                {
                    column.Width = column.ActualWidth;
                }
                column.Width = double.NaN;
            }
        }

        private static void AutoSizeCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (((ListView)sender).View is GridView view)
            {
                ListViewAutoSize.AutoSizeColumns(view);
            }
        }
    }
}
