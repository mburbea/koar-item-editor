using System.Windows;
using System.Windows.Input;

namespace KoAR.SaveEditor.Views.ChangeOrAddItem;

partial class ChangeOrAddItemView
{
    static ChangeOrAddItemView() => CommandManager.RegisterClassCommandBinding(typeof(ChangeOrAddItemView), new(ApplicationCommands.Close, (sender, e) => ((Window)sender).Close()));

    public ChangeOrAddItemView() => this.InitializeComponent();

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key != Key.Escape)
        {
            base.OnKeyDown(e);
            return;
        }
        this.Close();
    }
}
