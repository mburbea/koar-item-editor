using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace KoAR.SaveEditor.Views.Updates
{
    partial class UpdateWindow
    {
        public UpdateWindow() => this.InitializeComponent();

        protected override void OnClosing(CancelEventArgs e)
        {
            UpdateViewModelBase viewModel = (UpdateViewModelBase)this.DataContext;
            PropertyChangedEventManager.RemoveHandler(viewModel, this.ViewModel_DialogResultChanged, nameof(viewModel.DialogResult));
            base.OnClosing(e);
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.Property == FrameworkElement.DataContextProperty && e.NewValue is UpdateViewModelBase viewModel)
            {
                PropertyChangedEventManager.AddHandler(viewModel, this.ViewModel_DialogResultChanged, nameof(viewModel.DialogResult));
            }
            base.OnPropertyChanged(e);
        }

        private void RichTextBox_Loaded(object sender, RoutedEventArgs e) => ((RichTextBox)sender).Document = ((UpdateViewModelBase)this.DataContext).Document;

        private void ViewModel_DialogResultChanged(object sender, PropertyChangedEventArgs e)
        {
            this.DialogResult = ((UpdateViewModelBase)sender).DialogResult;
            this.Close();
        }
    }
}
