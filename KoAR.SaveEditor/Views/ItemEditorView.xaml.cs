using System.ComponentModel;
using System.Windows;

namespace KoAR.SaveEditor.Views
{
    partial class ItemEditorView
    {
        public ItemEditorView()
        {
            this.InitializeComponent();
            this.Loaded += this.ItemEditorView_Loaded;
        }

        private void ItemEditorView_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= this.ItemEditorView_Loaded;
            ItemEditorViewModel viewModel = (ItemEditorViewModel)this.DataContext;
            PropertyChangedEventManager.AddHandler(viewModel, this.ViewModel_ReadOnlyPropertyChanged, nameof(viewModel.ReadOnly));
        }

        private void ViewModel_ReadOnlyPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            this.PART_TextBox.Focus();
            this.PART_TextBox.SelectAll();
            ItemEditorViewModel viewModel = (ItemEditorViewModel)this.DataContext;
            PropertyChangedEventManager.RemoveHandler(viewModel, this.ViewModel_ReadOnlyPropertyChanged, nameof(viewModel.ReadOnly));
        }
    }
}
