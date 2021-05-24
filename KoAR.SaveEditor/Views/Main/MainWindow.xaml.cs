using System;
using System.Windows.Input;
using KoAR.SaveEditor.Properties;

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

        private void Help_Executed(object sender, ExecutedRoutedEventArgs e) => this.ViewModel.ShowHelp();

        private void Open_Executed(object sender, ExecutedRoutedEventArgs e) => this.ViewModel.OpenFile();

        private void Save_CanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = this.ViewModel.HasUnsavedChanges;

        private void Save_Executed(object sender, ExecutedRoutedEventArgs e) => this.ViewModel.SaveFile();
    }
}
