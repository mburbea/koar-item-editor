using System.Windows;
using System.Windows.Input;

namespace KoAR.SaveEditor.Views
{
    partial class ItemEditorWindow
    {
        static ItemEditorWindow() => CommandManager.RegisterClassCommandBinding(typeof(ItemEditorWindow), new CommandBinding(ApplicationCommands.Close, (sender, e) => ((ItemEditorWindow)sender).Close()));

        public ItemEditorWindow()
        {
            this.InitializeComponent();
            this.Loaded += this.Window_Loaded;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= this.Window_Loaded;
            this.PART_TextBox.Focus();
            this.PART_TextBox.SelectAll();
        }
    }
}
