using System.Windows;
using System.Windows.Input;

namespace KoAR.SaveEditor.Views
{
    partial class ChangeOrAddItemWindow
    {
        static ChangeOrAddItemWindow() => CommandManager.RegisterClassCommandBinding(typeof(ChangeOrAddItemWindow), new CommandBinding(ApplicationCommands.Close, (sender, e) => ((Window)sender).Close()));

        public ChangeOrAddItemWindow() => this.InitializeComponent();

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                this.Dispatcher.InvokeAsync(this.Close);
            }
            base.OnKeyDown(e);
        }
    }
}
