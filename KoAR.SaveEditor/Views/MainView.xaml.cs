using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using TaskDialogInterop;

namespace KoAR.SaveEditor.Views
{
    partial class MainView
    {
        static MainView() => CommandManager.RegisterClassCommandBinding(typeof(MainView), new CommandBinding(ApplicationCommands.Help, MainView.DisplayHelp));

        public MainView()
        {
            this.InitializeComponent();
            this.Loaded += this.MainView_Loaded;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            MainViewModel viewModel = (MainViewModel)this.DataContext;
            if (viewModel.UnsavedChanges == true)
            {
                TaskDialogResult result = TaskDialog.Show(new TaskDialogOptions
                {
                    MainInstruction = "Unsaved Changes Detected!",
                    Content = "Changed were made to the equipment that have not been saved.",
                    Owner = this,
                    CommandButtons = new[]
                    {
                        "Quit without saving.",
                        "Save before closing.\nFile will be saved and then the application will close.",
                        "Cancel.\nApplication will not close."
                    },
                    DefaultButtonIndex = 0,
                    Title = "KoAR Save Editor",
                    MainIcon = VistaTaskDialogIcon.Warning,
                    AllowDialogCancellation = true,
                    FooterText = " "
                });
                switch (result.CommandButtonResult)
                {
                    case null:
                    case 2:
                        e.Cancel = true;
                        break;
                    case 1:
                        viewModel.SaveCommand.Execute();
                        break;
                }
            }
            base.OnClosing(e);
        }

        private static void DisplayHelp(object sender, ExecutedRoutedEventArgs e)
        {
            TaskDialog.Show(new TaskDialogOptions
            {
                Owner = (Window)sender,
                Title = "KoAR Save Editor",
                MainInstruction = "Help",
                MainIcon = VistaTaskDialogIcon.Information,
                CommonButtons = TaskDialogCommonButtons.Close,
                Content = @"1. It is highly suggested to use this with as few items in your inventory as possible OR to damage your item's durability to an identifiable degree for better item identification if the item is not crafted.

2.Your saves are usually not in the same folder as the game.  In Windows 7+, they can be in C:\Program Files(x86)\Steam\userdata\<user_id>\102500\remote\.

3.When modifying item names, do NOT use special characters.

4.Items that are not crafted will show as ""Unknown"".  Edit these items at your own risk.

5.This is best used with crafted items that have a lot of properties, i.e.lots of flawless components.

6.You can only add as many as you can delete. For example, if the editor says you have 4 attributes(despite the item having more in-game), you can only delete the 4 and add up to 4.

7.Editing unique items or adding properties beyond the maximum detected will cause your file to not load.

8.The provided property.xml file is exhaustive but not complete. You can play around with in-game properties and make educated guesses on which attributes the unknown properties are by way of deleting and tracking.",
                FooterText = "While tricky, using the Hex Code edit can hypothetically give you any item with any property. It's unknown if this can bypass the unique item rule or the save corruption in 7.",
                FooterIcon = VistaTaskDialogIcon.Warning
            });
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

        private void MainView_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= this.MainView_Loaded;
            this.Dispatcher.InvokeAsync(((MainViewModel)this.DataContext).OpenFileCommand.Execute, DispatcherPriority.Render);
        }
    }
}
