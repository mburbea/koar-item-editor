using System.Windows.Input;

namespace KoAR.SaveEditor.Views
{
    partial class MainView
    {
        static MainView() => CommandManager.RegisterClassCommandBinding(typeof(MainView), new CommandBinding(ApplicationCommands.Help, MainView.DisplayHelp));

        public MainView() => this.InitializeComponent();

        private static void DisplayHelp(object sender, ExecutedRoutedEventArgs e)
        {
            HelpWindow window = new HelpWindow { Owner = (MainView)sender };
            window.ShowDialog();
            e.Handled = true;
        }
    }
}
