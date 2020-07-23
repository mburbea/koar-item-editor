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
            MainViewModel viewModel = (MainViewModel)this.DataContext;
            if (viewModel.UnsavedChanges)
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
                    FooterText = " " // Dialog looks a bit weird without a footer.
                });
                switch (result.CommandButtonResult)
                {
                    case null:
                    case 2:
                        e.Cancel = true;
                        break;
                    case 1:
                        viewModel.Save();
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
