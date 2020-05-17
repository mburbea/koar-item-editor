using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace KoAR.SaveEditor.Views
{
    partial class MainView
    {
        static MainView() => CommandManager.RegisterClassCommandBinding(typeof(MainView), new CommandBinding(ApplicationCommands.Help, MainView.DisplayHelp));

        public MainView() => this.InitializeComponent();

        protected override void OnClosing(CancelEventArgs e)
        {
            MainViewModel viewModel = (MainViewModel)this.DataContext;
            if (viewModel.UnsavedChanges == true && MessageBox.Show("There are unsaved changes.  Quit without saving?", "KoAR Save Item Editor", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.No)
            {
                e.Cancel = true;
            }
            base.OnClosing(e);
        }

        private static void DisplayHelp(object sender, ExecutedRoutedEventArgs e)
        {
            HelpWindow window = new HelpWindow { Owner = (MainView)sender };
            window.ShowDialog();
            e.Handled = true;
        }

        private void AutoSizeColumns_Click(object sender, RoutedEventArgs e)
        {
            foreach (GridViewColumn column in ((GridView)this.PART_ListView.View).Columns)
            {
                if (double.IsNaN(column.Width))
                {
                    column.Width = column.ActualWidth;
                }
                column.Width = double.NaN;
            }
        }

        private void CheckBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ItemModel model = (ItemModel)((FrameworkElement)sender).DataContext;
            ListViewItem item = (ListViewItem)this.PART_ListView.ItemContainerGenerator.ContainerFromItem(model);
            item.IsSelected = true;
        }
    }
}
