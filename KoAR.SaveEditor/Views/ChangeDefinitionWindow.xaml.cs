using System.Windows;
using System.Windows.Input;

namespace KoAR.SaveEditor.Views
{
    partial class ChangeDefinitionWindow
    {
        static ChangeDefinitionWindow() => CommandManager.RegisterClassCommandBinding(typeof(ChangeDefinitionWindow), new CommandBinding(ApplicationCommands.Close, (sender, e) => ((Window)sender).Close()));

        public ChangeDefinitionWindow() => this.InitializeComponent();
    }
}
