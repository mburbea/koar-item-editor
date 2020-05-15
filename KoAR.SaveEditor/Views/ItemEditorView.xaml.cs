using System.Windows;
using System.Windows.Input;

namespace KoAR.SaveEditor.Views
{
    partial class ItemEditorView
    {
        static ItemEditorView() => CommandManager.RegisterClassCommandBinding(typeof(ItemEditorView), new CommandBinding(ApplicationCommands.Close, (sender, e) => ((ItemEditorView)sender).Close()));

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
