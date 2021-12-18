using System.Windows;
using System.Windows.Input;

namespace KoAR.SaveEditor.Constructs;

public static class CommandMethods
{
    public static void TryExecute(this ICommand command, object? parameter = null, IInputElement? target = null)
    {
        switch (command)
        {
            case RoutedCommand routedCommand:
                if (routedCommand.CanExecute(parameter, target))
                {
                    routedCommand.Execute(parameter, target);
                }
                break;
            default:
                if (command.CanExecute(parameter))
                {
                    command.Execute(parameter);
                }
                break;
        }
    }
}
