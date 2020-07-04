using System.Windows;
using System.Windows.Input;

namespace KoAR.SaveEditor.Views
{
    partial class ChangeOrAddItemWindow
    {
        static ChangeOrAddItemWindow() => CommandManager.RegisterClassCommandBinding(typeof(ChangeOrAddItemWindow), new CommandBinding(ApplicationCommands.Close, (sender, e) => ((Window)sender).Close()));

        public ChangeOrAddItemWindow() => this.InitializeComponent();
    }
}
