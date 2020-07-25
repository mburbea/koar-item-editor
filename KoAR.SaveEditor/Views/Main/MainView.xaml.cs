using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using KoAR.SaveEditor.Properties;
using TaskDialogInterop;

namespace KoAR.SaveEditor.Views.Main
{
    partial class MainView
    {
        static MainView() => CommandManager.RegisterClassCommandBinding(typeof(MainView), new CommandBinding(ApplicationCommands.Help, MainView.DisplayHelp));

        public MainView()
        {
            this.InitializeComponent();
            this.Loaded += this.Window_Loaded;
        }

        protected override void OnClosed(EventArgs e)
        {
            Settings.Default.Save();
            base.OnClosed(e);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            e.Cancel = ((MainViewModel)this.DataContext).CancelDueToUnsavedChanges(
                "Quit without saving.",
                "Save before closing.\nFile will be saved and then the application will close.",
                "Application will not close."
            );
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
                Content = @"1. Your saves are usually not in the same folder as the game.  In Windows 7+, they can be in C:\Program Files(x86)\Steam\userdata\<user_id>\102500\remote\.

2. When modifying item names, do NOT use special characters.

3. Editing unique items or adding properties beyond the maximum detected will cause your file to not load."
            });
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= this.Window_Loaded;
            this.Dispatcher.InvokeAsync(((MainViewModel)this.DataContext).OpenFile, DispatcherPriority.Render);
        }
    }
}
