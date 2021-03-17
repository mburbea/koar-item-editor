using System;
using System.Windows.Input;
using KoAR.SaveEditor.Properties;
using TaskDialogInterop;

namespace KoAR.SaveEditor.Views.Main
{
    partial class MainWindow
    {
        public MainWindow() => this.InitializeComponent();

        private MainWindowViewModel ViewModel => (MainWindowViewModel)this.DataContext;

        protected override void OnClosed(EventArgs e)
        {
            Settings.Default.Save();
            base.OnClosed(e);
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Escape && this.ZoomPopup.IsOpen)
            {
                this.ZoomPopup.IsOpen = false;
                e.Handled = true;
            }
            base.OnPreviewKeyDown(e);
        }

        private void Help_Executed(object sender, ExecutedRoutedEventArgs e) => TaskDialog.Show(new()
        {
            Owner = this,
            Title = $"KoAR Save Editor",
            MainInstruction = "Help",
            MainIcon = VistaTaskDialogIcon.Information,
            CommonButtons = TaskDialogCommonButtons.Close,
            Content = @"1. Your saves are usually not in the same folder as the game.  The editor attemps to make
educated guesses as to the save file directory.

2. When modifying item names, do NOT use special characters.

3. Editing equipped items may cause your file to not load."
        });

        private void Open_Executed(object sender, ExecutedRoutedEventArgs e) => this.ViewModel.OpenFile();

        private void Save_CanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = this.ViewModel.HasUnsavedChanges;

        private void Save_Executed(object sender, ExecutedRoutedEventArgs e) => this.ViewModel.SaveFile();
    }
}
