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
            this.PART_TextBox.Focus();
            this.PART_TextBox.SelectAll();
        }
    }
}
